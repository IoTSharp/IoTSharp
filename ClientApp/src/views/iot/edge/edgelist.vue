<template>
	<div class="edge-list-page console-list-page">
		<ConsolePageShell
			eyebrow="Edge Workspace"
			title="EdgeNode 管理"
			description="查看边缘节点的版本、活跃状态和健康检查结果。"
		>
			<template #actions>
				<el-button type="primary" @click="refreshEdges">刷新列表</el-button>
				<el-tooltip
					:content="canCreateEdge ? '创建 Gateway 身份并作为边缘运行时接入平台' : '当前账号缺少 CustomerAdmin 权限，暂时不能创建设备身份'"
					placement="bottom"
				>
					<span class="edge-list-page__action-wrap">
						<el-button plain :disabled="!canCreateEdge" @click="openCreateDialog">新增边缘节点</el-button>
					</span>
				</el-tooltip>
			</template>

			<div class="edge-list-page__card console-list-panel">
				<div class="edge-list-page__card-head">
					<h2>EdgeNode 列表</h2>
					<span v-if="overview.lastRefresh" class="edge-list-page__summary">共 {{ overview.total }} 条</span>
				</div>

				<div class="edge-list-page__crud console-list-grid">
					<fs-crud ref="crudRef" v-bind="crudBinding">
						<template #actionbar-right>
							<div class="edge-list-page__actionbar">
								<span class="edge-list-page__actionbar-tag is-success">健康 {{ overview.healthyCount }} 个</span>
								<span class="edge-list-page__actionbar-tag is-primary">活跃 {{ overview.activeCount }} 个</span>
							</div>
						</template>
					</fs-crud>
				</div>
			</div>
		</ConsolePageShell>

		<EdgeDetail ref="edgeDetailRef" />
		<EdgeOnboardingDialog ref="onboardingRef" />

		<el-dialog v-model="createDialog" title="新增边缘节点" width="640px" destroy-on-close>
			<div class="edge-create-dialog">
				<el-alert
					title="第一版边缘管理先创建 Gateway 身份；实际 runtimeType 在 Register 阶段由 Gateway 自行声明。"
					type="info"
					:closable="false"
					show-icon
				/>

				<el-form label-width="120px" class="edge-create-dialog__form">
					<el-form-item label="节点名称" required>
						<el-input v-model="createForm.name" maxlength="100" placeholder="例如：Hangzhou-Plant-GW-01" />
					</el-form-item>
					<el-form-item label="运行时模板">
						<el-select v-model="createForm.runtimeType" style="width: 100%">
							<el-option label="Gateway" value="gateway" />
						</el-select>
					</el-form-item>
					<el-form-item label="超时(秒)">
						<el-input-number v-model="createForm.timeout" :min="30" :max="86400" style="width: 100%" />
					</el-form-item>
				</el-form>
			</div>

			<template #footer>
				<el-button @click="createDialog = false">取消</el-button>
				<el-button type="primary" :loading="creating" @click="submitCreateEdge">创建并查看接入信息</el-button>
			</template>
		</el-dialog>
	</div>
</template>

<script setup lang="ts">
import { computed, nextTick, onMounted, reactive, ref } from 'vue';
import { storeToRefs } from 'pinia';
import { useCrud, useExpose } from '@fast-crud/fast-crud';
import { ElMessage } from 'element-plus';
import ConsolePageShell from '/@/components/console/ConsolePageShell.vue';
import { useUserInfo } from '/@/stores/userInfo';
import { edgeApi } from '/@/api/edge';
import EdgeDetail from './EdgeDetail.vue';
import EdgeOnboardingDialog from './EdgeOnboardingDialog.vue';
import { createEdgeCrudOptions } from './edgeCrudOptions';

const overview = reactive({
	total: 0,
	pageCount: 0,
	healthyCount: 0,
	activeCount: 0,
	lastRefresh: '',
});

const edgeDetailRef = ref();
const onboardingRef = ref();
const crudRef = ref();
const crudBinding = ref();
const createDialog = ref(false);
const creating = ref(false);
const createForm = reactive({
	name: '',
	runtimeType: 'gateway',
	timeout: 300,
});

const userInfoStore = useUserInfo();
const { userInfos } = storeToRefs(userInfoStore);
const { crudExpose } = useExpose({ crudRef, crudBinding });
const { crudOptions } = createEdgeCrudOptions(
	{
		expose: crudExpose,
		openOnboarding(edge) {
			onboardingRef.value?.openDialog(edge, edge?.runtimeType || 'gateway');
		},
	},
	edgeDetailRef,
	overview
);

// eslint-disable-next-line @typescript-eslint/no-unused-vars,no-unused-vars
useCrud({ crudExpose, crudOptions });

const roleBag = computed(() => String(userInfos.value?.roles?.[0] ?? '').toLowerCase());
const canCreateEdge = computed(() => roleBag.value.includes('customeradmin'));

const refreshEdges = () => {
	crudExpose.doRefresh();
};

const openCreateDialog = () => {
	createForm.name = '';
	createForm.runtimeType = 'gateway';
	createForm.timeout = 300;
	createDialog.value = true;
};

const submitCreateEdge = async () => {
	if (!createForm.name.trim()) {
		ElMessage.warning('节点名称不能为空');
		return;
	}

	creating.value = true;
	try {
		const response = await edgeApi().createEdgeNode({
			name: createForm.name.trim(),
			timeout: createForm.timeout,
			deviceType: 'Gateway',
			identityType: 'AccessToken',
		});

		const createdEdge = response.data ?? {};
		createDialog.value = false;
		ElMessage.success('边缘节点已创建');
		refreshEdges();
		await nextTick();
		onboardingRef.value?.openDialog(
			{
				id: createdEdge.id,
				name: createdEdge.name || createForm.name.trim(),
				runtimeType: createForm.runtimeType,
			},
			createForm.runtimeType
		);
	}
	catch (error: any) {
		const message = error?.response?.data?.msg || error?.message || '边缘节点创建失败';
		ElMessage.error(message);
	}
	finally {
		creating.value = false;
	}
};

onMounted(() => {
	refreshEdges();
});
</script>

<style lang="scss" scoped>
.edge-list-page {
	--workspace-accent: var(--iotsharp-operation-accent, var(--iotsharp-accent));
	display: flex;
	flex-direction: column;
	gap: 18px;
}

.edge-list-page__action-wrap {
	display: inline-flex;
}

.edge-list-page__card {
	padding: clamp(10px, 1.4vw, 14px);
	border: 1px solid var(--iotsharp-border);
	border-radius: var(--iotsharp-radius-panel);
	background: var(--iotsharp-surface);
	box-shadow: var(--iotsharp-shadow-panel);
}

.edge-list-page__card-head,
.edge-list-page__actionbar {
	display: flex;
	align-items: center;
	justify-content: space-between;
	gap: 16px;
}

.edge-list-page__card-head h2 {
	margin: 0;
	color: var(--iotsharp-text);
	font-size: 17px;
}

.edge-list-page__summary {
	color: var(--iotsharp-text-soft);
	font-size: 12px;
	font-variant-numeric: tabular-nums;
}

.edge-list-page__crud {
	margin-top: 8px;
	position: relative;
}

.edge-list-page__actionbar {
	justify-content: flex-end;
	margin-bottom: 12px;
	flex-wrap: wrap;
}

.edge-list-page__actionbar-tag {
	display: inline-flex;
	align-items: center;
	padding: 8px 12px;
	border-radius: var(--iotsharp-radius-small);
	background: var(--iotsharp-surface-muted);
	color: var(--iotsharp-text-soft);
	font-size: 12px;
	font-weight: 600;
}

.edge-list-page__actionbar-tag.is-primary {
	background: var(--iotsharp-operation-surface, var(--iotsharp-selection));
	color: var(--workspace-accent);
}

.edge-list-page__actionbar-tag.is-success {
	background: var(--iotsharp-success-surface);
	color: var(--iotsharp-success);
}

:deep(.fs-crud) {
	--el-fill-color-blank: transparent;
}

:deep(.fs-crud .el-card) {
	box-shadow: none;
}

:deep(.fs-crud .el-table) {
	border-radius: var(--iotsharp-radius-control);
	overflow: hidden;
}

:deep(.fs-crud .el-pagination) {
	margin-top: 18px;
}

.edge-create-dialog {
	display: flex;
	flex-direction: column;
	gap: 18px;
}

.edge-create-dialog__form {
	margin-top: 6px;
}

</style>
