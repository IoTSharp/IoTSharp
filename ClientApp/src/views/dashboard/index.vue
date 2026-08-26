<template>
	<div class="ops-dashboard" v-loading="loading">
		<header class="overview-bar">
			<div class="overview-bar__head">
				<div class="overview-bar__copy">
					<h1>运行概览</h1>
					<p>汇总接入状态、消息链路、平台健康和当前待处理事项。</p>
				</div>
				<div class="overview-bar__tools">
					<span>{{ versionText }}</span>
					<span>更新于 {{ lastUpdatedText }}</span>
					<el-button type="primary" :loading="loading" @click="refreshDashboard">
						<el-icon><RefreshRight /></el-icon>
						刷新
					</el-button>
				</div>
			</div>

			<dl class="overview-metrics">
				<div v-for="item in overviewMetrics" :key="item.label" class="overview-metric">
					<dt>{{ item.label }}</dt>
					<dd>{{ item.value }}</dd>
					<small>{{ item.hint }}</small>
				</div>
			</dl>
		</header>

		<section class="dashboard-core">
			<article class="dashboard-panel dashboard-panel--trend">
				<header class="dashboard-panel__head">
					<div>
						<h2>消息总线趋势</h2>
						<p>最近 24 小时的发布与订阅结果，用于识别消息波动与失败时段。</p>
					</div>
					<div class="panel-facts">
						<span>节点 {{ formatCount(messageMetrics.servers) }}</span>
						<span>订阅端 {{ formatCount(messageMetrics.subscribers) }}</span>
					</div>
				</header>
				<div ref="messageChartRef" class="message-chart" aria-label="消息总线趋势图"></div>
				<div class="trend-summary">
					<div>
						<span>24 小时消息</span>
						<strong>{{ formatCount(messageTotal24h) }}</strong>
					</div>
					<div>
						<span>处理成功率</span>
						<strong>{{ percentText(messageSuccessRate) }}</strong>
					</div>
					<div>
						<span>失败消息</span>
						<strong :class="{ 'text-danger': messageFailureTotal > 0 }">{{ formatCount(messageFailureTotal) }}</strong>
					</div>
				</div>
			</article>

			<article class="dashboard-panel dashboard-panel--health">
				<header class="dashboard-panel__head">
					<div>
						<h2>平台健康</h2>
						<p>优先显示异常依赖与基础服务。</p>
					</div>
					<span class="health-state" :class="{ 'is-warning': hasUnhealthyChecks }">
						{{ hasUnhealthyChecks ? unhealthyChecksCount + ' 项待处理' : '检查通过' }}
					</span>
				</header>

				<div v-if="healthEntries.length" class="health-list">
					<div v-for="item in healthEntries" :key="item.name" class="health-item">
						<span class="health-item__indicator" :class="{ 'is-healthy': item.status === 'Healthy' }"></span>
						<div class="health-item__copy">
							<strong>{{ item.name }}</strong>
							<small>{{ item.description || '未提供补充说明' }}</small>
						</div>
						<span class="health-item__status">{{ healthStatusLabel(item.status) }}</span>
					</div>
				</div>
				<el-empty v-else description="暂无健康检查数据" :image-size="72" />
			</article>
		</section>

		<section class="dashboard-lower">
			<article class="dashboard-panel capability-panel">
				<header class="dashboard-panel__head">
					<div>
						<h2>平台能力</h2>
						<p>按接入采集、实时规则和运维闭环组织日常工作入口。</p>
					</div>
				</header>
				<div class="capability-groups">
					<section v-for="group in capabilityGroups" :key="group.title" class="capability-group">
						<div class="capability-group__title">
							<el-icon><component :is="group.icon" /></el-icon>
							<div>
								<h3>{{ group.title }}</h3>
								<p>{{ group.description }}</p>
							</div>
						</div>
						<nav class="capability-links" :aria-label="group.title">
							<button v-for="link in group.links" :key="link.path" type="button" @click="openRoute(link.path)">
								<span>{{ link.label }}</span>
								<el-icon><ArrowRight /></el-icon>
							</button>
						</nav>
					</section>
				</div>
			</article>

			<article class="dashboard-panel attention-panel">
				<header class="dashboard-panel__head">
					<div>
						<h2>当前关注</h2>
						<p>根据真实运行数据生成的处置入口。</p>
					</div>
				</header>
				<div class="attention-list">
					<button
						v-for="item in attentionItems"
						:key="item.label"
						type="button"
						class="attention-item"
						:class="'tone-' + item.tone"
						:disabled="!item.path"
						@click="item.path && openRoute(item.path)"
					>
						<span class="attention-item__value">{{ item.value }}</span>
						<span class="attention-item__copy">
							<strong>{{ item.label }}</strong>
							<small>{{ item.hint }}</small>
						</span>
						<el-icon v-if="item.path"><ArrowRight /></el-icon>
					</button>
				</div>
			</article>
		</section>
	</div>
</template>

<script lang="ts" setup>
import dayjs from 'dayjs';
import * as echarts from 'echarts';
import type { EChartsOption } from 'echarts';
import { computed, nextTick, onActivated, onBeforeUnmount, onMounted, ref } from 'vue';
import { useRouter } from 'vue-router';
import { ArrowRight, Bell, Box, Connection, Cpu, RefreshRight, SetUp } from '@element-plus/icons-vue';
import { ElMessage } from 'element-plus';
import { storeToRefs } from 'pinia';
import { useAppInfo } from '/@/stores/appInfo';
import { getHealthChecks, getKanban, getMessageInfo } from '/@/api/dashboard';

interface KanbanData {
	eventCount: number;
	onlineDeviceCount: number;
	attributesDataCount: number;
	deviceCount: number;
	alarmsCount: number;
	userCount: number;
	ProductCount: number;
	rulesCount: number;
}

interface MessageMetrics {
	servers: number;
	subscribers: number;
	publishedSucceeded: number;
	receivedSucceeded: number;
	publishedFailed: number;
	receivedFailed: number;
	dayHour: string[];
	publishSuccessed: number[];
	publishFailed: number[];
	subscribeSuccessed: number[];
	subscribeFailed: number[];
}

interface HealthEntry {
	name: string;
	status: string;
	description: string;
	duration: string;
}

const router = useRouter();
const storesAppInfo = useAppInfo();
const { appInfo } = storeToRefs(storesAppInfo);
const messageChartRef = ref<HTMLDivElement>();
const chartInstance = ref<echarts.ECharts | null>(null);
const loading = ref(false);
const lastUpdated = ref<Date | null>(null);
const healthEntries = ref<HealthEntry[]>([]);
const kanban = ref<KanbanData>({
	eventCount: 0,
	onlineDeviceCount: 0,
	attributesDataCount: 0,
	deviceCount: 0,
	alarmsCount: 0,
	userCount: 0,
	ProductCount: 0,
	rulesCount: 0,
});
const messageMetrics = ref<MessageMetrics>({
	servers: 0,
	subscribers: 0,
	publishedSucceeded: 0,
	receivedSucceeded: 0,
	publishedFailed: 0,
	receivedFailed: 0,
	dayHour: [],
	publishSuccessed: [],
	publishFailed: [],
	subscribeSuccessed: [],
	subscribeFailed: [],
});

const versionText = computed(() => (appInfo.value.version ? '版本 ' + appInfo.value.version : '自托管部署'));
const lastUpdatedText = computed(() => (lastUpdated.value ? dayjs(lastUpdated.value).format('HH:mm:ss') : '尚未同步'));
const offlineDevices = computed(() => Math.max(kanban.value.deviceCount - kanban.value.onlineDeviceCount, 0));
const onlineRate = computed(() => ratio(kanban.value.onlineDeviceCount, kanban.value.deviceCount));
const healthyChecksCount = computed(() => healthEntries.value.filter((item) => item.status === 'Healthy').length);
const unhealthyChecksCount = computed(() => Math.max(healthEntries.value.length - healthyChecksCount.value, 0));
const hasUnhealthyChecks = computed(() => unhealthyChecksCount.value > 0);
const messageFailureTotal = computed(() => messageMetrics.value.publishedFailed + messageMetrics.value.receivedFailed);
const messageTotal24h = computed(
	() =>
		sumValues(messageMetrics.value.publishSuccessed) +
		sumValues(messageMetrics.value.publishFailed) +
		sumValues(messageMetrics.value.subscribeSuccessed) +
		sumValues(messageMetrics.value.subscribeFailed)
);
const messageSuccessRate = computed(() =>
	ratio(
		messageMetrics.value.publishedSucceeded + messageMetrics.value.receivedSucceeded,
		messageMetrics.value.publishedSucceeded + messageMetrics.value.receivedSucceeded + messageFailureTotal.value
	)
);

const overviewMetrics = computed(() => [
	{ label: '设备总数', value: formatCount(kanban.value.deviceCount), hint: '运行实例' },
	{ label: '在线设备', value: formatCount(kanban.value.onlineDeviceCount), hint: percentText(onlineRate.value) + ' 在线' },
	{ label: '产品', value: formatCount(kanban.value.ProductCount), hint: '能力模板' },
	{ label: '实时规则', value: formatCount(kanban.value.rulesCount), hint: '规则链' },
	{ label: '告警设备', value: formatCount(kanban.value.alarmsCount), hint: '当前告警' },
	{ label: '平台事件', value: formatCount(kanban.value.eventCount), hint: '事件记录' },
]);

const capabilityGroups = [
	{
		title: '接入与采集',
		description: '建模业务对象，管理设备与边缘运行时。',
		icon: Connection,
		links: [
			{ label: '产品', path: '/iot/product/productlist' },
			{ label: '资产', path: '/iot/assets/assetlist' },
			{ label: '设备', path: '/iot/devices/devicelist' },
			{ label: 'Edge 节点', path: '/iot/devices/edgelist' },
		],
	},
	{
		title: '实时规则',
		description: '设计实时规则并查看处理事件。',
		icon: SetUp,
		links: [
			{ label: '规则链', path: '/iot/rules/flowlist' },
			{ label: '规则审计', path: '/iot/rules/flowevents' },
			{ label: '设备告警', path: '/iot/alarms/alarmlist' },
		],
	},
	{
		title: '运维与发布',
		description: '围绕边缘任务、配置版本与运行诊断闭环。',
		icon: Cpu,
		links: [
			{ label: 'Edge 任务', path: '/iot/devices/edgetasks' },
			{ label: 'Edge 节点', path: '/iot/devices/edgelist' },
		],
	},
];

const attentionItems = computed(() => {
	const items = [];
	if (hasUnhealthyChecks.value) {
		items.push({ label: '健康检查异常', value: unhealthyChecksCount.value + ' 项', hint: '检查依赖与基础服务', tone: 'danger', path: '' });
	}
	if (offlineDevices.value > 0) {
		items.push({ label: '离线设备', value: formatCount(offlineDevices.value) + ' 台', hint: '进入设备列表核查连接', tone: 'warning', path: '/iot/devices/devicelist' });
	}
	if (messageFailureTotal.value > 0) {
		items.push({ label: '失败消息', value: formatCount(messageFailureTotal.value) + ' 条', hint: '查看规则审计与消息时段', tone: 'warning', path: '/iot/rules/flowevents' });
	}
	if (kanban.value.alarmsCount > 0) {
		items.push({ label: '设备告警', value: formatCount(kanban.value.alarmsCount) + ' 台', hint: '进入告警列表处置', tone: 'danger', path: '/iot/alarms/alarmlist' });
	}
	if (!items.length) {
		items.push({ label: '暂无待处理异常', value: '正常', hint: '当前采集到的运行指标未发现异常', tone: 'success', path: '' });
	}
	return items;
});

const formatCount = (value: number) => new Intl.NumberFormat('zh-CN').format(value || 0);
const percentText = (value: number) => value.toFixed(1) + '%';
const ratio = (value: number, total: number) => (total ? (value / total) * 100 : 0);
const sumValues = (values: number[]) => values.reduce((sum, current) => sum + current, 0);
const healthStatusLabel = (status: string) => (status === 'Healthy' ? '正常' : status === 'Degraded' ? '降级' : '异常');
const openRoute = (path: string) => void router.push(path);

function normalizeHealthEntries(payload: any): HealthEntry[] {
	const latestExecution = Array.isArray(payload) ? payload[0] : undefined;
	const rawEntries = latestExecution?.entries ? Object.entries(latestExecution.entries) : [];
	return rawEntries
		.map(([name, entry]: [string, any]) => ({
			name,
			status: entry?.status ?? 'Unknown',
			description: entry?.description ?? '',
			duration: entry?.duration ?? '',
		}))
		.sort((left, right) => Number(left.status === 'Healthy') - Number(right.status === 'Healthy'));
}

function themeColor(name: string, fallback: string) {
	return getComputedStyle(document.documentElement).getPropertyValue(name).trim() || fallback;
}

function buildLineSeries(name: string, data: number[], color: string) {
	return {
		name,
		type: 'line',
		smooth: true,
		showSymbol: false,
		lineStyle: { width: 2, color },
		areaStyle: {
			color: new echarts.graphic.LinearGradient(0, 0, 0, 1, [
				{ offset: 0, color: color + '30' },
				{ offset: 1, color: color + '03' },
			]),
		},
		data,
	};
}

function renderChart() {
	if (!messageChartRef.value) return;
	chartInstance.value?.dispose();
	chartInstance.value = echarts.init(messageChartRef.value);
	const accent = themeColor('--iotsharp-accent', '#1E5B4F');
	const text = themeColor('--iotsharp-text-muted', '#71807C');
	const border = themeColor('--iotsharp-border', '#DFE7E4');
	const option: EChartsOption = {
		animationDuration: 420,
		tooltip: { trigger: 'axis' },
		legend: { top: 0, textStyle: { color: text, fontSize: 11 } },
		grid: { top: 42, right: 12, bottom: 12, left: 12, containLabel: true },
		xAxis: {
			type: 'category',
			boundaryGap: false,
			data: messageMetrics.value.dayHour,
			axisLine: { lineStyle: { color: border } },
			axisLabel: { color: text, fontSize: 10 },
		},
		yAxis: {
			type: 'value',
			axisLabel: { color: text, fontSize: 10 },
			splitLine: { lineStyle: { color: border, type: 'dashed' } },
		},
		series: [
			buildLineSeries('发布成功', messageMetrics.value.publishSuccessed, accent),
			buildLineSeries('发布失败', messageMetrics.value.publishFailed, '#A85D00'),
			buildLineSeries('订阅成功', messageMetrics.value.subscribeSuccessed, '#157347'),
			buildLineSeries('订阅失败', messageMetrics.value.subscribeFailed, '#BD3E3E'),
		],
	};
	chartInstance.value.setOption(option);
}

function resizeChart() {
	void nextTick(() => chartInstance.value?.resize());
}

async function fetchDashboardData() {
	loading.value = true;
	try {
		const [kanbanRes, messageRes, healthRes] = await Promise.all([getKanban(), getMessageInfo(), getHealthChecks()]);
		kanban.value = { ...kanban.value, ...kanbanRes.data };
		messageMetrics.value = { ...messageMetrics.value, ...messageRes.data };
		healthEntries.value = normalizeHealthEntries(healthRes);
		lastUpdated.value = new Date();
		await nextTick();
		renderChart();
	} catch {
		ElMessage.error('运行概览加载失败，请稍后重试');
	} finally {
		loading.value = false;
	}
}

const refreshDashboard = () => fetchDashboardData();
const onThemeChange = () => void nextTick(renderChart);

onMounted(async () => {
	await fetchDashboardData();
	window.addEventListener('resize', resizeChart);
	window.addEventListener('iotsharp-theme-change', onThemeChange);
});

onActivated(resizeChart);
onBeforeUnmount(() => {
	window.removeEventListener('resize', resizeChart);
	window.removeEventListener('iotsharp-theme-change', onThemeChange);
	chartInstance.value?.dispose();
});
</script>

<style scoped lang="scss">
.ops-dashboard {
	display: flex;
	flex-direction: column;
	gap: 14px;
	color: var(--iotsharp-text);
}

.overview-bar {
	padding: 18px 20px 0;
	border: 1px solid rgba(var(--iotsharp-accent-rgb), 0.14);
	border-radius: var(--iotsharp-radius-panel);
	background: var(--iotsharp-quiet-gradient);
}

.overview-bar__head,
.dashboard-panel__head,
.overview-bar__tools,
.panel-facts {
	display: flex;
	align-items: center;
	justify-content: space-between;
	gap: 12px;
}

.overview-bar__copy h1 {
	margin: 0;
	color: var(--iotsharp-ink);
	font-size: 23px;
	font-weight: 700;
	letter-spacing: 0;
}

.overview-bar__copy p,
.dashboard-panel__head p,
.capability-group__title p {
	margin: 5px 0 0;
	color: var(--iotsharp-text-soft);
	font-size: 12px;
	line-height: 1.55;
}

.overview-bar__tools {
	justify-content: flex-end;
	color: var(--iotsharp-text-muted);
	font-size: 11px;
}

.overview-bar__tools .el-button {
	height: 34px;
	border-radius: var(--iotsharp-radius-control);
}

.overview-metrics {
	display: grid;
	grid-template-columns: repeat(6, minmax(0, 1fr));
	margin: 16px -20px 0;
	border-top: 1px solid rgba(var(--iotsharp-accent-rgb), 0.12);
}

.overview-metric {
	min-width: 0;
	padding: 13px 18px 15px;
	border-left: 1px solid rgba(var(--iotsharp-accent-rgb), 0.12);
}

.overview-metric:first-child {
	border-left: 0;
}

.overview-metric dt,
.trend-summary span {
	color: var(--iotsharp-text-muted);
	font-size: 10px;
}

.overview-metric dd {
	margin: 3px 0 0;
	color: var(--iotsharp-ink);
	font-size: 20px;
	font-weight: 700;
	font-variant-numeric: tabular-nums;
}

.overview-metric small {
	display: block;
	margin-top: 1px;
	color: var(--iotsharp-text-soft);
	font-size: 10px;
}

.dashboard-core,
.dashboard-lower {
	display: grid;
	grid-template-columns: minmax(0, 2fr) minmax(300px, 1fr);
	gap: 14px;
}

.dashboard-panel {
	min-width: 0;
	border: 1px solid var(--iotsharp-border);
	border-radius: var(--iotsharp-radius-panel);
	background: var(--iotsharp-surface);
	box-shadow: var(--iotsharp-shadow-panel);
}

.dashboard-panel__head {
	min-height: 62px;
	align-items: flex-start;
	padding: 13px 15px;
	border-bottom: 1px solid var(--iotsharp-border);
}

.dashboard-panel__head h2 {
	margin: 0;
	color: var(--iotsharp-ink);
	font-size: 15px;
	font-weight: 680;
	letter-spacing: 0;
}

.panel-facts {
	justify-content: flex-end;
	color: var(--iotsharp-text-muted);
	font-size: 10px;
}

.panel-facts span {
	padding: 5px 8px;
	border-radius: 999px;
	background: var(--iotsharp-surface-muted);
}

.message-chart {
	width: 100%;
	height: 300px;
	padding: 8px 8px 0;
}

.trend-summary {
	display: grid;
	grid-template-columns: repeat(3, minmax(0, 1fr));
	border-top: 1px solid var(--iotsharp-border);
}

.trend-summary > div {
	padding: 11px 15px;
	border-left: 1px solid var(--iotsharp-border);
}

.trend-summary > div:first-child {
	border-left: 0;
}

.trend-summary strong {
	display: block;
	margin-top: 2px;
	color: var(--iotsharp-ink);
	font-size: 16px;
	font-variant-numeric: tabular-nums;
}

.text-danger {
	color: var(--iotsharp-danger) !important;
}

.health-state {
	display: inline-flex;
	min-height: 25px;
	align-items: center;
	padding: 0 8px;
	border-radius: 999px;
	background: var(--iotsharp-success-surface);
	color: var(--iotsharp-success);
	font-size: 10px;
	font-weight: 650;
}

.health-state.is-warning {
	background: var(--iotsharp-warning-surface);
	color: var(--iotsharp-warning);
}

.health-list {
	padding: 4px 0;
}

.health-item {
	display: grid;
	grid-template-columns: 8px minmax(0, 1fr) auto;
	align-items: center;
	gap: 10px;
	min-height: 52px;
	padding: 8px 14px;
	border-bottom: 1px solid var(--iotsharp-border);
}

.health-item:last-child {
	border-bottom: 0;
}

.health-item__indicator {
	width: 7px;
	height: 7px;
	border-radius: 50%;
	background: var(--iotsharp-danger);
}

.health-item__indicator.is-healthy {
	background: var(--iotsharp-success);
}

.health-item__copy {
	display: grid;
	min-width: 0;
	gap: 2px;
}

.health-item__copy strong {
	overflow: hidden;
	color: var(--iotsharp-text);
	font-size: 12px;
	text-overflow: ellipsis;
	white-space: nowrap;
}

.health-item__copy small,
.health-item__status {
	color: var(--iotsharp-text-muted);
	font-size: 10px;
}

.capability-groups {
	display: grid;
	grid-template-columns: repeat(3, minmax(0, 1fr));
}

.capability-group {
	min-width: 0;
	padding: 15px;
	border-left: 1px solid var(--iotsharp-border);
}

.capability-group:first-child {
	border-left: 0;
}

.capability-group__title {
	display: grid;
	grid-template-columns: 30px minmax(0, 1fr);
	gap: 9px;
}

.capability-group__title > .el-icon {
	width: 30px;
	height: 30px;
	border-radius: 6px;
	background: var(--iotsharp-selection);
	color: var(--iotsharp-accent);
}

.capability-group__title h3 {
	margin: 0;
	color: var(--iotsharp-ink);
	font-size: 13px;
}

.capability-links {
	display: grid;
	gap: 2px;
	margin-top: 12px;
}

.capability-links button {
	display: flex;
	width: 100%;
	min-height: 32px;
	align-items: center;
	justify-content: space-between;
	padding: 0 8px;
	border: 0;
	border-radius: 4px;
	background: transparent;
	color: var(--iotsharp-text-soft);
	font: inherit;
	font-size: 11px;
	cursor: pointer;
}

.capability-links button:hover,
.capability-links button:focus-visible {
	background: var(--iotsharp-surface-muted);
	color: var(--iotsharp-accent);
}

.attention-list {
	display: grid;
	padding: 5px 0;
}

.attention-item {
	display: grid;
	grid-template-columns: 64px minmax(0, 1fr) 16px;
	align-items: center;
	gap: 10px;
	min-height: 58px;
	padding: 8px 14px;
	border: 0;
	border-bottom: 1px solid var(--iotsharp-border);
	background: transparent;
	color: inherit;
	font: inherit;
	text-align: left;
	cursor: pointer;
}

.attention-item:last-child {
	border-bottom: 0;
}

.attention-item:not(:disabled):hover {
	background: var(--iotsharp-surface-muted);
}

.attention-item:disabled {
	cursor: default;
}

.attention-item__value {
	font-size: 12px;
	font-weight: 700;
	font-variant-numeric: tabular-nums;
}

.tone-success .attention-item__value {
	color: var(--iotsharp-success);
}

.tone-warning .attention-item__value {
	color: var(--iotsharp-warning);
}

.tone-danger .attention-item__value {
	color: var(--iotsharp-danger);
}

.attention-item__copy {
	display: grid;
	min-width: 0;
	gap: 2px;
}

.attention-item__copy strong {
	font-size: 11px;
}

.attention-item__copy small {
	color: var(--iotsharp-text-muted);
	font-size: 10px;
	line-height: 1.4;
}

@media (max-width: 1160px) {
	.overview-metrics {
		grid-template-columns: repeat(3, minmax(0, 1fr));
	}

	.overview-metric:nth-child(4) {
		border-left: 0;
	}

	.dashboard-core,
	.dashboard-lower {
		grid-template-columns: 1fr;
	}
}

@media (max-width: 760px) {
	.overview-bar {
		padding: 15px 15px 0;
	}

	.overview-bar__head,
	.overview-bar__tools {
		align-items: flex-start;
		flex-direction: column;
	}

	.overview-bar__tools {
		width: 100%;
	}

	.overview-metrics {
		grid-template-columns: repeat(2, minmax(0, 1fr));
		margin: 14px -15px 0;
	}

	.overview-metric:nth-child(odd) {
		border-left: 0;
	}

	.message-chart {
		height: 250px;
	}

	.capability-groups {
		grid-template-columns: 1fr;
	}

	.capability-group {
		border-top: 1px solid var(--iotsharp-border);
		border-left: 0;
	}

	.capability-group:first-child {
		border-top: 0;
	}
}
</style>
