<template>
	<div class="device-list-page">
		<ConsolePageShell
			eyebrow="Device Workspace"
			title="设备接入与清单"
			description="把设备接入、状态筛选、规则委托和详情查看统一到一个设备工作台里，延续控制台首页的蓝白工作区风格。"
			:badges="deviceBadges"
			:metrics="deviceMetrics"
		>
			<template #actions>
				<el-button type="primary" @click="refreshDevices">刷新列表</el-button>
				<el-button @click="openCustomForm">批量委托规则</el-button>
			</template>

			<div class="device-list-page__card">
				<div class="device-list-page__card-head">
					<div>
						<div class="device-list-page__card-eyebrow">Device Table</div>
						<h3>设备列表</h3>
						<p>支持名称检索、在线状态筛选、批量选择与详情查看，便于快速进入某台设备的工作区。</p>
					</div>
					<div class="device-list-page__selection">
						<span>已选设备</span>
						<strong>{{ selectedItems.length }}</strong>
					</div>
				</div>

				<div class="device-list-page__crud">
					<fs-crud ref="crudRef" v-bind="crudBinding">
						<template #actionbar-right>
							<div class="device-list-page__actionbar">
								<span class="device-list-page__actionbar-tag">当前页 {{ deviceOverview.pageCount }} 台</span>
								<span class="device-list-page__actionbar-tag is-primary">在线 {{ deviceOverview.activeCount }} 台</span>
							</div>
						</template>
					</fs-crud>
				</div>
			</div>
		</ConsolePageShell>

		<DeviceDetail ref="deviceDetailRef"></DeviceDetail>
		<addRules ref="addRulesRef"></addRules>
	</div>
</template>

<script lang="ts" setup>
import { computed, onMounted, reactive, ref } from 'vue';
import { ElMessage } from 'element-plus';
import { useCrud, useExpose } from '@fast-crud/fast-crud';
import { useRoute } from 'vue-router';
import { storeToRefs } from 'pinia';
import ConsolePageShell from '/@/components/console/ConsolePageShell.vue';
import DeviceDetail from './DeviceDetail.vue';
import addRules from './addRules.vue';
import { createDeviceCrudOptions } from '/@/views/iot/devices/deviceCrudOptions';
import { useUserInfo } from '/@/stores/userInfo';

const selectedItems = ref<any[]>([]);
const stores = useUserInfo();
const route = useRoute();
const { userInfos } = storeToRefs(stores);

const deviceOverview = reactive({
	total: 0,
	pageCount: 0,
	activeCount: 0,
	inactiveCount: 0,
	lastRefresh: '',
});

const deviceDetailRef = ref();
const crudRef = ref();
const crudBinding = ref();
const addRulesRef = ref();
const customerId = route.query.id || userInfos.value.customer.id;

const { crudExpose } = useExpose({ crudRef, crudBinding });
const { crudOptions } = createDeviceCrudOptions(
	{ expose: crudExpose },
	customerId,
	deviceDetailRef,
	addRulesRef,
	selectedItems,
	deviceOverview
);

// eslint-disable-next-line @typescript-eslint/no-unused-vars,no-unused-vars
const { resetCrudOptions } = useCrud({ expose: crudExpose, crudOptions });

const shortCustomerId = computed(() => {
	const value = String(customerId || '--');
	return value.length > 12 ? `${value.slice(0, 8)}...` : value;
});

const deviceBadges = computed(() => [
	`客户视图 ${shortCustomerId.value}`,
	`已选 ${selectedItems.value.length} 台`,
	deviceOverview.lastRefresh ? `最近同步 ${deviceOverview.lastRefresh}` : '等待首次同步',
]);

const deviceMetrics = computed(() => [
	{
		label: '设备总数',
		value: deviceOverview.total,
		hint: '当前客户工作区下的设备总量。',
		tone: 'primary' as const,
	},
	{
		label: '当前页在线',
		value: deviceOverview.activeCount,
		hint: '本页处于活动状态的设备数量。',
		tone: 'success' as const,
	},
	{
		label: '当前页静默',
		value: deviceOverview.inactiveCount,
		hint: '本页尚未激活或暂时离线的设备。',
		tone: 'warning' as const,
	},
	{
		label: '规则委托',
		value: selectedItems.value.length,
		hint: '可对选中设备批量挂载规则。',
		tone: 'accent' as const,
	},
]);

const openCustomForm = () => {
	if (selectedItems.value.length) {
		addRulesRef.value.openDialog(selectedItems.value);
		return;
	}

	ElMessage.info('请先选择设备');
};

const refreshDevices = () => {
	crudExpose.doRefresh();
};

onMounted(() => {
	refreshDevices();
});
</script>

<style lang="scss" scoped>
.device-list-page {
	display: flex;
	flex-direction: column;
	gap: 18px;
}

.device-list-page__card {
	padding: 20px 22px;
	border-radius: 28px;
	border: 1px solid rgba(226, 232, 240, 0.92);
	background: linear-gradient(180deg, rgba(248, 251, 255, 0.96), rgba(255, 255, 255, 0.98));
	box-shadow: 0 18px 42px rgba(15, 23, 42, 0.05);
}

.device-list-page__card-head,
.device-list-page__actionbar {
	display: flex;
	align-items: center;
	justify-content: space-between;
	gap: 16px;
}

.device-list-page__card-eyebrow {
	margin-bottom: 10px;
	color: #2563eb;
	font-size: 12px;
	font-weight: 700;
	letter-spacing: 0.16em;
	text-transform: uppercase;
}

.device-list-page__card-head h3 {
	margin: 0;
	color: #123b6d;
	font-size: 22px;
	letter-spacing: -0.04em;
}

.device-list-page__card-head p {
	margin: 10px 0 0;
	color: #64748b;
	font-size: 13px;
	line-height: 1.75;
}

.device-list-page__selection {
	display: flex;
	flex-direction: column;
	align-items: flex-end;
	min-width: 120px;
	padding: 14px 16px;
	border-radius: 20px;
	border: 1px solid rgba(191, 219, 254, 0.9);
	background: rgba(255, 255, 255, 0.78);
}

.device-list-page__selection span {
	color: #64748b;
	font-size: 12px;
}

.device-list-page__selection strong {
	margin-top: 8px;
	color: #123b6d;
	font-size: 28px;
	line-height: 1;
}

.device-list-page__crud {
	margin-top: 18px;
	min-height: 520px;
}

.device-list-page__actionbar {
	flex-wrap: wrap;
	justify-content: flex-end;
}

.device-list-page__actionbar-tag {
	display: inline-flex;
	align-items: center;
	min-height: 32px;
	padding: 0 12px;
	border-radius: 999px;
	border: 1px solid rgba(226, 232, 240, 0.92);
	background: rgba(248, 250, 252, 0.9);
	color: #475569;
	font-size: 12px;
	font-weight: 600;
}

.device-list-page__actionbar-tag.is-primary {
	border-color: rgba(191, 219, 254, 0.92);
	background: rgba(219, 234, 254, 0.8);
	color: #2563eb;
}

:deep(.fs-crud) {
	--el-fill-color-blank: transparent;
}

:deep(.fs-crud .el-card) {
	box-shadow: none;
}

:deep(.fs-crud .el-table) {
	border-radius: 20px;
	overflow: hidden;
}

:deep(.fs-crud .el-table th.el-table__cell) {
	background: #f8fbff;
}

:deep(.fs-crud .el-pagination) {
	margin-top: 18px;
}

@media (max-width: 767px) {
	.device-list-page__card {
		padding: 18px;
		border-radius: 22px;
	}

	.device-list-page__card-head,
	.device-list-page__selection {
		align-items: flex-start;
	}

	.device-list-page__card-head {
		flex-direction: column;
	}

	.device-list-page__selection {
		width: 100%;
	}

	.device-list-page__crud {
		min-height: auto;
	}
}
</style>
