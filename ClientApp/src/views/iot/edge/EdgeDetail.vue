<template>
	<div>
		<el-drawer v-model="drawer" size="76%" class="edge-detail-drawer" append-to-body destroy-on-close>
			<ConsoleDrawerWorkspace
				v-loading="loading"
				eyebrow="Edge Detail"
				:title="edgeName"
				description="EdgeNode 运行态、能力声明、采集分配和任务闭环的统一视图。"
				:badges="badges"
				:metrics="metrics"
			>
				<template #actions>
					<el-button :icon="Refresh" type="primary" :loading="loading" @click="reload">刷新</el-button>
					<el-button :icon="Position" plain @click="openDispatchDialog">下发任务</el-button>
				</template>

				<section class="edge-detail__status-grid">
					<article v-for="tile in statusTiles" :key="tile.label" class="edge-status-tile" :class="`tone-${tile.tone}`">
						<span>{{ tile.label }}</span>
						<strong>{{ tile.value }}</strong>
						<small>{{ tile.hint }}</small>
					</article>
				</section>

				<section class="edge-detail__main-grid">
					<article class="edge-panel edge-panel--wide">
						<div class="edge-panel__head">
							<div>
								<h3>运行态快照</h3>
								<p>{{ runtimeSnapshot.contractVersion || 'edge-runtime-status-v1' }}</p>
							</div>
							<el-tag :type="statusTagType(runtimeSnapshot.status)" effect="plain">{{ runtimeSnapshot.status || '--' }}</el-tag>
						</div>
						<el-descriptions :column="2" border>
							<el-descriptions-item label="EdgeNodeId">{{ runtimeSnapshot.edgeNodeId || '--' }}</el-descriptions-item>
							<el-descriptions-item label="GatewayId">{{ runtimeSnapshot.gatewayId || '--' }}</el-descriptions-item>
							<el-descriptions-item label="RuntimeType">{{ runtimeSnapshot.runtimeType || '--' }}</el-descriptions-item>
							<el-descriptions-item label="RuntimeName">{{ runtimeSnapshot.runtimeName || '--' }}</el-descriptions-item>
							<el-descriptions-item label="Version">{{ runtimeSnapshot.version || '--' }}</el-descriptions-item>
							<el-descriptions-item label="InstanceId">{{ runtimeSnapshot.instanceId || '--' }}</el-descriptions-item>
							<el-descriptions-item label="Active">{{ formatBoolean(runtimeSnapshot.active) }}</el-descriptions-item>
							<el-descriptions-item label="Healthy">{{ formatBoolean(runtimeSnapshot.healthy) }}</el-descriptions-item>
							<el-descriptions-item label="Uptime">{{ formatDuration(runtimeSnapshot.uptimeSeconds) }}</el-descriptions-item>
							<el-descriptions-item label="Platform">{{ runtimeSnapshot.platform || '--' }}</el-descriptions-item>
							<el-descriptions-item label="HostName">{{ runtimeSnapshot.hostName || '--' }}</el-descriptions-item>
							<el-descriptions-item label="IpAddress">{{ runtimeSnapshot.ipAddress || '--' }}</el-descriptions-item>
							<el-descriptions-item label="LastHeartbeat">{{ formatDateTime(runtimeSnapshot.lastHeartbeatDateTime) }}</el-descriptions-item>
							<el-descriptions-item label="LastActivity">{{ formatDateTime(runtimeSnapshot.lastActivityDateTime) }}</el-descriptions-item>
							<el-descriptions-item label="RegisteredAt">{{ formatDateTime(runtimeSnapshot.lastRegistrationDateTime) }}</el-descriptions-item>
							<el-descriptions-item label="UpdatedAt">{{ formatDateTime(runtimeSnapshot.updatedAt) }}</el-descriptions-item>
						</el-descriptions>
					</article>

					<article class="edge-panel">
						<div class="edge-panel__head">
							<div>
								<h3>合同边界</h3>
								<p>{{ capabilitySnapshot.contractVersion || 'edge-capability-v1' }}</p>
							</div>
						</div>
						<div class="edge-contract-list">
							<div class="edge-contract-row">
								<span>Runtime</span>
								<strong>{{ runtimeSnapshot.contractVersion || '--' }}</strong>
							</div>
							<div class="edge-contract-row">
								<span>Capability</span>
								<strong>{{ capabilitySnapshot.contractVersion || '--' }}</strong>
							</div>
							<div class="edge-contract-row">
								<span>Collection</span>
								<strong>{{ latestAssignment?.contractVersion || '--' }}</strong>
							</div>
							<div class="edge-contract-row">
								<span>Task</span>
								<strong>{{ stateMachine.contractVersion || latestReceipt?.contractVersion || 'edge-task-v1' }}</strong>
							</div>
						</div>
					</article>
				</section>

				<section class="edge-detail__capability-grid">
					<article class="edge-panel edge-panel--wide">
						<div class="edge-panel__head">
							<div>
								<h3>能力快照</h3>
								<p>{{ formatDateTime(capabilitySnapshot.reportedAt || capabilitySnapshot.updatedAt) }}</p>
							</div>
							<el-tag effect="plain">{{ capabilityProtocolTags.length }} protocols</el-tag>
						</div>
						<div class="edge-capability-groups">
							<div class="edge-tag-group">
								<span>Protocols</span>
								<div>
									<el-tag v-for="item in capabilityProtocolTags" :key="item" effect="plain">{{ item }}</el-tag>
									<span v-if="!capabilityProtocolTags.length" class="edge-empty-inline">--</span>
								</div>
							</div>
							<div class="edge-tag-group">
								<span>PointTypes</span>
								<div>
									<el-tag v-for="item in capabilitySnapshot.supportedPointTypes" :key="item" type="success" effect="plain">{{ item }}</el-tag>
									<span v-if="!capabilitySnapshot.supportedPointTypes.length" class="edge-empty-inline">--</span>
								</div>
							</div>
							<div class="edge-tag-group">
								<span>Transforms</span>
								<div>
									<el-tag v-for="item in capabilitySnapshot.supportedTransforms" :key="item" type="warning" effect="plain">{{ item }}</el-tag>
									<span v-if="!capabilitySnapshot.supportedTransforms.length" class="edge-empty-inline">--</span>
								</div>
							</div>
							<div class="edge-tag-group">
								<span>Triggers</span>
								<div>
									<el-tag v-for="item in capabilitySnapshot.supportedReportTriggers" :key="item" type="info" effect="plain">{{ item }}</el-tag>
									<span v-if="!capabilitySnapshot.supportedReportTriggers.length" class="edge-empty-inline">--</span>
								</div>
							</div>
							<div class="edge-tag-group">
								<span>Features</span>
								<div>
									<el-tag v-for="item in capabilitySnapshot.features" :key="item" effect="plain">{{ item }}</el-tag>
									<span v-if="!capabilitySnapshot.features.length" class="edge-empty-inline">--</span>
								</div>
							</div>
						</div>
					</article>

					<article class="edge-panel">
						<div class="edge-panel__head">
							<div>
								<h3>任务能力</h3>
								<p>{{ taskCapabilityRows.length }} 个声明</p>
							</div>
						</div>
						<el-table :data="taskCapabilityRows" stripe height="275">
							<el-table-column prop="taskType" label="TaskType" min-width="150" show-overflow-tooltip />
							<el-table-column prop="contractVersion" label="Contract" min-width="140" show-overflow-tooltip />
							<el-table-column prop="supportsProgressText" label="Progress" width="90" />
							<el-table-column prop="supportsCancellationText" label="Cancel" width="80" />
						</el-table>
					</article>
				</section>

				<section class="edge-detail__assignment-grid">
					<article class="edge-panel edge-panel--wide">
						<div class="edge-panel__head">
							<div>
								<h3>采集配置分配</h3>
								<p>{{ assignmentSummary }}</p>
							</div>
							<el-tag :type="assignmentStatusTagType(latestAssignment?.status)" effect="plain">
								{{ latestAssignment?.status || 'No Assignment' }}
							</el-tag>
						</div>
						<div class="edge-version-strip">
							<div class="edge-version-strip__item">
								<span>当前版本</span>
								<strong>{{ formatVersion(versionStatusSnapshot.currentConfigurationVersion) }}</strong>
								<small>{{ shortHash(versionStatusSnapshot.currentConfigurationHash) }}</small>
							</div>
							<div class="edge-version-strip__item">
								<span>目标版本</span>
								<strong>{{ formatVersion(versionStatusSnapshot.targetConfigurationVersion) }}</strong>
								<small>{{ shortHash(versionStatusSnapshot.targetConfigurationHash) }}</small>
							</div>
							<div class="edge-version-strip__item edge-version-strip__item--wide">
								<span>差异</span>
								<strong>
									<el-tag :type="versionDifferenceTagType" effect="plain">
										{{ versionDifferenceText }}
									</el-tag>
								</strong>
								<small>{{ versionStatusSnapshot.differenceSummary }}</small>
							</div>
							<div class="edge-version-strip__item edge-version-strip__item--wide">
								<span>最近发布结果</span>
								<strong>
									<el-tag :type="taskStatusTagType(versionStatusSnapshot.lastPublishStatus || '')" effect="plain">
										{{ versionStatusSnapshot.lastPublishStatus || '--' }}
									</el-tag>
								</strong>
								<small>{{ formatDateTime(versionStatusSnapshot.lastPublishAt) }}</small>
							</div>
						</div>
						<el-table :data="collectionAssignments" stripe>
							<el-table-column prop="configurationVersion" label="Target" width="95">
								<template #default="scope">{{ formatVersion(scope.row.configurationVersion) }}</template>
							</el-table-column>
							<el-table-column prop="appliedConfigurationVersion" label="Current" width="105">
								<template #default="scope">{{ formatVersion(scope.row.appliedConfigurationVersion) }}</template>
							</el-table-column>
							<el-table-column label="Diff" min-width="160" show-overflow-tooltip>
								<template #default="scope">
									<el-tag :type="assignmentHasDifference(scope.row) ? 'warning' : 'success'" effect="plain">
										{{ assignmentDifferenceText(scope.row) }}
									</el-tag>
								</template>
							</el-table-column>
							<el-table-column prop="status" label="Status" width="120">
								<template #default="scope">
									<el-tag :type="assignmentStatusTagType(scope.row.status)" effect="plain">{{ scope.row.status }}</el-tag>
								</template>
							</el-table-column>
							<el-table-column prop="targetType" label="TargetType" min-width="140" />
							<el-table-column prop="runtimeType" label="RuntimeType" min-width="120" />
							<el-table-column prop="instanceId" label="InstanceId" min-width="180" show-overflow-tooltip />
							<el-table-column prop="targetKey" label="TargetKey" min-width="260" show-overflow-tooltip />
							<el-table-column prop="taskCount" label="Tasks" width="80" />
							<el-table-column prop="configurationHash" label="Hash" min-width="220" show-overflow-tooltip />
							<el-table-column prop="assignedAt" label="AssignedAt" min-width="170">
								<template #default="scope">{{ formatDateTime(scope.row.assignedAt) }}</template>
							</el-table-column>
							<el-table-column prop="lastPulledAt" label="LastPulledAt" min-width="170">
								<template #default="scope">{{ formatDateTime(scope.row.lastPulledAt) }}</template>
							</el-table-column>
							<el-table-column prop="lastExecutionStatus" label="LastResult" min-width="130">
								<template #default="scope">
									<el-tag :type="taskStatusTagType(scope.row.lastExecutionStatus)" effect="plain">{{ scope.row.lastExecutionStatus || '--' }}</el-tag>
								</template>
							</el-table-column>
							<el-table-column prop="lastExecutionProgress" label="Progress" width="120">
								<template #default="scope">
									<el-progress
										v-if="typeof scope.row.lastExecutionProgress === 'number'"
										:percentage="scope.row.lastExecutionProgress"
										:stroke-width="6"
										:show-text="false"
									/>
									<span v-else>--</span>
								</template>
							</el-table-column>
							<el-table-column prop="lastExecutionAt" label="LastResultAt" min-width="170">
								<template #default="scope">{{ formatDateTime(scope.row.lastExecutionAt) }}</template>
							</el-table-column>
						</el-table>
					</article>

					<article class="edge-panel">
						<div class="edge-panel__head">
							<div>
								<h3>兼容合同</h3>
								<p>{{ compatibleContractRows.length }} 条</p>
							</div>
						</div>
						<el-table :data="compatibleContractRows" stripe height="275">
							<el-table-column prop="contractName" label="Name" min-width="150" show-overflow-tooltip />
							<el-table-column prop="contractVersion" label="Version" min-width="140" show-overflow-tooltip />
							<el-table-column prop="deprecatedText" label="Deprecated" width="110" />
						</el-table>
					</article>
				</section>

				<section class="edge-detail__task-grid">
					<article class="edge-panel edge-panel--wide">
						<div class="edge-panel__head">
							<div>
								<h3>任务历史</h3>
								<p>最近 {{ receiptHistory.length }} 条正式任务事件</p>
							</div>
							<el-tag :type="taskStatusTagType(latestReceipt?.status)" effect="plain">{{ latestReceipt?.status || 'No Receipt' }}</el-tag>
						</div>
						<el-table :data="receiptHistory" stripe>
							<el-table-column prop="reportedAt" label="At" min-width="170">
								<template #default="scope">{{ formatDateTime(scope.row.reportedAt) }}</template>
							</el-table-column>
							<el-table-column prop="category" label="Category" width="105">
								<template #default="scope">
									<el-tag effect="plain">{{ scope.row.category }}</el-tag>
								</template>
							</el-table-column>
							<el-table-column prop="status" label="Status" width="120">
								<template #default="scope">
									<el-tag :type="taskStatusTagType(scope.row.status)" effect="plain">{{ scope.row.status }}</el-tag>
								</template>
							</el-table-column>
							<el-table-column prop="taskType" label="TaskType" min-width="150" show-overflow-tooltip />
							<el-table-column prop="taskId" label="TaskId" min-width="230" show-overflow-tooltip />
							<el-table-column prop="runtimeType" label="Runtime" min-width="120" />
							<el-table-column prop="progress" label="Progress" width="120">
								<template #default="scope">
									<el-progress
										v-if="typeof scope.row.progress === 'number'"
										:percentage="scope.row.progress"
										:stroke-width="6"
										:show-text="false"
									/>
									<span v-else>{{ scope.row.progress }}</span>
								</template>
							</el-table-column>
							<el-table-column prop="message" label="Message" min-width="220" show-overflow-tooltip />
							<el-table-column type="expand">
								<template #default="scope">
									<pre class="edge-json edge-json--inline">{{ scope.row.payloadText }}</pre>
								</template>
							</el-table-column>
						</el-table>
					</article>

					<article class="edge-panel">
						<div class="edge-panel__head">
							<div>
								<h3>最近回执</h3>
								<p>{{ formatDateTime(latestReceipt?.reportedAt) }}</p>
							</div>
						</div>
						<el-empty v-if="!latestReceipt" description="暂无回执" />
						<div v-else class="edge-receipt-card">
							<div class="edge-receipt-card__status">
								<el-tag :type="taskStatusTagType(latestReceipt.status)" effect="dark">{{ latestReceipt.status }}</el-tag>
								<span>{{ latestReceipt.taskId }}</span>
							</div>
							<el-progress
								v-if="typeof latestReceipt.progress === 'number'"
								:percentage="latestReceipt.progress"
								:status="latestReceipt.status === 'Failed' ? 'exception' : latestReceipt.status === 'Succeeded' ? 'success' : undefined"
							/>
							<el-descriptions :column="1" border>
								<el-descriptions-item label="TargetType">{{ latestReceipt.targetType || '--' }}</el-descriptions-item>
								<el-descriptions-item label="TargetKey">{{ latestReceipt.targetKey || '--' }}</el-descriptions-item>
								<el-descriptions-item label="RuntimeType">{{ latestReceipt.runtimeType || '--' }}</el-descriptions-item>
								<el-descriptions-item label="InstanceId">{{ latestReceipt.instanceId || '--' }}</el-descriptions-item>
								<el-descriptions-item label="Message">{{ latestReceipt.message || '--' }}</el-descriptions-item>
							</el-descriptions>
						</div>
					</article>
				</section>

				<section class="edge-detail__state-grid">
					<article class="edge-panel">
						<div class="edge-panel__head">
							<div>
								<h3>状态机</h3>
								<p>{{ stateMachine.states.length }} states</p>
							</div>
						</div>
						<div class="edge-state-flow">
							<div v-for="state in stateMachine.states" :key="state" class="edge-state-flow__row">
								<el-tag :type="taskStatusTagType(state)" effect="plain">{{ state }}</el-tag>
								<span>{{ formatTransitions(state) }}</span>
							</div>
						</div>
					</article>

					<article class="edge-panel">
						<div class="edge-panel__head">
							<div>
								<h3>Metadata</h3>
								<p>runtime/capability</p>
							</div>
						</div>
						<pre class="edge-json">{{ prettyJson(runtimeSnapshot.metadata) }}</pre>
					</article>

					<article class="edge-panel">
						<div class="edge-panel__head">
							<div>
								<h3>Metrics</h3>
								<p>heartbeat snapshot</p>
							</div>
						</div>
						<pre class="edge-json">{{ prettyJson(runtimeSnapshot.metrics) }}</pre>
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
import { Position, Refresh } from '@element-plus/icons-vue';
import ConsoleDrawerWorkspace from '/@/components/console/ConsoleDrawerWorkspace.vue';
import {
	edgeApi,
	type EdgeCapability,
	type EdgeCollectionAssignment,
	type EdgeCollectionVersionStatus,
	type EdgeRuntimeStatus,
	type EdgeTaskHistoryRecord,
	type EdgeTaskReceipt,
	type EdgeTaskStateMachine,
} from '/@/api/edge/index';

type TagType = '' | 'success' | 'warning' | 'info' | 'danger';
type Tone = 'primary' | 'accent' | 'success' | 'warning' | 'danger';

interface NormalizedHistoryRecord {
	reportedAt?: string;
	category: string;
	status: string;
	taskId: string;
	taskType: string;
	runtimeType: string;
	instanceId: string;
	progress: number | string;
	message: string;
	payloadText: string;
}

const api = edgeApi();
const drawer = ref(false);
const loading = ref(false);
const edgeRef = ref<Record<string, any>>({});
const runtimeStatus = ref<EdgeRuntimeStatus | null>(null);
const capability = ref<EdgeCapability | null>(null);
const latestReceipt = ref<EdgeTaskReceipt | null>(null);
const receiptHistory = ref<NormalizedHistoryRecord[]>([]);
const collectionAssignments = ref<EdgeCollectionAssignment[]>([]);
const collectionVersionStatus = ref<EdgeCollectionVersionStatus | null>(null);
const dispatchDialog = ref(false);
const stateMachine = reactive<EdgeTaskStateMachine>({
	contractVersion: '',
	states: [],
	transitions: {},
	terminalStates: [],
});

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
		runtimeType: 'gateway',
		parametersText: '{\n  "packageUrl": "https://example.invalid/pkg.zip",\n  "checksum": "sha256:..."\n}',
		expireInMinutes: 60,
	},
	PackageApply: {
		runtimeType: 'gateway',
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

const protocolNames: Record<string, string> = {
	0: 'Unknown',
	1: 'Modbus',
	2: 'OpcUa',
	3: 'Bacnet',
	4: 'IEC104',
	5: 'Mqtt',
	99: 'Custom',
};

const transformNames: Record<string, string> = {
	1: 'Scale',
	2: 'Offset',
	3: 'Expression',
	4: 'EnumMap',
	5: 'BitExtract',
	6: 'WordSwap',
	7: 'ByteSwap',
	8: 'Clamp',
	9: 'DefaultOnError',
};

const triggerNames: Record<string, string> = {
	1: 'Always',
	2: 'OnChange',
	3: 'Deadband',
	4: 'Interval',
	5: 'QualityChange',
};

const edgeName = computed(() => edgeRef.value?.name || 'Edge 详情');

const runtimeSnapshot = computed(() => {
	const source = runtimeStatus.value ?? edgeRef.value?.runtimeStatus ?? {};
	const fallback = edgeRef.value ?? {};
	return {
		contractVersion: source.contractVersion || 'edge-runtime-status-v1',
		edgeNodeId: source.edgeNodeId || fallback.id || '',
		gatewayId: source.gatewayId || fallback.gatewayId || fallback.id || '',
		active: source.active ?? fallback.active,
		lastActivityDateTime: source.lastActivityDateTime || fallback.lastActivityDateTime,
		runtimeType: source.runtimeType || fallback.runtimeType || '',
		runtimeName: source.runtimeName || fallback.runtimeName || fallback.name || '',
		version: source.version || fallback.version || '',
		instanceId: source.instanceId || fallback.instanceId || '',
		platform: source.platform || fallback.platform || '',
		hostName: source.hostName || fallback.hostName || '',
		ipAddress: source.ipAddress || fallback.ipAddress || '',
		status: source.status || fallback.status || '',
		healthy: source.healthy ?? fallback.healthy,
		uptimeSeconds: source.uptimeSeconds ?? fallback.uptimeSeconds,
		lastHeartbeatDateTime: source.lastHeartbeatDateTime || fallback.lastHeartbeatDateTime,
		lastRegistrationDateTime: source.lastRegistrationDateTime || fallback.lastRegistrationDateTime,
		updatedAt: source.updatedAt || fallback.updatedAt,
		metadata: normalizeMap(source.metadata, fallback.metadata),
		metrics: normalizeMap(source.metrics, fallback.metrics),
	};
});

const capabilitySnapshot = computed(() => {
	const source = capability.value ?? edgeRef.value?.capability ?? normalizeMap(edgeRef.value?.capabilities);
	return {
		contractVersion: source.contractVersion || 'edge-capability-v1',
		edgeNodeId: source.edgeNodeId || edgeRef.value?.id || '',
		gatewayId: source.gatewayId || edgeRef.value?.gatewayId || edgeRef.value?.id || '',
		runtimeType: source.runtimeType || runtimeSnapshot.value.runtimeType || '',
		runtimeName: source.runtimeName || runtimeSnapshot.value.runtimeName || '',
		version: source.version || runtimeSnapshot.value.version || '',
		instanceId: source.instanceId || runtimeSnapshot.value.instanceId || '',
		reportedAt: source.reportedAt || '',
		updatedAt: source.updatedAt || '',
		protocols: normalizeList(source.protocols, protocolNames),
		supportedProtocols: normalizeList(source.supportedProtocols, protocolNames),
		supportedPointTypes: normalizeList(source.supportedPointTypes),
		supportedTransforms: normalizeList(source.supportedTransforms, transformNames),
		supportedReportTriggers: normalizeList(source.supportedReportTriggers, triggerNames),
		features: normalizeList(source.features),
		tasks: normalizeList(source.tasks),
		taskCapabilities: normalizeRows(source.taskCapabilities),
		compatibleContracts: normalizeRows(source.compatibleContracts),
		metadata: normalizeMap(source.metadata),
	};
});

const capabilityProtocolTags = computed(() => uniqueList([...capabilitySnapshot.value.protocols, ...capabilitySnapshot.value.supportedProtocols]));

const taskCapabilityRows = computed(() => {
	const rows = capabilitySnapshot.value.taskCapabilities.length
		? capabilitySnapshot.value.taskCapabilities
		: capabilitySnapshot.value.tasks.map((taskType) => ({ taskType, contractVersion: 'edge-task-v1', supportsProgress: true, supportsCancellation: false }));

	return rows.map((item) => ({
		taskType: toText(item.taskType),
		contractVersion: toText(item.contractVersion || 'edge-task-v1'),
		supportsProgressText: formatBoolean(item.supportsProgress),
		supportsCancellationText: formatBoolean(item.supportsCancellation),
	}));
});

const compatibleContractRows = computed(() =>
	capabilitySnapshot.value.compatibleContracts.map((item) => ({
		contractName: toText(item.contractName),
		contractVersion: toText(item.contractVersion),
		deprecatedText: formatBoolean(item.deprecated),
	}))
);

const activeAssignments = computed(() => collectionAssignments.value.filter((item) => item.status === 'Active'));
const latestAssignment = computed(() => activeAssignments.value[0] ?? collectionAssignments.value[0] ?? null);
const assignmentSummary = computed(() => {
	if (!collectionAssignments.value.length) {
		return '暂无采集配置分配';
	}

	return `Active ${activeAssignments.value.length} / Total ${collectionAssignments.value.length}`;
});

const versionStatusSnapshot = computed(() => {
	const source = collectionVersionStatus.value ?? edgeRef.value?.collectionVersionStatus;
	if (source && (source.gatewayId || source.targetConfigurationVersion !== undefined || source.currentConfigurationVersion !== undefined)) {
		return normalizeVersionStatus(source);
	}

	return buildVersionStatusFromAssignment(latestAssignment.value);
});
const hasTargetConfiguration = computed(() => typeof versionStatusSnapshot.value.targetConfigurationVersion === 'number');
const versionDifferenceTagType = computed<TagType>(() => {
	if (!hasTargetConfiguration.value) {
		return 'info';
	}

	return versionStatusSnapshot.value.hasDifference ? 'warning' : 'success';
});
const versionDifferenceText = computed(() => {
	if (!hasTargetConfiguration.value) {
		return '暂无目标';
	}

	return versionStatusSnapshot.value.hasDifference ? '待同步' : '一致';
});

const badges = computed(() => [
	runtimeSnapshot.value.runtimeType || 'unknown-runtime',
	runtimeSnapshot.value.status || 'unknown-status',
	runtimeSnapshot.value.contractVersion,
]);

const metrics = computed(() => [
	{ label: 'Status', value: runtimeSnapshot.value.status || '--', hint: '运行态快照状态', tone: statusTone(runtimeSnapshot.value.status) },
	{ label: 'Heartbeat', value: formatDateTime(runtimeSnapshot.value.lastHeartbeatDateTime), hint: '最近心跳', tone: 'primary' as const },
	{ label: 'Capability', value: capabilityProtocolTags.value.length, hint: '协议能力数量', tone: 'accent' as const },
	{
		label: 'Assignment',
		value: formatVersionPair(versionStatusSnapshot.value),
		hint: versionStatusSnapshot.value.differenceSummary,
		tone: !hasTargetConfiguration.value ? 'primary' as const : versionStatusSnapshot.value.hasDifference ? 'warning' as const : 'success' as const,
	},
	{ label: 'Task', value: latestReceipt.value?.status || '暂无', hint: '最近任务状态', tone: statusTone(latestReceipt.value?.status) },
	{ label: 'History', value: receiptHistory.value.length, hint: '最近任务事件', tone: 'warning' as const },
]);

const statusTiles = computed(() => [
	{
		label: '运行状态',
		value: runtimeSnapshot.value.status || '--',
		hint: `${runtimeSnapshot.value.runtimeName || '--'} / ${runtimeSnapshot.value.version || '--'}`,
		tone: statusTone(runtimeSnapshot.value.status),
	},
	{
		label: '健康',
		value: formatBoolean(runtimeSnapshot.value.healthy),
		hint: runtimeSnapshot.value.active ? 'active=true' : 'active=false',
		tone: runtimeSnapshot.value.healthy === false ? 'danger' : 'success',
	},
	{
		label: '心跳',
		value: formatDateTime(runtimeSnapshot.value.lastHeartbeatDateTime),
		hint: formatDuration(runtimeSnapshot.value.uptimeSeconds),
		tone: 'primary',
	},
	{
		label: '采集配置',
		value: formatVersionPair(versionStatusSnapshot.value),
		hint: versionStatusSnapshot.value.differenceSummary || latestAssignment.value?.targetKey || '--',
		tone: !hasTargetConfiguration.value ? 'primary' : versionStatusSnapshot.value.hasDifference ? 'warning' : 'accent',
	},
]);

function normalizeVersionStatus(source: Partial<EdgeCollectionVersionStatus>): EdgeCollectionVersionStatus {
	return {
		contractVersion: source.contractVersion || 'collection-config-v1',
		gatewayId: source.gatewayId || runtimeSnapshot.value.gatewayId || edgeRef.value?.id || '',
		edgeNodeId: source.edgeNodeId || runtimeSnapshot.value.edgeNodeId || edgeRef.value?.id || '',
		assignmentId: source.assignmentId,
		targetConfigurationVersionId: source.targetConfigurationVersionId,
		targetConfigurationVersion: source.targetConfigurationVersion ?? null,
		targetConfigurationHash: source.targetConfigurationHash || '',
		targetTaskCount: source.targetTaskCount ?? null,
		targetSourceType: source.targetSourceType || '',
		targetSourceId: source.targetSourceId || '',
		targetSourceVersion: source.targetSourceVersion || '',
		targetAssignedAt: source.targetAssignedAt ?? null,
		lastPulledAt: source.lastPulledAt ?? null,
		currentConfigurationVersion: source.currentConfigurationVersion ?? null,
		currentConfigurationHash: source.currentConfigurationHash || '',
		currentAppliedAt: source.currentAppliedAt ?? null,
		isTargetApplied: Boolean(source.isTargetApplied),
		hasDifference: Boolean(source.hasDifference),
		versionDelta: source.versionDelta ?? null,
		differenceSummary: source.differenceSummary || '暂无目标采集配置',
		lastPublishTaskId: source.lastPublishTaskId,
		lastPublishStatus: source.lastPublishStatus ?? null,
		lastPublishMessage: source.lastPublishMessage || '',
		lastPublishProgress: source.lastPublishProgress ?? null,
		lastPublishAt: source.lastPublishAt ?? null,
	};
}

function buildVersionStatusFromAssignment(assignment?: EdgeCollectionAssignment | null): EdgeCollectionVersionStatus {
	if (!assignment) {
		return normalizeVersionStatus({
			differenceSummary: '暂无目标采集配置',
		});
	}

	const targetVersion = assignment.configurationVersion;
	const currentVersion = assignment.appliedConfigurationVersion ?? null;
	const hasTargetHash = Boolean(assignment.configurationHash);
	const hashMatches = !hasTargetHash || String(assignment.appliedConfigurationHash || '').toLowerCase() === String(assignment.configurationHash || '').toLowerCase();
	const versionMatches = typeof currentVersion === 'number' && currentVersion === targetVersion;
	const isTargetApplied = versionMatches && hashMatches;
	const versionDelta = typeof currentVersion === 'number' ? targetVersion - currentVersion : null;

	return normalizeVersionStatus({
		gatewayId: assignment.gatewayId,
		edgeNodeId: assignment.edgeNodeId,
		assignmentId: assignment.id,
		targetConfigurationVersionId: assignment.collectionConfigurationVersionId,
		targetConfigurationVersion: targetVersion,
		targetConfigurationHash: assignment.configurationHash,
		targetTaskCount: assignment.taskCount,
		targetSourceType: assignment.sourceType,
		targetSourceId: assignment.sourceId,
		targetSourceVersion: assignment.sourceVersion,
		targetAssignedAt: assignment.assignedAt,
		lastPulledAt: assignment.lastPulledAt,
		currentConfigurationVersion: currentVersion,
		currentConfigurationHash: assignment.appliedConfigurationHash,
		currentAppliedAt: assignment.appliedAt,
		isTargetApplied,
		hasDifference: !isTargetApplied,
		versionDelta,
		differenceSummary: buildDifferenceSummary(currentVersion, targetVersion, versionDelta, isTargetApplied, hashMatches),
		lastPublishTaskId: assignment.lastExecutionTaskId,
		lastPublishStatus: assignment.lastExecutionStatus,
		lastPublishMessage: assignment.lastExecutionMessage,
		lastPublishProgress: assignment.lastExecutionProgress,
		lastPublishAt: assignment.lastExecutionAt,
	});
}

function buildDifferenceSummary(
	currentVersion: number | null,
	targetVersion: number,
	versionDelta: number | null,
	isTargetApplied: boolean,
	hashMatches: boolean
) {
	if (isTargetApplied) {
		return '当前版本已与目标版本一致';
	}

	if (currentVersion === null) {
		return '执行端尚未确认已应用版本';
	}

	if (!hashMatches && currentVersion === targetVersion) {
		return '当前版本号一致但配置哈希不同';
	}

	if (typeof versionDelta === 'number' && versionDelta > 0) {
		return `当前版本落后目标版本 ${versionDelta} 个版本`;
	}

	if (typeof versionDelta === 'number' && versionDelta < 0) {
		return `当前版本高于目标版本 ${Math.abs(versionDelta)} 个版本`;
	}

	return '当前版本和目标版本存在差异';
}

const formatVersion = (value?: number | null) => (typeof value === 'number' ? `v${value}` : '--');

const formatVersionPair = (status: EdgeCollectionVersionStatus) => `${formatVersion(status.currentConfigurationVersion)} -> ${formatVersion(status.targetConfigurationVersion)}`;

const shortHash = (value?: string | null) => {
	if (!value) {
		return '--';
	}

	const text = String(value);
	return text.length > 16 ? `${text.slice(0, 8)}...${text.slice(-6)}` : text;
};

const assignmentHasDifference = (assignment: EdgeCollectionAssignment) => buildVersionStatusFromAssignment(assignment).hasDifference;

const assignmentDifferenceText = (assignment: EdgeCollectionAssignment) => {
	const status = buildVersionStatusFromAssignment(assignment);
	return status.hasDifference ? status.differenceSummary : '一致';
};

const formatDateTime = (value?: string | Date | null) => (value ? dayjs(value).format('YYYY-MM-DD HH:mm:ss') : '--');

const formatBoolean = (value?: boolean | null) => {
	if (value === null || value === undefined) {
		return '--';
	}

	return value ? '是' : '否';
};

const formatDuration = (seconds?: number | null) => {
	if (seconds === null || seconds === undefined || Number.isNaN(Number(seconds))) {
		return '--';
	}

	const total = Math.max(0, Number(seconds));
	const days = Math.floor(total / 86400);
	const hours = Math.floor((total % 86400) / 3600);
	const minutes = Math.floor((total % 3600) / 60);
	const secs = Math.floor(total % 60);
	if (days > 0) {
		return `${days}d ${hours}h ${minutes}m`;
	}

	if (hours > 0) {
		return `${hours}h ${minutes}m`;
	}

	if (minutes > 0) {
		return `${minutes}m ${secs}s`;
	}

	return `${secs}s`;
};

const prettyJson = (raw?: unknown) => {
	if (raw === null || raw === undefined || raw === '') {
		return '{}';
	}

	const parsed = typeof raw === 'string' ? parseJson(raw) : raw;
	try {
		return JSON.stringify(parsed, null, 2);
	} catch {
		return String(raw);
	}
};

const parseJson = (value: string) => {
	try {
		return JSON.parse(value);
	} catch {
		return value;
	}
};

const normalizeMap = (primary?: unknown, fallback?: unknown): Record<string, any> => {
	const parsed = primary === undefined || primary === null || primary === '' ? fallback : primary;
	const value = typeof parsed === 'string' ? parseJson(parsed) : parsed;
	return value && typeof value === 'object' && !Array.isArray(value) ? (value as Record<string, any>) : {};
};

const normalizeRows = (value?: unknown): Record<string, any>[] => (Array.isArray(value) ? value.filter((item) => item && typeof item === 'object') as Record<string, any>[] : []);

const normalizeList = (value?: unknown, names?: Record<string, string>) => {
	const parsed = typeof value === 'string' ? parseJson(value) : value;
	if (Array.isArray(parsed)) {
		return uniqueList(parsed.map((item) => enumText(item, names)).filter(Boolean));
	}

	if (typeof parsed === 'string') {
		return uniqueList(parsed.split(',').map((item) => enumText(item, names)).filter(Boolean));
	}

	return [];
};

const uniqueList = (values: string[]) => Array.from(new Set(values.map((item) => item.trim()).filter(Boolean)));

const enumText = (value: unknown, names?: Record<string, string>) => {
	const key = String(value ?? '').trim();
	if (!key) {
		return '';
	}

	return names?.[key] ?? key;
};

const toText = (value?: unknown) => {
	if (value === null || value === undefined || value === '') {
		return '--';
	}

	return String(value);
};

const statusTagType = (status?: string): TagType => {
	const normalized = String(status || '').toLowerCase();
	if (['running', 'registered', 'succeeded', 'accepted'].includes(normalized)) {
		return 'success';
	}

	if (['degraded', 'pending', 'sent'].includes(normalized)) {
		return 'warning';
	}

	if (['offline', 'failed', 'timedout', 'cancelled'].includes(normalized)) {
		return 'danger';
	}

	return 'info';
};

const taskStatusTagType = (status?: string): TagType => statusTagType(status);

const assignmentStatusTagType = (status?: string): TagType => {
	const normalized = String(status || '').toLowerCase();
	if (normalized === 'active') {
		return 'success';
	}

	if (normalized === 'pending') {
		return 'warning';
	}

	if (normalized === 'revoked') {
		return 'danger';
	}

	return 'info';
};

const statusTone = (status?: string): Tone => {
	const tag = statusTagType(status);
	if (tag === 'success') {
		return 'success';
	}

	if (tag === 'warning') {
		return 'warning';
	}

	if (tag === 'danger') {
		return 'danger';
	}

	return 'primary';
};

const normalizeHistoryRecord = (record: EdgeTaskHistoryRecord): NormalizedHistoryRecord => {
	const key = String(record.key || '');
	const parts = key.split('.');
	const category = parts[parts.length - 1] || '--';
	const taskIdPart = parts.length >= 4 ? parts[3] : '';
	const payload = normalizeMap(record.payload);
	const address = normalizeMap(payload.address);

	return {
		reportedAt: record.at,
		category,
		status: toText(payload.status || record.status),
		taskId: toText(payload.taskId || taskIdPart),
		taskType: toText(payload.taskType),
		runtimeType: toText(payload.runtimeType || address.runtimeType),
		instanceId: toText(payload.instanceId || address.instanceId),
		progress: typeof payload.progress === 'number' ? payload.progress : '--',
		message: toText(payload.message),
		payloadText: prettyJson(payload),
	};
};

const formatTransitions = (state: string) => {
	const next = stateMachine.transitions?.[state] ?? [];
	if (stateMachine.terminalStates.includes(state)) {
		return 'terminal';
	}

	return next.length ? next.join(' / ') : '--';
};

const reload = async () => {
	if (!edgeRef.value?.id) {
		return;
	}

	loading.value = true;
	try {
		const edgeId = edgeRef.value.id;
		const [detailRes, runtimeRes, capabilityRes, receiptRes, historyRes, stateMachineRes, assignmentRes, versionStatusRes] = await Promise.all([
			api.getEdgeDetail(edgeId),
			api.getRuntimeStatus(edgeId),
			api.getCapability(edgeId),
			api.getLatestReceipt(edgeId),
			api.getReceiptHistory(edgeId),
			api.getStateMachine(),
			api.getCollectionAssignments(edgeId, { offset: 0, limit: 20, sorter: 'ConfigurationVersion', sort: 'desc' }),
			api.getCollectionVersionStatus(edgeId),
		]);

		edgeRef.value = detailRes.data ?? {};
		runtimeStatus.value = runtimeRes.data ?? edgeRef.value.runtimeStatus ?? null;
		capability.value = capabilityRes.data ?? edgeRef.value.capability ?? null;
		latestReceipt.value = receiptRes.data ?? null;
		receiptHistory.value = ((historyRes.data ?? []) as EdgeTaskHistoryRecord[]).map(normalizeHistoryRecord);
		collectionAssignments.value = assignmentRes.data?.rows ?? [];
		collectionVersionStatus.value = versionStatusRes.data ?? edgeRef.value.collectionVersionStatus ?? null;
		stateMachine.contractVersion = stateMachineRes.data?.contractVersion ?? '';
		stateMachine.states = stateMachineRes.data?.states ?? [];
		stateMachine.transitions = stateMachineRes.data?.transitions ?? {};
		stateMachine.terminalStates = stateMachineRes.data?.terminalStates ?? [];
	} catch (error: any) {
		ElMessage.error(error?.response?.data?.msg || error?.message || 'Edge 详情加载失败');
	} finally {
		loading.value = false;
	}
};

const openDispatchDialog = () => {
	dispatchForm.runtimeType = runtimeSnapshot.value.runtimeType || 'gateway';
	dispatchForm.instanceId = runtimeSnapshot.value.instanceId || '';
	applyTaskTemplate(dispatchForm.taskType);
	dispatchDialog.value = true;
};

const applyTaskTemplate = (taskType: string) => {
	const template = taskTemplates[taskType];
	if (!template) {
		return;
	}

	dispatchForm.runtimeType = runtimeSnapshot.value.runtimeType || template.runtimeType || '';
	dispatchForm.instanceId = runtimeSnapshot.value.instanceId || dispatchForm.instanceId;
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

	const instanceId = dispatchForm.instanceId || runtimeSnapshot.value.instanceId;
	if (!instanceId?.trim()) {
		ElMessage.warning('InstanceId 不能为空');
		return;
	}

	const taskId = crypto.randomUUID();
	const targetKey = `${edgeRef.value.id}:${dispatchForm.runtimeType || runtimeSnapshot.value.runtimeType || 'unknown'}${instanceId ? `:${instanceId}` : ''}`;

	await api.submitTask({
		contractVersion: 'edge-task-v1',
		taskId,
		taskType: dispatchForm.taskType as any,
		address: {
			targetType: 'GatewayRuntime',
			deviceId: edgeRef.value.id,
			runtimeType: dispatchForm.runtimeType || runtimeSnapshot.value.runtimeType,
			instanceId,
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
	runtimeStatus.value = edgeRef.value.runtimeStatus ?? null;
	capability.value = edgeRef.value.capability ?? null;
	latestReceipt.value = null;
	receiptHistory.value = [];
	collectionAssignments.value = [];
	collectionVersionStatus.value = edgeRef.value.collectionVersionStatus ?? null;
	drawer.value = true;
	await reload();
};

defineExpose({
	openDialog,
});
</script>

<style lang="scss" scoped>
.edge-detail__status-grid,
.edge-detail__main-grid,
.edge-detail__capability-grid,
.edge-detail__assignment-grid,
.edge-detail__task-grid,
.edge-detail__state-grid {
	display: grid;
	gap: 16px;
}

.edge-detail__status-grid {
	grid-template-columns: repeat(4, minmax(0, 1fr));
}

.edge-detail__main-grid,
.edge-detail__capability-grid,
.edge-detail__assignment-grid,
.edge-detail__task-grid {
	grid-template-columns: minmax(0, 1.7fr) minmax(320px, 0.8fr);
}

.edge-detail__state-grid {
	grid-template-columns: 1.1fr 1fr 1fr;
}

.edge-status-tile,
.edge-panel {
	border: 1px solid rgba(226, 232, 240, 0.92);
	background: linear-gradient(180deg, rgba(255, 255, 255, 0.98), rgba(248, 250, 252, 0.96));
	box-shadow: 0 14px 34px rgba(15, 23, 42, 0.05);
}

.edge-status-tile {
	min-height: 116px;
	padding: 16px 18px;
	border-radius: 18px;
	overflow: hidden;
}

.edge-status-tile span {
	display: block;
	color: #64748b;
	font-size: 12px;
	font-weight: 700;
	text-transform: uppercase;
}

.edge-status-tile strong {
	display: block;
	margin-top: 12px;
	color: #123b6d;
	font-size: 22px;
	line-height: 1.2;
	word-break: break-word;
}

.edge-status-tile small {
	display: block;
	margin-top: 10px;
	color: #7890a8;
	font-size: 12px;
	line-height: 1.5;
	word-break: break-word;
}

.edge-panel {
	min-width: 0;
	padding: 18px;
	border-radius: 18px;
}

.edge-panel--wide {
	min-width: 0;
}

.edge-panel__head {
	display: flex;
	align-items: flex-start;
	justify-content: space-between;
	gap: 14px;
	margin-bottom: 14px;
}

.edge-panel__head h3 {
	margin: 0;
	color: #123b6d;
	font-size: 17px;
	letter-spacing: 0;
}

.edge-panel__head p {
	margin: 7px 0 0;
	color: #64748b;
	font-size: 12px;
	line-height: 1.5;
	word-break: break-word;
}

.edge-contract-list,
.edge-receipt-card,
.edge-state-flow {
	display: flex;
	flex-direction: column;
	gap: 12px;
}

.edge-contract-row {
	display: flex;
	align-items: center;
	justify-content: space-between;
	gap: 12px;
	padding: 12px 14px;
	border-radius: 12px;
	background: rgba(248, 250, 252, 0.92);
	border: 1px solid rgba(226, 232, 240, 0.82);
}

.edge-contract-row span {
	color: #64748b;
	font-size: 12px;
	font-weight: 700;
}

.edge-contract-row strong {
	color: #123b6d;
	font-size: 13px;
	word-break: break-word;
	text-align: right;
}

.edge-capability-groups {
	display: grid;
	grid-template-columns: repeat(2, minmax(0, 1fr));
	gap: 14px;
}

.edge-version-strip {
	display: grid;
	grid-template-columns: minmax(130px, 0.8fr) minmax(130px, 0.8fr) minmax(180px, 1.2fr) minmax(180px, 1.2fr);
	gap: 12px;
	margin-bottom: 14px;
}

.edge-version-strip__item {
	min-width: 0;
	min-height: 92px;
	padding: 12px;
	border-radius: 12px;
	background: rgba(248, 250, 252, 0.92);
	border: 1px solid rgba(226, 232, 240, 0.82);
}

.edge-version-strip__item span {
	display: block;
	color: #64748b;
	font-size: 12px;
	font-weight: 700;
}

.edge-version-strip__item strong {
	display: block;
	margin-top: 8px;
	min-height: 24px;
	color: #123b6d;
	font-size: 18px;
	line-height: 1.35;
	word-break: break-word;
}

.edge-version-strip__item small {
	display: block;
	margin-top: 8px;
	color: #7890a8;
	font-size: 12px;
	line-height: 1.45;
	word-break: break-word;
}

.edge-tag-group {
	min-height: 86px;
	padding: 12px;
	border-radius: 12px;
	background: rgba(248, 250, 252, 0.92);
	border: 1px solid rgba(226, 232, 240, 0.82);
}

.edge-tag-group > span {
	display: block;
	margin-bottom: 10px;
	color: #64748b;
	font-size: 12px;
	font-weight: 700;
}

.edge-tag-group > div {
	display: flex;
	flex-wrap: wrap;
	gap: 8px;
}

.edge-empty-inline {
	color: #94a3b8;
	font-size: 13px;
}

.edge-receipt-card__status {
	display: flex;
	align-items: center;
	gap: 10px;
	padding: 12px;
	border-radius: 12px;
	background: rgba(248, 250, 252, 0.92);
	border: 1px solid rgba(226, 232, 240, 0.82);
}

.edge-receipt-card__status span {
	color: #334155;
	font-size: 12px;
	word-break: break-all;
}

.edge-state-flow__row {
	display: grid;
	grid-template-columns: 112px minmax(0, 1fr);
	align-items: center;
	gap: 10px;
	padding: 10px 12px;
	border-radius: 12px;
	background: rgba(248, 250, 252, 0.92);
	border: 1px solid rgba(226, 232, 240, 0.82);
}

.edge-state-flow__row span:last-child {
	color: #475569;
	font-size: 12px;
	word-break: break-word;
}

.edge-json {
	margin: 0;
	min-height: 240px;
	max-height: 360px;
	padding: 14px;
	border-radius: 12px;
	background: #0f172a;
	color: #dbeafe;
	overflow: auto;
	font-size: 12px;
	line-height: 1.6;
}

.edge-json--inline {
	min-height: 0;
	max-height: 240px;
}

.tone-success {
	background: linear-gradient(180deg, rgba(22, 163, 74, 0.1), rgba(255, 255, 255, 0.96));
}

.tone-warning {
	background: linear-gradient(180deg, rgba(245, 158, 11, 0.11), rgba(255, 255, 255, 0.96));
}

.tone-danger {
	background: linear-gradient(180deg, rgba(239, 68, 68, 0.1), rgba(255, 255, 255, 0.96));
}

.tone-accent {
	background: linear-gradient(180deg, rgba(14, 165, 233, 0.1), rgba(255, 255, 255, 0.96));
}

:deep(.el-table) {
	border-radius: 12px;
	overflow: hidden;
}

:deep(.el-descriptions__label) {
	width: 148px;
	color: #64748b;
	font-weight: 700;
}

@media (max-width: 1280px) {
	.edge-detail__status-grid,
	.edge-detail__state-grid {
		grid-template-columns: repeat(2, minmax(0, 1fr));
	}

	.edge-detail__main-grid,
	.edge-detail__capability-grid,
	.edge-detail__assignment-grid,
	.edge-detail__task-grid {
		grid-template-columns: 1fr;
	}

	.edge-version-strip {
		grid-template-columns: repeat(2, minmax(0, 1fr));
	}
}

@media (max-width: 767px) {
	.edge-detail__status-grid,
	.edge-detail__state-grid,
	.edge-capability-groups,
	.edge-version-strip {
		grid-template-columns: 1fr;
	}

	.edge-panel,
	.edge-status-tile {
		border-radius: 14px;
	}
}
</style>
