<template>
	<div class="release-page console-list-page">
		<ConsolePageShell
			eyebrow="Release Center"
			title="受控发布"
			description="围绕目标范围、灰度批次、人工确认和回滚闭合发布证据。"
			:metrics="isNarrow || !loaded || loadFailed ? [] : metrics"
		>
			<template #actions>
				<el-button @click="loadPlans">刷新</el-button>
				<el-button type="primary" @click="openCreate">新建发布计划</el-button>
			</template>

			<div class="release-card console-list-panel">
				<div class="release-card__head">
					<div>
						<h3>发布计划</h3>
					</div>
					<form class="release-card__filter" @submit.prevent="applyNameFilter">
						<el-input v-model="draftName" clearable aria-label="计划名称" placeholder="按名称筛选" @clear="applyNameFilter" />
						<el-button native-type="submit" :loading="loading">查询</el-button>
					</form>
				</div>

				<p v-if="loadFailed" class="release-card__notice" role="alert">
					发布计划加载失败。<el-button link type="primary" @click="loadPlans">重试</el-button>
				</p>
				<div v-else-if="isNarrow" :aria-busy="loading" v-loading="loading">
					<p class="release-card__notice" role="status">{{ resultSummary }}</p>
					<ul v-if="plans.length" class="release-list" aria-label="发布计划">
						<li v-for="row in plans" :key="row.id">
							<div class="release-list__summary">
								<strong>{{ row.name }}</strong>
								<span class="console-status" :class="statusClass(row.status)">{{ statusLabel(row.status) }}</span>
								<span class="release-list__meta">批次 {{ row.currentBatchNo || 0 }} / {{ maxBatch(row) }} · {{ row.succeededTaskCount }}/{{ row.totalTaskCount }} 项成功</span>
								<span class="release-list__meta">更新于 {{ formatDate(row.updatedAt) }}</span>
							</div>
							<el-button link type="primary" :aria-label="'查看详情 ' + row.name" @click="openDetail(row)">详情</el-button>
						</li>
					</ul>
				</div>
				<div v-else class="console-list-table" role="region" aria-label="发布计划列表" tabindex="0">
					<el-table v-loading="loading" :data="plans" row-key="id" size="small" :empty-text="resultSummary" @row-click="openDetail">
						<el-table-column prop="name" label="计划" min-width="220" show-overflow-tooltip />
						<el-table-column prop="planType" label="类型" width="150" />
						<el-table-column label="状态" width="150">
							<template #default="{ row }"><span class="console-status" :class="statusClass(row.status)">{{ statusLabel(row.status) }}</span></template>
						</el-table-column>
						<el-table-column label="任务进度" width="170">
							<template #default="{ row }">
								<div class="progress-cell"><span>{{ row.succeededTaskCount }}/{{ row.totalTaskCount }}</span><el-progress :percentage="progress(row)" :show-text="false" /></div>
							</template>
						</el-table-column>
						<el-table-column prop="currentBatchNo" label="当前批次" width="100" />
						<el-table-column prop="updatedAt" label="最近更新" width="180">
							<template #default="{ row }">{{ formatDate(row.updatedAt) }}</template>
						</el-table-column>
						<el-table-column label="操作" width="230" fixed="right">
							<template #default="{ row }">
								<el-button link type="primary" @click.stop="openDetail(row)">详情</el-button>
								<el-button v-if="canStart(row)" link type="success" @click.stop="runAction(row, 'start')">开始</el-button>
								<el-button v-if="canConfirm(row)" link type="success" @click.stop="runAction(row, 'confirm')">确认批次</el-button>
								<el-button v-if="canPause(row)" link type="warning" @click.stop="runAction(row, 'pause')">暂停</el-button>
								<el-button v-if="canResume(row)" link type="success" @click.stop="runAction(row, 'resume')">继续</el-button>
								<el-button v-if="canRollback(row)" link type="danger" @click.stop="runAction(row, 'rollback')">回滚</el-button>
							</template>
						</el-table-column>
					</el-table>
				</div>
				<el-pagination
					v-if="loaded && !loadFailed"
					:current-page="page"
					:page-size="pageSize"
					:total="total"
					:pager-count="5"
					class="console-list-pagination"
					size="small"
					background
					:layout="isNarrow ? 'prev, pager, next' : 'total, prev, pager, next'"
					@update:current-page="changePage"
				/>
			</div>
		</ConsolePageShell>

		<el-drawer v-model="detailVisible" title="发布计划详情" size="min(720px, 100%)" destroy-on-close>
			<template v-if="selectedPlan">
				<div class="detail-summary">
					<div><span>计划名称</span><strong>{{ selectedPlan.name }}</strong></div>
					<div><span>状态</span><span class="console-status" :class="statusClass(selectedPlan.status)">{{ statusLabel(selectedPlan.status) }}</span></div>
					<div><span>批次</span><strong>{{ selectedPlan.currentBatchNo || 0 }} / {{ maxBatch(selectedPlan) }}</strong></div>
					<div><span>目标任务</span><strong>{{ selectedPlan.totalTaskCount }} 项</strong></div>
					<div v-if="selectedPlan.configurationVersionId"><span>配置版本</span><strong>{{ selectedPlan.configurationVersionId }}</strong></div>
				</div>
				<p v-if="detailLoading" role="status">正在读取计划详情…</p>
				<div v-else-if="detailFailed" class="detail-alert" role="alert">
					计划详情加载失败。<el-button link type="primary" @click="openDetail(selectedPlan)">重试</el-button>
				</div>
				<div v-else class="detail-actions">
					<el-button v-if="canStart(selectedPlan)" type="success" @click="runAction(selectedPlan, 'start')">开始发布</el-button>
					<el-button v-if="canConfirm(selectedPlan)" type="success" @click="runAction(selectedPlan, 'confirm')">确认下一批</el-button>
					<el-button v-if="canPause(selectedPlan)" type="warning" @click="runAction(selectedPlan, 'pause')">暂停</el-button>
					<el-button v-if="canResume(selectedPlan)" type="success" @click="runAction(selectedPlan, 'resume')">继续</el-button>
					<el-button v-if="canRollback(selectedPlan)" type="danger" plain @click="runAction(selectedPlan, 'rollback')">回滚</el-button>
				</div>
				<el-alert v-if="selectedPlan.description" :title="selectedPlan.description" type="info" :closable="false" class="detail-alert" />
				<div v-if="!detailLoading && !detailFailed" role="region" aria-label="目标任务和回执" tabindex="0">
					<h4>目标任务</h4>
					<el-table :data="selectedPlan.tasks || []" size="small">
					<el-table-column prop="batchNo" label="批次" width="60" />
					<el-table-column prop="targetType" label="目标类型" width="130" />
					<el-table-column prop="targetKey" label="目标" min-width="180" show-overflow-tooltip />
					<el-table-column label="状态" width="120"><template #default="{ row }"><span class="console-status" :class="statusClass(row.status)">{{ statusLabel(row.status) }}</span></template></el-table-column>
						<el-table-column prop="message" label="回执" min-width="180" show-overflow-tooltip />
					</el-table>
				</div>
			</template>
		</el-drawer>

		<el-dialog v-model="createVisible" title="新建发布计划" width="min(640px, calc(100vw - 24px))" destroy-on-close>
			<el-form :label-position="isNarrow ? 'top' : 'right'" label-width="130px">
				<el-form-item label="计划名称" required><el-input v-model="createForm.name" placeholder="例如：杭州工厂 Edge 灰度升级" /></el-form-item>
				<el-form-item label="发布类型" required>
					<el-select v-model="createForm.planType" style="width: 100%" @change="onPlanTypeChange">
						<el-option label="软件更新" value="SoftwareUpdate" />
						<el-option label="采集配置发布" value="ConfigurationRollout" />
						<el-option label="设备脚本 OTA" value="DeviceScriptOta" />
						<el-option label="固件 OTA" value="FirmwareOta" />
					</el-select>
				</el-form-item>
				<el-form-item v-if="createForm.planType !== 'ConfigurationRollout'" label="发布包" required>
					<el-select v-model="createForm.packageId" filterable clearable placeholder="选择已发布的软件包" style="width: 100%">
						<el-option v-for="item in packages" :key="item.id" :label="`${item.name} · ${item.version}`" :value="item.id" />
					</el-select>
					<div v-if="packages.length === 0" class="form-hint">暂无可用软件包，请先在 ReleasePackages API 上传包。</div>
				</el-form-item>
				<el-form-item v-else label="采集配置版本" required>
					<div class="inline-form-control">
						<el-select v-model="createForm.configurationVersionId" filterable allow-create default-first-option clearable placeholder="选择或粘贴不可变版本 ID" style="width: 100%">
							<el-option v-for="item in configurationVersions" :key="item.id" :label="`v${item.version} · ${item.configurationHash || item.id}`" :value="item.id" />
						</el-select>
						<el-button :loading="loadingConfigurationVersions" @click="loadConfigurationVersions">读取版本</el-button>
					</div>
					<div class="form-hint">先填写目标 ID，再读取该 Gateway/EdgeNode 的已发布不可变配置版本。</div>
				</el-form-item>
				<el-form-item label="目标类型"><el-select v-model="createForm.targetType" style="width: 100%"><el-option v-for="item in targetTypes" :key="item" :label="item" :value="item" /></el-select></el-form-item>
				<el-form-item label="目标 ID" required><el-input v-model="createForm.targetIds" type="textarea" :rows="2" placeholder="输入一个或多个目标 ID，每行或逗号分隔" /></el-form-item>
				<el-form-item label="确认策略"><el-radio-group v-model="createForm.confirmationPolicy"><el-radio value="ManualBetweenBatches">批次间确认</el-radio><el-radio value="ManualBeforeStart">开始前确认</el-radio><el-radio value="None">自动</el-radio></el-radio-group></el-form-item>
				<el-form-item label="每批目标数"><el-input-number v-model="createForm.batchSize" :min="0" :max="1000" /><span class="form-hint inline">0 表示全部目标一批下发</span></el-form-item>
				<el-form-item label="失败后继续"><el-switch v-model="createForm.continueOnFailure" /></el-form-item>
				<el-form-item label="创建后开始"><el-switch v-model="createForm.autoStart" /></el-form-item>
			</el-form>
			<template #footer><el-button @click="createVisible = false">取消</el-button><el-button type="primary" :loading="creating" @click="submitCreate">创建计划</el-button></template>
		</el-dialog>
	</div>
</template>

<script setup lang="ts">
import { computed, onMounted, reactive, ref } from 'vue';
import { ElMessage, ElMessageBox } from 'element-plus';
import { useMediaQuery } from '@vueuse/core';
import ConsolePageShell from '/@/components/console/ConsolePageShell.vue';
import { releaseApi, type CollectionConfigurationVersion, type ReleasePackage, type ReleasePlan, type ReleasePlanActionRequest, type ReleasePlanCreateRequest, type ReleasePlanType, type ReleaseConfirmationPolicy, type ReleaseTargetType } from '/@/api/release';

const api = releaseApi();
const isNarrow = useMediaQuery('(max-width: 767px)');
const loading = ref(false);
const loaded = ref(false);
const loadFailed = ref(false);
const detailLoading = ref(false);
const detailFailed = ref(false);
let latestListRequest = 0;
let latestDetailRequest = 0;
const creating = ref(false);
const plans = ref<ReleasePlan[]>([]);
const packages = ref<ReleasePackage[]>([]);
const configurationVersions = ref<CollectionConfigurationVersion[]>([]);
const loadingConfigurationVersions = ref(false);
const total = ref(0);
const page = ref(1);
const pageSize = 10;
const draftName = ref('');
const appliedName = ref('');
const selectedPlan = ref<ReleasePlan>();
const detailVisible = ref(false);
const createVisible = ref(false);
const createForm = reactive({ name: '', planType: 'SoftwareUpdate' as ReleasePlanType, packageId: '', configurationVersionId: '', targetType: 'EdgeNode' as ReleaseTargetType, targetIds: '', confirmationPolicy: 'ManualBetweenBatches' as ReleaseConfirmationPolicy, batchSize: 1, continueOnFailure: false, autoStart: true });
const targetTypes = computed<ReleaseTargetType[]>(() => {
	if (['DeviceScriptOta', 'FirmwareOta'].includes(createForm.planType)) return ['Device', 'AssetScope', 'DeviceScope'];
	return createForm.planType === 'ConfigurationRollout' ? ['Gateway'] : ['EdgeNode', 'Gateway'];
});

const statusLabels: Record<string, string> = { Draft: '草稿', WaitingConfirmation: '等待确认', Running: '运行中', Paused: '已暂停', Succeeded: '已成功', PartiallySucceeded: '部分成功', Failed: '失败', Cancelled: '已取消', RollingBack: '回滚中', RolledBack: '已回滚', RollbackFailed: '回滚失败', Pending: '待下发', Sent: '已发送', Accepted: '已接收', TimedOut: '超时', RolledBackTask: '已回滚' };
const statusLabel = (status: string) => statusLabels[status] || status;
const statusTones: Record<string, string> = {
	Succeeded: 'success',
	Running: 'info',
	Sent: 'info',
	Accepted: 'info',
	RollingBack: 'info',
	RolledBack: 'info',
	Paused: 'warning',
	WaitingConfirmation: 'warning',
	PartiallySucceeded: 'warning',
	TimedOut: 'warning',
	Failed: 'danger',
	RollbackFailed: 'danger',
};
const statusClass = (status: string) => statusTones[status] ? 'console-status--' + statusTones[status] : '';
const formatDate = (value?: string) => {
	const date = value ? new Date(value) : null;
	return date && !Number.isNaN(date.getTime()) ? date.toLocaleString() : '—';
};
const progress = (row: ReleasePlan) => row.totalTaskCount ? Math.round((row.succeededTaskCount / row.totalTaskCount) * 100) : 0;
const maxBatch = (row: ReleasePlan) => row.batchSize > 0 ? Math.ceil(row.totalTaskCount / row.batchSize) : (row.totalTaskCount ? 1 : 0);
const metrics = computed(() => [
	{ label: '筛选结果', value: total.value, hint: '当前条件下可见的计划。', tone: 'primary' as const },
	{ label: '本页运行中', value: plans.value.filter((item) => item.status === 'Running').length, hint: '本页状态为运行中的计划。', tone: 'accent' as const },
	{ label: '本页待确认', value: plans.value.filter((item) => item.status === 'WaitingConfirmation').length, hint: '需人工确认后继续下发。', tone: 'warning' as const },
	{ label: '本页失败或回滚失败', value: plans.value.filter((item) => item.status === 'Failed' || item.status === 'RollbackFailed').length, hint: '可打开详情核查回执。', tone: 'danger' as const },
]);
const resultSummary = computed(() => {
	if (loading.value) return '正在加载发布计划…';
	if (total.value) return '共 ' + total.value + ' 项计划';
	return appliedName.value ? '没有符合条件的发布计划' : '暂无发布计划';
});

/** 按已应用的名称查询，保留刷新前的页码。 */
async function loadPlans() {
	const requestId = ++latestListRequest;
	loading.value = true;
	loadFailed.value = false;
	try {
		const response: any = await api.listPlans({ offset: page.value - 1, limit: pageSize, name: appliedName.value });
		if (requestId !== latestListRequest) return;
		plans.value = response?.data?.rows || [];
		total.value = response?.data?.total || 0;
		loaded.value = true;
	} catch {
		if (requestId === latestListRequest) loadFailed.value = true;
	} finally {
		if (requestId === latestListRequest) loading.value = false;
	}
}

function applyNameFilter() {
	appliedName.value = draftName.value.trim();
	changePage(1);
}

function changePage(nextPage: number) {
	page.value = nextPage;
	void loadPlans();
}

async function loadPackages() {
	try {
		const response: any = await api.listPackages({ offset: 0, limit: 100 });
		packages.value = response?.data?.rows || [];
	} catch { packages.value = []; }
}

/** 打开详情并读取最新目标任务。只有详情加载成功时显示操作。 */
async function openDetail(row: ReleasePlan) {
	const requestId = ++latestDetailRequest;
	selectedPlan.value = row;
	detailVisible.value = true;
	detailLoading.value = true;
	detailFailed.value = false;
	try {
		const response: any = await api.getPlan(row.id);
		if (requestId !== latestDetailRequest || !detailVisible.value) return;
		if (response?.data) selectedPlan.value = response.data;
		else detailFailed.value = true;
	} catch {
		if (requestId === latestDetailRequest) detailFailed.value = true;
	} finally {
		if (requestId === latestDetailRequest) detailLoading.value = false;
	}
}

function openCreate() {
	Object.assign(createForm, { name: '', planType: 'SoftwareUpdate', packageId: '', configurationVersionId: '', targetType: 'EdgeNode', targetIds: '', confirmationPolicy: 'ManualBetweenBatches', batchSize: 1, continueOnFailure: false, autoStart: true });
	configurationVersions.value = [];
	createVisible.value = true;
}

function onPlanTypeChange() {
	createForm.targetType = targetTypes.value[0];
	createForm.packageId = '';
	createForm.configurationVersionId = '';
	configurationVersions.value = [];
}

async function loadConfigurationVersions() {
	const firstTarget = createForm.targetIds.split(/[\n,;]+/).map((item) => item.trim()).find(Boolean);
	if (!firstTarget) { ElMessage.warning('请先填写一个 Gateway ID'); return; }
	loadingConfigurationVersions.value = true;
	try {
		const response: any = await api.listConfigurationVersions(firstTarget);
		configurationVersions.value = response?.data?.rows || [];
		if (configurationVersions.value.length === 0) ElMessage.info('该目标暂无已发布配置版本，可粘贴版本 ID');
	} catch { configurationVersions.value = []; ElMessage.error('读取配置版本失败'); }
	finally { loadingConfigurationVersions.value = false; }
}

async function submitCreate() {
	const ids = createForm.targetIds.split(/[\n,;]+/).map((item) => item.trim()).filter(Boolean);
	if (!createForm.name.trim() || ids.length === 0) { ElMessage.warning('计划名称和目标 ID 均不能为空'); return; }
	if (createForm.planType === 'ConfigurationRollout' ? !createForm.configurationVersionId : !createForm.packageId) { ElMessage.warning(createForm.planType === 'ConfigurationRollout' ? '请选择或填写采集配置版本' : '请选择发布包'); return; }
	creating.value = true;
	try {
		const payload: ReleasePlanCreateRequest = { name: createForm.name.trim(), planType: createForm.planType, confirmationPolicy: createForm.confirmationPolicy, strategy: { batchSize: createForm.batchSize, continueOnFailure: createForm.continueOnFailure }, autoStart: createForm.autoStart, targets: ids.map((targetId) => ({ targetType: createForm.targetType, targetId })) };
		if (createForm.planType === 'ConfigurationRollout') payload.configurationVersionId = createForm.configurationVersionId;
		else payload.packageId = createForm.packageId;
		await api.createPlan(payload);
		createVisible.value = false;
		ElMessage.success('发布计划已创建');
		await loadPlans();
	} finally { creating.value = false; }
}

const canStart = (row: ReleasePlan) => row.status === 'Draft';
const canConfirm = (row: ReleasePlan) => row.status === 'WaitingConfirmation' || (row.status === 'Running' && row.confirmationPolicy === 'ManualBetweenBatches');
const canPause = (row: ReleasePlan) => row.status === 'Running';
const canResume = (row: ReleasePlan) => row.status === 'Paused';
const canRollback = (row: ReleasePlan) => ['Running', 'Succeeded', 'PartiallySucceeded', 'Failed', 'Paused'].includes(row.status);

async function runAction(row: ReleasePlan, action: 'start' | 'confirm' | 'pause' | 'resume' | 'rollback') {
	const labels = { start: '开始发布', confirm: '确认批次', pause: '暂停发布', resume: '继续发布', rollback: '回滚发布' };
	try {
		await ElMessageBox.confirm(`确定要${labels[action]}“${row.name}”吗？操作会留下审计记录。`, labels[action], { type: action === 'rollback' ? 'warning' : 'info' });
		const payload: ReleasePlanActionRequest = { reason: `console:${action}`, force: action === 'confirm' };
		if (action === 'rollback' && row.planType === 'ConfigurationRollout') {
			const prompt = await ElMessageBox.prompt('输入用于回滚的不可变配置版本 ID', '配置版本回滚', { inputValue: row.configurationVersionId || '', inputPlaceholder: 'GUID' });
			payload.rollbackConfigurationVersionId = prompt.value.trim();
		}
		if (action === 'start') await api.startPlan(row.id, payload);
		if (action === 'confirm') await api.confirmPlan(row.id, payload);
		if (action === 'pause') await api.pausePlan(row.id, payload);
		if (action === 'resume') await api.resumePlan(row.id, payload);
		if (action === 'rollback') await api.rollbackPlan(row.id, payload);
		ElMessage.success(`${labels[action]}已提交`);
		await loadPlans();
		if (detailVisible.value && selectedPlan.value?.id === row.id) await openDetail(row);
	} catch (error: any) {
		if (error !== 'cancel' && error !== 'close') ElMessage.error(error?.message || `${labels[action]}失败`);
	}
}

onMounted(() => { loadPlans(); loadPackages(); });
</script>

<style lang="scss" scoped>
.release-card {
	padding: 10px 16px;
	border: 1px solid var(--iotsharp-border);
	border-radius: var(--iotsharp-radius-panel);
	background: var(--iotsharp-surface);
	box-shadow: var(--iotsharp-shadow-panel);
}
.release-card__head {
	display: flex;
	align-items: center;
	justify-content: space-between;
	gap: 12px;
}
.release-card h3 {
	margin: 0;
	color: var(--iotsharp-ink);
	font-size: 15px;
}
.progress-cell { display:flex; flex-direction:column; gap:4px; }
.progress-cell .el-progress { width:100px; }
.detail-summary { display:grid; grid-template-columns: 1fr 1fr 1fr; gap:12px; margin-bottom:20px; }
.detail-summary > div {
	display: flex;
	min-width: 0;
	flex-direction: column;
	align-items: flex-start;
	gap: 5px;
	padding: 12px;
	border: 1px solid var(--iotsharp-border);
	border-radius: var(--iotsharp-radius-panel);
	overflow-wrap: anywhere;
}
.detail-summary > div > span:first-child {
	color: var(--iotsharp-text-soft);
	font-size: 12px;
}
.detail-actions { display:flex; gap:8px; flex-wrap:wrap; margin-bottom:16px; }
.detail-alert { margin-bottom:18px; }
.form-hint { color:var(--iotsharp-text-muted); font-size:12px; line-height:1.5; }
.form-hint.inline { margin-left:10px; }
.inline-form-control { display:flex; align-items:center; gap:8px; width:100%; }
.release-card__filter {
	display: flex;
	align-items: center;
	gap: 8px;
}
.release-card__notice {
	margin: 10px 0 4px;
	color: var(--iotsharp-text-soft);
	font-size: 13px;
}
.release-list {
	margin: 0;
	padding: 0;
	list-style: none;
}
.release-list li {
	display: flex;
	align-items: flex-start;
	justify-content: space-between;
	gap: 8px;
	padding: 8px 0;
}
.release-list li + li {
	border-top: 1px solid var(--iotsharp-border);
}
.release-list__summary {
	display: grid;
	min-width: 0;
	justify-items: start;
	gap: 4px;
	color: var(--iotsharp-ink);
	overflow-wrap: anywhere;
}
.release-list__meta {
	color: var(--iotsharp-text-soft);
	font-size: 13px;
}
.release-list li > .el-button {
	flex: none;
	min-width: 44px;
	min-height: 44px;
}
@media (max-width: 767px) {
	.release-card { padding: 10px; }
	.release-card__head { align-items: stretch; flex-direction: column; }
	.detail-summary { grid-template-columns: repeat(2, minmax(0, 1fr)); }
	.inline-form-control { flex-wrap: wrap; }
	.detail-actions .el-button + .el-button { margin-left: 0; }
}

</style>
