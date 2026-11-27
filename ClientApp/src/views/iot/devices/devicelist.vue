<template>
	<div class="device-list-page console-list-page">
		<ConsolePageShell
			eyebrow="Device Workspace"
			title="设备接入与清单"
			description="查看设备连接状态、设备详情，并向选中的设备委托规则。"
		>
			<section class="device-list-page__card console-list-panel" aria-labelledby="device-table-title">
				<form class="device-query" aria-label="筛选设备" @submit.prevent="applyFilters(draftFilters)">
					<div class="device-query__field">
						<label for="device-query-name">设备名称或 ID</label>
						<el-input
							id="device-query-name"
							v-model="draftFilters.name"
							placeholder="输入名称或完整 ID"
							:prefix-icon="Search"
							:disabled="busy"
							clearable
						/>
					</div>
					<el-checkbox v-model="draftFilters.connected" :disabled="busy">仅在线</el-checkbox>
					<div class="device-query__actions">
						<el-button type="primary" native-type="submit" :loading="submitting" :disabled="busy">查询</el-button>
						<el-button :disabled="busy || !hasFiltersToClear" @click="applyFilters(emptyFilters)">重置</el-button>
						<el-button :icon="Refresh" :disabled="busy" @click="refreshDevices">刷新</el-button>
					</div>
				</form>

				<div v-if="hasAppliedFilters" class="device-query__applied" role="group" aria-label="已应用的筛选条件">
					<span>已应用</span>
					<button
						v-if="deviceOverview.appliedFilters.name"
						type="button"
						class="device-query__chip"
						:disabled="busy"
						:aria-label="'移除设备名称或 ID 条件 ' + deviceOverview.appliedFilters.name"
						@click="applyFilters({ ...deviceOverview.appliedFilters, name: '' })"
					>
						<span>名称或 ID {{ deviceOverview.appliedFilters.name }}</span>
						<el-icon aria-hidden="true"><Close /></el-icon>
					</button>
					<button
						v-if="deviceOverview.appliedFilters.connected"
						type="button"
						class="device-query__chip"
						:disabled="busy"
						aria-label="移除仅在线条件"
						@click="applyFilters({ ...deviceOverview.appliedFilters, connected: false })"
					>
						<span>仅在线</span>
						<el-icon aria-hidden="true"><Close /></el-icon>
					</button>
				</div>
				<p v-if="queryFailed" class="device-query__error" role="alert">设备列表加载失败，请重试。</p>

				<header class="device-list-page__header">
					<h2 id="device-table-title">设备列表</h2>
					<p v-if="deviceOverview.loaded" role="status">
						共 {{ deviceOverview.total }} 台 · 本页在线 {{ deviceOverview.onlineCount }} 台
					</p>
				</header>
				<div class="console-list-grid" :aria-busy="busy">
					<fs-crud ref="crudRef" v-bind="crudBinding">
						<template #actionbar-right>
							<div v-if="selectedItems.length" class="device-table__selection">
								<span role="status">已选 {{ selectedItems.length }} 台</span>
								<el-button size="small" :disabled="busy" @click="openCustomForm">批量委托规则</el-button>
								<el-button size="small" text :disabled="busy" @click="clearSelection">取消选择</el-button>
							</div>
						</template>
					</fs-crud>
				</div>
			</section>
		</ConsolePageShell>

		<DeviceDetail ref="deviceDetailRef" />
		<AddRules ref="addRulesRef" />
	</div>
</template>

<script lang="ts" setup>
import { computed, onMounted, reactive, ref } from 'vue';
import { Close, Refresh, Search } from '@element-plus/icons-vue';
import { useCrud, useExpose } from '@fast-crud/fast-crud';
import { useRoute } from 'vue-router';
import { storeToRefs } from 'pinia';
import ConsolePageShell from '/@/components/console/ConsolePageShell.vue';
import { useUserInfo } from '/@/stores/userInfo';
import AddRules from './addRules.vue';
import DeviceDetail from './DeviceDetail.vue';
import { createDeviceCrudOptions, type DeviceListFilters, type DeviceListOverview } from './deviceCrudOptions';
import type { TableDataRow } from './model';

const selectedItems = ref<TableDataRow[]>([]);
const route = useRoute();
const { userInfos } = storeToRefs(useUserInfo());
const emptyFilters: DeviceListFilters = { name: '', connected: false };
const draftFilters = reactive<DeviceListFilters>({ ...emptyFilters });
const deviceOverview = reactive<DeviceListOverview>({
	total: 0,
	pageCount: 0,
	onlineCount: 0,
	loaded: false,
	appliedFilters: { ...emptyFilters },
});
const submitting = ref(false);
const queryFailed = ref(false);
const deviceDetailRef = ref();
const crudRef = ref();
const crudBinding = ref();
const addRulesRef = ref();
const customerId = computed(() => typeof route.query.id === 'string' ? route.query.id : userInfos.value.customer?.id);

const { crudExpose } = useExpose({ crudRef, crudBinding });
const { crudOptions } = createDeviceCrudOptions(customerId, openDeviceDetails, selectedItems, deviceOverview);
useCrud({ expose: crudExpose, crudOptions });

const busy = computed(() => submitting.value || Boolean(crudBinding.value?.table?.loading));
const hasAppliedFilters = computed(() => Boolean(deviceOverview.appliedFilters.name || deviceOverview.appliedFilters.connected));
const hasFiltersToClear = computed(() => Boolean(hasAppliedFilters.value || draftFilters.name || draftFilters.connected));

function openDeviceDetails(device: TableDataRow) {
	deviceDetailRef.value?.openDialog(device);
}

function openCustomForm() {
	addRulesRef.value.openDialog(selectedItems.value);
}

function clearSelection() {
	crudExpose.getBaseTableRef()?.clearSelection();
}

/** 查询失败时恢复已应用条件和分页，便于继续查看现有结果。 */
async function applyFilters(filters: DeviceListFilters) {
	if (busy.value) return;
	const nextFilters = { name: filters.name.trim(), connected: filters.connected };
	const previousPage = crudExpose.getPage().currentPage;
	submitting.value = true;
	queryFailed.value = false;
	try {
		await crudExpose.doSearch({ form: nextFilters, mergeForm: false });
		Object.assign(draftFilters, nextFilters);
	} catch {
		crudExpose.setSearchFormData({ form: deviceOverview.appliedFilters, mergeForm: false, refWarning: false });
		crudExpose.doPageTurn(previousPage);
		queryFailed.value = true;
	} finally {
		submitting.value = false;
	}
}

async function refreshDevices() {
	if (busy.value) return;
	queryFailed.value = false;
	try {
		await crudExpose.doRefresh();
	} catch {
		queryFailed.value = true;
	}
}

onMounted(refreshDevices);
</script>

<style lang="scss" scoped>
.device-list-page {
	min-width: 0;
}

.device-list-page__card {
	min-width: 0;
	padding: clamp(10px, 1.4vw, 14px);
	border: 1px solid var(--iotsharp-border);
	border-radius: var(--iotsharp-radius-panel);
	background: var(--iotsharp-surface);
	box-shadow: var(--iotsharp-shadow-panel);
}

.device-list-page__header {
	display: flex;
	flex-wrap: wrap;
	align-items: center;
	justify-content: space-between;
	gap: 8px 12px;
	margin-block-start: 10px;

	h2 {
		margin: 0;
		color: var(--iotsharp-text);
		font-size: 17px;
	}

	p {
		margin: 0;
		color: var(--iotsharp-text-soft);
		font-size: 12px;
	}
}

.device-query {
	display: flex;
	flex-wrap: wrap;
	align-items: end;
	gap: 8px 14px;
	padding: 8px 10px;
	border: 1px solid var(--iotsharp-border);
	border-radius: var(--iotsharp-radius-control);
	background: var(--iotsharp-surface-muted);
}

.device-query__field {
	display: grid;
	flex: 1 1 240px;
	gap: 4px;
	min-width: 0;
	max-width: 360px;
	color: var(--iotsharp-text-soft);
	font-size: 12px;
	font-weight: 600;
}

.device-query__actions,
.device-table__selection,
.device-query__applied {
	display: flex;
	flex-wrap: wrap;
	align-items: center;
	gap: 6px;
}

.device-query__applied,
.device-query__error {
	margin: 8px 0 0;
	color: var(--iotsharp-text-soft);
	font-size: 12px;
}

.device-query__error {
	color: var(--iotsharp-danger);
}

.device-query__chip {
	display: inline-flex;
	align-items: center;
	gap: 5px;
	min-width: 0;
	max-width: 100%;
	min-height: 26px;
	padding: 3px 8px;
	border: 1px solid var(--iotsharp-border);
	border-radius: var(--iotsharp-radius-small);
	background: var(--iotsharp-data-surface, var(--iotsharp-selection));
	color: var(--iotsharp-data-accent, var(--iotsharp-accent));
	font: inherit;
	cursor: pointer;

	span {
		overflow-wrap: anywhere;
	}

	.el-icon {
		flex: none;
	}

	&:hover {
		border-color: currentColor;
	}
}

.device-table__selection {
	padding: 3px 6px 3px 8px;
	border-radius: var(--iotsharp-radius-control);
	background: var(--iotsharp-data-surface, var(--iotsharp-selection));
	color: var(--iotsharp-data-accent, var(--iotsharp-accent));
	font-size: 12px;
	font-weight: 600;
}

.device-query__actions,
.device-table__selection {
	.el-button + .el-button {
		margin-inline-start: 0;
	}
}

:deep(.device-table__name) {
	display: inline-block;
	max-width: 100%;
	padding: 2px 0;
	overflow: hidden;
	border: 0;
	border-radius: var(--iotsharp-radius-small);
	background: none;
	color: var(--iotsharp-data-accent, var(--iotsharp-accent));
	font: inherit;
	font-weight: 650;
	text-overflow: ellipsis;
	vertical-align: middle;
	white-space: nowrap;
	cursor: pointer;

	&:hover {
		text-decoration: underline;
	}
}

.device-list-page__card :deep(.fs-crud-table) {
	min-height: 0;
}

.device-list-page__card :deep(.fs-crud-search) {
	display: none;
}

@media (max-width: 650px) {
	.device-query__field {
		flex-basis: 100%;
		max-width: none;
	}
}
</style>
