<template>
	<div>
		<el-drawer v-model="drawer" size="72%" class="edge-detail-drawer" append-to-body destroy-on-close>
			<ConsoleDrawerWorkspace
				eyebrow="Edge Detail"
				:title="edgeName"
				description="集中查看 Edge 节点的基础信息、能力声明、元数据和运行指标，作为第一版对接与排障窗口。"
				:badges="badges"
				:metrics="metrics"
			>
				<template #actions>
					<el-button type="primary" @click="reload">刷新</el-button>
					<el-button plain @click="openDispatchDialog">下发任务</el-button>
				</template>

				<section class="edge-detail__overview">
					<article class="edge-card edge-card--main">
						<div class="edge-card__header">
							<div>
								<h3>节点概览</h3>
								<p>展示 Edge 节点第一版管理范围内的基础运行时字段，便于核对注册与心跳状态。</p>
							</div>
						</div>
						<el-descriptions :column="2" border>
							<el-descriptions-item label="Name">{{ edgeRef.name || '--' }}</el-descriptions-item>
							<el-descriptions-item label="RuntimeType">{{ edgeRef.runtimeType || '--' }}</el-descriptions-item>
							<el-descriptions-item label="RuntimeName">{{ edgeRef.runtimeName || '--' }}</el-descriptions-item>
							<el-descriptions-item label="Version">{{ edgeRef.version || '--' }}</el-descriptions-item>
							<el-descriptions-item label="Status">{{ edgeRef.status || '--' }}</el-descriptions-item>
							<el-descriptions-item label="Healthy">{{ formatBoolean(edgeRef.healthy) }}</el-descriptions-item>
							<el-descriptions-item label="Active">{{ formatBoolean(edgeRef.active) }}</el-descriptions-item>
							<el-descriptions-item label="Platform">{{ edgeRef.platform || '--' }}</el-descriptions-item>
							<el-descriptions-item label="HostName">{{ edgeRef.hostName || '--' }}</el-descriptions-item>
							<el-descriptions-item label="IpAddress">{{ edgeRef.ipAddress || '--' }}</el-descriptions-item>
							<el-descriptions-item label="LastHeartbeatDateTime">{{ formatDateTime(edgeRef.lastHeartbeatDateTime) }}</el-descriptions-item>
							<el-descriptions-item label="LastActivityDateTime">{{ formatDateTime(edgeRef.lastActivityDateTime) }}</el-descriptions-item>
						</el-descriptions>
					</article>

					<article class="edge-card">
						<div class="edge-card__header">
							<div>
								<h3>对接提示</h3>
								<p>第一版只覆盖查询、筛选和详情，不承载配置下发与任务执行操作。</p>
							</div>
						</div>
						<ul class="edge-tip-list">
							<li>能力声明用于确认 Gateway 或 PiXiu 的协议与任务支持范围。</li>
							<li>metadata 保留部署与环境上下文，避免塞入高频或大体量运行数据。</li>
							<li>metrics 仅做详情展示，不参与第一版筛选与排序。</li>
						</ul>
					</article>
				</section>

				<section class="edge-json-panels">
					<article class="edge-card">
						<div class="edge-card__header"><h3>Capabilities</h3></div>
						<pre>{{ prettyJson(edgeRef.capabilities) }}</pre>
					</article>
					<article class="edge-card">
						<div class="edge-card__header"><h3>Metadata</h3></div>
						<pre>{{ prettyJson(edgeRef.metadata) }}</pre>
					</article>
					<article class="edge-card">
						<div class="edge-card__header"><h3>Metrics</h3></div>
						<pre>{{ prettyJson(edgeRef.metrics) }}</pre>
					</article>
				</section>

				<section class="edge-receipt-section">
					<article class="edge-card edge-card--main">
						<div class="edge-card__header">
							<div>
								<h3>最近回执</h3>
								<p>展示当前 Edge 节点最近一次任务回执，便于核对 request/receipt 闭环是否打通。</p>
							</div>
						</div>
						<el-empty v-if="!latestReceipt" description="暂无最近回执" />
						<el-descriptions v-else :column="2" border>
							<el-descriptions-item label="TaskId">{{ latestReceipt.taskId || '--' }}</el-descriptions-item>
							<el-descriptions-item label="Status">{{ latestReceipt.status || '--' }}</el-descriptions-item>
							<el-descriptions-item label="TargetType">{{ latestReceipt.targetType || '--' }}</el-descriptions-item>
							<el-descriptions-item label="TargetKey">{{ latestReceipt.targetKey || '--' }}</el-descriptions-item>
							<el-descriptions-item label="RuntimeType">{{ latestReceipt.runtimeType || '--' }}</el-descriptions-item>
							<el-descriptions-item label="InstanceId">{{ latestReceipt.instanceId || '--' }}</el-descriptions-item>
							<el-descriptions-item label="Progress">{{ latestReceipt.progress ?? '--' }}</el-descriptions-item>
							<el-descriptions-item label="ReportedAt">{{ formatDateTime(latestReceipt.reportedAt) }}</el-descriptions-item>
							<el-descriptions-item label="Message" :span="2">{{ latestReceipt.message || '--' }}</el-descriptions-item>
						</el-descriptions>
					</article>

					<article class="edge-card">
						<div class="edge-card__header">
							<div>
								<h3>状态机摘要</h3>
								<p>来自平台接口的状态机定义，用于帮助执行端和控制台保持一致。</p>
							</div>
						</div>
						<div class="edge-state-tags">
							<span v-for="state in stateMachine.states" :key="state" class="edge-state-tag">{{ state }}</span>
						</div>
					</article>
				</section>

				<section class="edge-history-section">
					<article class="edge-card">
						<div class="edge-card__header">
							<div>
								<h3>回执历史</h3>
								<p>展示当前 Edge 节点最近的任务请求与回执历史，帮助核对平台下发和执行端反馈闭环。</p>
							</div>
						</div>
						<el-table :data="receiptHistory" stripe>
							<el-table-column prop="reportedAt" label="ReportedAt" min-width="170">
								<template #default="scope">{{ formatDateTime(scope.row.reportedAt) }}</template>
							</el-table-column>
							<el-table-column prop="category" label="Category" min-width="100" />
							<el-table-column prop="status" label="Status" min-width="120" />
							<el-table-column prop="taskId" label="TaskId" min-width="260" show-overflow-tooltip />
							<el-table-column prop="progress" label="Progress" min-width="100" />
							<el-table-column prop="message" label="Message" min-width="240" show-overflow-tooltip />
						</el-table>
					</article>
				</section>
			</ConsoleDrawerWorkspace>
		</el-drawer>

		<el-dialog v-model="dispatchDialog" title="平台侧任务下发" width="620px">
			<el-form label-width="130px">
				<el-form-item label="TaskType">
					<el-select v-model="dispatchForm.taskType" style="width: 100%" @change="applyTaskTemplate">
						<el-option v-for="item in taskTypeOptions" :key="item" :label="item" :value="item" />
					</el-select>
				</el-form-item>
				<el-form-item label="RuntimeType">
					<el-input v-model="dispatchForm.runtimeType" />
				</el-form-item>
				<el-form-item label="InstanceId">
					<el-input v-model="dispatchForm.instanceId" />
				</el-form-item>
				<el-form-item label="ExpireAt(min)">
					<el-input-number v-model="dispatchForm.expireInMinutes" :min="1" :max="1440" style="width: 100%" />
				</el-form-item>
				<el-form-item label="Parameters(JSON)">
					<el-input v-model="dispatchForm.parametersText" type="textarea" :rows="6" />
				</el-form-item>
				<el-alert title="提交前会校验 runtimeType、instanceId 和 parameters JSON。" type="info" :closable="false" />
			</el-form>
			<template #footer>
				<el-button @click="dispatchDialog = false">取消</el-button>
				<el-button type="primary" @click="submitTask">提交任务</el-button>
			</template>
		</el-dialog>
	</div>
</template>

<script setup lang="ts">
import { computed, reactive, ref } from 'vue';
import dayjs from 'dayjs';
import { ElMessage } from 'element-plus';
import ConsoleDrawerWorkspace from '/@/components/console/ConsoleDrawerWorkspace.vue';
import { edgeApi } from '/@/api/edge/index';

const drawer = ref(false);
const edgeRef = ref<Record<string, any>>({});
const latestReceipt = ref<Record<string, any> | null>(null);
const receiptHistory = ref<Record<string, any>[]>([]);
const dispatchDialog = ref(false);
const stateMachine = reactive<{ states: string[] }>({ states: [] });
const taskTypeOptions = ['ConfigPush', 'ConfigPullRequest', 'PackageDownload', 'PackageApply', 'RestartRuntime', 'HealthProbe'];
const taskTemplates: Record<string, { runtimeType?: string; parametersText: string; expireInMinutes: number }> = {
	ConfigPush: {
		runtimeType: 'gateway',
		parametersText: '{\n  "configVersion": "v1",\n  "mode": "merge",\n  "payload": {}\n}',
		expireInMinutes: 30,
	},
	HealthProbe: {
		runtimeType: 'gateway',
		parametersText: '{\n  "reason": "console-dispatch",\n  "timeoutSeconds": 15\n}',
		expireInMinutes: 10,
	},
	ConfigPullRequest: {
		runtimeType: 'gateway',
		parametersText: '{\n  "requestedBy": "platform"\n}',
		expireInMinutes: 15,
	},
	PackageDownload: {
		runtimeType: 'pixiu',
		parametersText: '{\n  "packageUrl": "https://example.invalid/pkg.zip",\n  "checksum": "sha256:..."\n}',
		expireInMinutes: 60,
	},
	PackageApply: {
		runtimeType: 'pixiu',
		parametersText: '{\n  "packageVersion": "1.0.0",\n  "restart": true\n}',
		expireInMinutes: 60,
	},
	RestartRuntime: {
		runtimeType: 'gateway',
		parametersText: '{\n  "reason": "config-updated"\n}',
		expireInMinutes: 10,
	},
};
const dispatchForm = reactive({
	taskType: 'HealthProbe',
	runtimeType: '',
	instanceId: '',
	parametersText: '{\n  "reason": "console-dispatch"\n}',
	expireInMinutes: 10,
});

const edgeName = computed(() => edgeRef.value?.name || 'Edge 详情');
const badges = computed(() => [edgeRef.value?.runtimeType || 'unknown', edgeRef.value?.status || 'unknown']);
const metrics = computed(() => [
	{ label: 'Runtime', value: edgeRef.value?.runtimeName || '--', hint: '运行时实例名称', tone: 'primary' as const },
	{ label: 'Version', value: edgeRef.value?.version || '--', hint: '运行时版本', tone: 'success' as const },
	{ label: 'Healthy', value: formatBoolean(edgeRef.value?.healthy), hint: '最近健康状态', tone: 'accent' as const },
	{ label: 'Active', value: formatBoolean(edgeRef.value?.active), hint: '最近活跃状态', tone: 'warning' as const },
	{ label: 'Receipt', value: latestReceipt.value?.status || '暂无', hint: '最近任务回执状态。', tone: 'primary' as const },
	{ label: 'History', value: receiptHistory.value.length, hint: '当前页面缓存的回执历史条数。', tone: 'success' as const },
]);

const formatDateTime = (value?: string) => (value ? dayjs(value).format('YYYY-MM-DD HH:mm:ss') : '--');
const formatBoolean = (value?: boolean | null) => (value === null || value === undefined ? '--' : value ? '是' : '否');
const prettyJson = (raw?: string) => {
	if (!raw) {
		return '{}';
	}

	try {
		return JSON.stringify(JSON.parse(raw), null, 2);
	} catch {
		return raw;
	}
};

const reload = async () => {
	if (!edgeRef.value?.id) {
		return;
	}

	const [detailRes, receiptRes, historyRes, stateMachineRes] = await Promise.all([
		edgeApi().getEdgeDetail(edgeRef.value.id),
		edgeApi().getLatestReceipt(edgeRef.value.id),
		edgeApi().getReceiptHistory(edgeRef.value.id),
		edgeApi().getStateMachine(),
	]);

	edgeRef.value = detailRes.data ?? {};
	latestReceipt.value = receiptRes.data ?? null;
	receiptHistory.value = (historyRes.data ?? []).map(normalizeHistoryRecord);
	stateMachine.states = stateMachineRes.data?.states ?? [];
};

const normalizeHistoryRecord = (record: Record<string, any>) => {
	const key = String(record.key || '');
	const parts = key.split('.');
	const category = parts[parts.length - 1] || '--';
	const taskIdPart = parts.length >= 4 ? parts[3] : '';

	let payload: Record<string, any> = {};
	try {
		payload = record.payload ? JSON.parse(record.payload) : {};
	} catch {
		payload = {};
	}

	return {
		reportedAt: record.at,
		category,
		status: payload.status || record.status || '--',
		taskId: payload.taskId || taskIdPart || '--',
		progress: payload.progress ?? '--',
		message: payload.message || '--',
	};
};

const openDispatchDialog = () => {
	dispatchForm.runtimeType = edgeRef.value?.runtimeType || '';
	dispatchForm.instanceId = edgeRef.value?.instanceId || '';
	applyTaskTemplate(dispatchForm.taskType);
	dispatchDialog.value = true;
};

const applyTaskTemplate = (taskType: string) => {
	const template = taskTemplates[taskType];
	if (!template) {
		return;
	}

	dispatchForm.runtimeType = edgeRef.value?.runtimeType || template.runtimeType || '';
	dispatchForm.parametersText = template.parametersText;
	dispatchForm.expireInMinutes = template.expireInMinutes;
};

const submitTask = async () => {
	if (!edgeRef.value?.id) {
		ElMessage.warning('当前 Edge 节点缺少 ID，无法下发任务');
		return;
	}

	let parameters: Record<string, unknown> = {};
	try {
		parameters = dispatchForm.parametersText ? JSON.parse(dispatchForm.parametersText) : {};
	} catch {
		ElMessage.error('Parameters 不是合法 JSON');
		return;
	}

	if (!dispatchForm.runtimeType?.trim()) {
		ElMessage.warning('RuntimeType 不能为空');
		return;
	}

	if (!dispatchForm.instanceId?.trim() && !edgeRef.value?.instanceId) {
		ElMessage.warning('InstanceId 不能为空');
		return;
	}

	const taskId = crypto.randomUUID();
	const targetKey = `${edgeRef.value.id}:${dispatchForm.runtimeType || edgeRef.value.runtimeType || 'unknown'}${dispatchForm.instanceId ? `:${dispatchForm.instanceId}` : ''}`;

	await edgeApi().submitTask({
		contractVersion: 'edge-task-v1',
		taskId,
		taskType: dispatchForm.taskType as any,
		address: {
			targetType: dispatchForm.runtimeType === 'pixiu' ? 'PixiuRuntime' : 'GatewayRuntime',
			deviceId: edgeRef.value.id,
			runtimeType: dispatchForm.runtimeType || edgeRef.value.runtimeType,
			instanceId: dispatchForm.instanceId || edgeRef.value.instanceId,
			targetKey,
		},
		createdAt: new Date().toISOString(),
		expireAt: new Date(Date.now() + dispatchForm.expireInMinutes * 60 * 1000).toISOString(),
		parameters,
		metadata: {
			source: 'edge-detail-ui',
		},
	});

	dispatchDialog.value = false;
	ElMessage.success('任务已提交');
	await reload();
};

const openDialog = async (edge: Record<string, any>) => {
	edgeRef.value = { ...(edge ?? {}) };
	drawer.value = true;
	await reload();
};

defineExpose({
	openDialog,
});
</script>

<style lang="scss" scoped>
.edge-detail__overview,
.edge-json-panels,
.edge-receipt-section,
.edge-history-section {
	display: grid;
	gap: 18px;
}

.edge-detail__overview {
	grid-template-columns: 1.7fr 1fr;
	margin-bottom: 18px;
}

.edge-json-panels {
	grid-template-columns: repeat(3, minmax(0, 1fr));
}

.edge-receipt-section {
	grid-template-columns: 1.7fr 1fr;
	margin-top: 18px;
}

.edge-history-section {
	margin-top: 18px;
}

.edge-card {
	padding: 18px;
	border-radius: 24px;
	border: 1px solid rgba(226, 232, 240, 0.92);
	background: linear-gradient(180deg, rgba(255, 255, 255, 0.98), rgba(248, 250, 252, 0.96));
	box-shadow: 0 18px 42px rgba(15, 23, 42, 0.05);
}

.edge-card__header {
	margin-bottom: 14px;
}

.edge-card__header h3 {
	margin: 0;
	color: #123b6d;
}

.edge-card__header p {
	margin: 8px 0 0;
	color: #64748b;
	font-size: 13px;
	line-height: 1.7;
}

.edge-tip-list {
	margin: 0;
	padding-left: 18px;
	color: #475569;
	line-height: 1.8;
}

.edge-state-tags {
	display: flex;
	flex-wrap: wrap;
	gap: 10px;
}

.edge-state-tag {
	display: inline-flex;
	align-items: center;
	padding: 8px 12px;
	border-radius: 999px;
	background: rgba(219, 234, 254, 0.72);
	color: #1d4ed8;
	font-size: 12px;
	font-weight: 700;
}

pre {
	margin: 0;
	padding: 14px;
	border-radius: 18px;
	background: #0f172a;
	color: #dbeafe;
	overflow: auto;
	min-height: 220px;
	font-size: 12px;
	line-height: 1.6;
}

@media (max-width: 1200px) {
	.edge-detail__overview,
	.edge-json-panels,
	.edge-receipt-section {
		grid-template-columns: 1fr;
	}
}
</style>