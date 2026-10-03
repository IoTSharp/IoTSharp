<template>
	<div class="release-page">
		<ConsolePageShell
			eyebrow="Release Center"
			title="受控发布"
			description="围绕目标范围、灰度批次、人工确认和回滚闭合发布证据。"
			:badges="badges"
			:metrics="metrics"
		>
			<template #actions>
				<el-button @click="loadPlans">刷新</el-button>
				<el-button type="primary" @click="openCreate">新建发布计划</el-button>
			</template>

			<div class="release-card">
				<div class="release-card__head">
					<div>
						<div class="release-card__eyebrow">Canary Rollout</div>
						<h3>发布计划</h3>
						<p>先创建可审计的计划，再按批次下发 EdgeNode、Gateway 或设备范围任务。</p>
					</div>
					<el-input v-model="query.name" clearable placeholder="按名称筛选" style="width: 220px" @keyup.enter="loadPlans" />
				</div>

				<el-table v-loading="loading" :data="plans" row-key="id" @row-click="openDetail">
					<el-table-column prop="name" label="计划" min-width="220" show-overflow-tooltip />
					<el-table-column prop="planType" label="类型" width="150" />
					<el-table-column label="状态" width="150">
						<template #default="{ row }"><el-tag :type="statusType(row.status)">{{ statusLabel(row.status) }}</el-tag></template>
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
				<el-empty v-if="!loading && plans.length === 0" description="还没有发布计划" />
				<el-pagination v-model:current-page="page" class="release-pagination" background layout="prev, pager, next" :page-size="query.limit" :total="total" @current-change="loadPlans" />
			</div>
		</ConsolePageShell>

		<el-drawer v-model="detailVisible" title="发布计划详情" size="720px" destroy-on-close>
			<template v-if="selectedPlan">
				<div class="detail-summary">
					<div><span>计划名称</span><strong>{{ selectedPlan.name }}</strong></div>
					<div><span>状态</span><el-tag :type="statusType(selectedPlan.status)">{{ statusLabel(selectedPlan.status) }}</el-tag></div>
					<div><span>批次</span><strong>{{ selectedPlan.currentBatchNo || 0 }} / {{ maxBatch(selectedPlan) }}</strong></div>
					<div v-if="selectedPlan.configurationVersionId"><span>配置版本</span><strong>{{ selectedPlan.configurationVersionId }}</strong></div>
				</div>
				<div class="detail-actions">
					<el-button v-if="canStart(selectedPlan)" type="success" @click="runAction(selectedPlan, 'start')">开始发布</el-button>
					<el-button v-if="canConfirm(selectedPlan)" type="success" @click="runAction(selectedPlan, 'confirm')">确认下一批</el-button>
					<el-button v-if="canPause(selectedPlan)" type="warning" @click="runAction(selectedPlan, 'pause')">暂停</el-button>
					<el-button v-if="canResume(selectedPlan)" type="success" @click="runAction(selectedPlan, 'resume')">继续</el-button>
					<el-button v-if="canRollback(selectedPlan)" type="danger" plain @click="runAction(selectedPlan, 'rollback')">回滚</el-button>
				</div>
				<el-alert v-if="selectedPlan.description" :title="selectedPlan.description" type="info" :closable="false" class="detail-alert" />
				<h4>目标任务</h4>
				<el-table :data="selectedPlan.tasks || []" size="small">
					<el-table-column prop="batchNo" label="批次" width="60" />
					<el-table-column prop="targetType" label="目标类型" width="130" />
					<el-table-column prop="targetKey" label="目标" min-width="180" show-overflow-tooltip />
					<el-table-column label="状态" width="120"><template #default="{ row }"><el-tag size="small" :type="taskStatusType(row.status)">{{ statusLabel(row.status) }}</el-tag></template></el-table-column>
					<el-table-column prop="message" label="回执" min-width="180" show-overflow-tooltip />
				</el-table>
			</template>
		</el-drawer>

		<el-dialog v-model="createVisible" title="新建发布计划" width="640px" destroy-on-close>
			<el-form label-width="130px">
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
import ConsolePageShell from '/@/components/console/ConsolePageShell.vue';
import { releaseApi, type CollectionConfigurationVersion, type ReleasePackage, type ReleasePlan, type ReleasePlanActionRequest, type ReleasePlanCreateRequest, type ReleasePlanStatus, type ReleasePlanType, type ReleaseConfirmationPolicy, type ReleaseTargetType } from '/@/api/release';

const api = releaseApi();
const loading = ref(false);
const creating = ref(false);
const plans = ref<ReleasePlan[]>([]);
const packages = ref<ReleasePackage[]>([]);
const configurationVersions = ref<CollectionConfigurationVersion[]>([]);
const loadingConfigurationVersions = ref(false);
const total = ref(0);
const page = ref(1);
const query = reactive({ offset: 0, limit: 10, name: '' });
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
const statusType = (status: string) => ({ Succeeded: 'success', Running: 'primary', Paused: 'warning', WaitingConfirmation: 'warning', Failed: 'danger', RollbackFailed: 'danger', RolledBack: 'info', PartiallySucceeded: 'warning' }[status] || 'info') as any;
const taskStatusType = (status: string) => statusType(status);
const formatDate = (value?: string) => value ? new Date(value).toLocaleString() : '--';
const progress = (row: ReleasePlan) => row.totalTaskCount ? Math.round((row.succeededTaskCount / row.totalTaskCount) * 100) : 0;
const maxBatch = (row: ReleasePlan) => row.batchSize > 0 ? Math.ceil(row.totalTaskCount / row.batchSize) : (row.totalTaskCount ? 1 : 0);
const badges = computed(() => [`计划 ${total.value}`, `当前页 ${plans.value.length}`, `进行中 ${plans.value.filter((item) => item.status === 'Running').length}`]);
const metrics = computed(() => [
	{ label: '计划总数', value: total.value, hint: '当前租户可见的发布计划。', tone: 'primary' as const },
	{ label: '运行中', value: plans.value.filter((item) => item.status === 'Running').length, hint: '正在等待任务回执的计划。', tone: 'accent' as const },
	{ label: '待确认', value: plans.value.filter((item) => item.status === 'WaitingConfirmation').length, hint: '需要人工确认后继续下发。', tone: 'warning' as const },
	{ label: '失败/回滚失败', value: plans.value.filter((item) => item.status === 'Failed' || item.status === 'RollbackFailed').length, hint: '需要检查回执并决定是否回滚。', tone: 'danger' as const },
]);

async function loadPlans() {
	loading.value = true;
	query.offset = page.value - 1;
	try {
		const response: any = await api.listPlans(query);
		plans.value = response?.data?.rows || [];
		total.value = response?.data?.total || 0;
	} finally { loading.value = false; }
}

async function loadPackages() {
	try {
		const response: any = await api.listPackages({ offset: 0, limit: 100 });
		packages.value = response?.data?.rows || [];
	} catch { packages.value = []; }
}

async function openDetail(row: ReleasePlan) {
	try {
		const response: any = await api.getPlan(row.id);
		selectedPlan.value = response?.data || row;
	} catch { selectedPlan.value = row; }
	detailVisible.value = true;
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
		if (detailVisible.value) await openDetail(row);
	} catch (error: any) {
		if (error !== 'cancel' && error !== 'close') ElMessage.error(error?.message || `${labels[action]}失败`);
	}
}

onMounted(() => { loadPlans(); loadPackages(); });
</script>

<style lang="scss" scoped>
.release-page { display: flex; flex-direction: column; gap: 18px; }
.release-card { padding: 20px 22px; border: 1px solid rgba(226,232,240,.92); border-radius: 28px; background: linear-gradient(180deg,rgba(248,251,255,.96),#fff); box-shadow: 0 18px 42px rgba(15,23,42,.05); }
.release-card__head { display:flex; align-items:center; justify-content:space-between; gap:16px; margin-bottom:18px; }
.release-card__eyebrow { margin-bottom: 8px; color:#2563eb; font-size:12px; font-weight:700; letter-spacing:.16em; text-transform:uppercase; }
.release-card h3 { margin:0; color:#123b6d; font-size:22px; }
.release-card p { margin:8px 0 0; color:#64748b; font-size:13px; }
.progress-cell { display:flex; flex-direction:column; gap:4px; }
.progress-cell .el-progress { width:100px; }
.release-pagination { justify-content:flex-end; margin-top:18px; }
.detail-summary { display:grid; grid-template-columns: 1fr 1fr 1fr; gap:12px; margin-bottom:20px; }
.detail-summary > div { display:flex; flex-direction:column; gap:5px; padding:12px; border:1px solid #e2e8f0; border-radius:12px; }
.detail-summary span { color:#64748b; font-size:12px; }
.detail-actions { display:flex; gap:8px; flex-wrap:wrap; margin-bottom:16px; }
.detail-alert { margin-bottom:18px; }
.form-hint { color:#94a3b8; font-size:12px; line-height:1.5; }
.form-hint.inline { margin-left:10px; }
.inline-form-control { display:flex; align-items:center; gap:8px; width:100%; }
@media (max-width: 800px) { .release-card__head { align-items:stretch; flex-direction:column; } .detail-summary { grid-template-columns:1fr; } }
</style>
