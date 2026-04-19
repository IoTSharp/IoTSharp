<template>
	<div class="edge-list-page">
		<ConsolePageShell
			eyebrow="Edge Workspace"
			title="EdgeNode 管理"
			description="第一版只覆盖 EdgeNode 管理页、查询筛选与详情展示，用统一工作台承接 Gateway 与 PiXiu 的节点查询视图。"
			:badges="edgeBadges"
			:metrics="edgeMetrics"
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

			<div class="edge-list-page__card">
				<div class="edge-list-page__card-head">
					<div>
						<div class="edge-list-page__card-eyebrow">Edge Table</div>
						<h3>EdgeNode 列表</h3>
						<p>支持名称、运行时、健康状态、活跃状态、版本与平台筛选，便于快速定位需要查看的 Edge 节点。</p>
					</div>
					<div class="edge-list-page__selection">
						<span>当前页节点</span>
						<strong>{{ overview.pageCount }}</strong>
					</div>
				</div>

				<div class="edge-list-page__crud">
					<fs-crud ref="crudRef" v-bind="crudBinding">
						<template #actionbar-right>
							<div class="edge-list-page__actionbar">
								<span class="edge-list-page__actionbar-tag">当前页 {{ overview.pageCount }} 个</span>
								<span class="edge-list-page__actionbar-tag is-primary">健康 {{ overview.healthyCount }} 个</span>
								<span class="edge-list-page__actionbar-tag is-success">活跃 {{ overview.activeCount }} 个</span>
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
					title="第一版边缘管理先创建 Gateway 身份；实际 runtimeType 在 Register 阶段由 Gateway 或 PiXiu 自行声明。"
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
							<el-option label="PiXiu" value="pixiu" />
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

const edgeBadges = computed(() => [
	`节点总数 ${overview.total}`,
	`当前页 ${overview.pageCount}`,
	overview.lastRefresh ? `最近同步 ${overview.lastRefresh}` : '等待首次同步',
]);

const edgeMetrics = computed(() => [
	{ label: '节点总数', value: overview.total, hint: '符合当前筛选条件的 Edge 节点总数。', tone: 'primary' as const },
	{ label: '当前页健康', value: overview.healthyCount, hint: '当前页 healthy=true 的节点数量。', tone: 'success' as const },
	{ label: '当前页活跃', value: overview.activeCount, hint: '当前页 active=true 的节点数量。', tone: 'accent' as const },
	{ label: '详情能力', value: '已启用', hint: '支持查看 capabilities、metadata、metrics。', tone: 'warning' as const },
]);

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
	display: flex;
	flex-direction: column;
	gap: 18px;
}

.edge-list-page__action-wrap {
	display: inline-flex;
}

.edge-list-page__card {
	padding: 20px 22px;
	border-radius: 28px;
	border: 1px solid rgba(226, 232, 240, 0.92);
	background: linear-gradient(180deg, rgba(248, 251, 255, 0.96), rgba(255, 255, 255, 0.98));
	box-shadow: 0 18px 42px rgba(15, 23, 42, 0.05);
}

.edge-list-page__card-head,
.edge-list-page__actionbar {
	display: flex;
	align-items: center;
	justify-content: space-between;
	gap: 16px;
}

.edge-list-page__card-eyebrow {
	margin-bottom: 10px;
	color: #2563eb;
	font-size: 12px;
	font-weight: 700;
	letter-spacing: 0.16em;
	text-transform: uppercase;
}

.edge-list-page__card-head h3 {
	margin: 0;
	color: #123b6d;
	font-size: 22px;
	letter-spacing: -0.04em;
}

.edge-list-page__card-head p {
	margin: 10px 0 0;
	color: #64748b;
	font-size: 13px;
	line-height: 1.75;
}

.edge-list-page__selection {
	display: flex;
	flex-direction: column;
	align-items: flex-end;
	min-width: 120px;
	padding: 14px 16px;
	border-radius: 20px;
	border: 1px solid rgba(191, 219, 254, 0.9);
	background: rgba(255, 255, 255, 0.78);
}

.edge-list-page__selection span {
	color: #64748b;
	font-size: 12px;
}

.edge-list-page__selection strong {
	margin-top: 8px;
	color: #123b6d;
	font-size: 28px;
	line-height: 1;
}

.edge-list-page__crud {
	margin-top: 18px;
	min-height: 520px;
	position: relative;
	padding-bottom: 12px;
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
	border-radius: 999px;
	background: rgba(226, 232, 240, 0.9);
	color: #334155;
	font-size: 12px;
	font-weight: 600;
}

.edge-list-page__actionbar-tag.is-primary {
	background: rgba(191, 219, 254, 0.9);
	color: #1d4ed8;
}

.edge-list-page__actionbar-tag.is-success {
	background: rgba(187, 247, 208, 0.9);
	color: #15803d;
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

:deep(.fs-crud .fs-crud-table) {
	min-height: 260px;
	height: auto;
}

.edge-create-dialog {
	display: flex;
	flex-direction: column;
	gap: 18px;
}

.edge-create-dialog__form {
	margin-top: 6px;
}

@media (max-width: 767px) {
	.edge-list-page__card {
		padding: 18px;
		border-radius: 22px;
	}

	.edge-list-page__card-head,
	.edge-list-page__selection {
		align-items: flex-start;
	}

	.edge-list-page__card-head {
		flex-direction: column;
	}

	.edge-list-page__selection {
		width: 100%;
	}

	.edge-list-page__crud {
		min-height: auto;
	}
}
</style>
