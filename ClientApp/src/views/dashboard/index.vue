<template>
	<div class="workspace-page" v-loading="loading">
		<section class="workspace-hero">
			<div class="card hero-main">
				<div class="eyebrow">Workspace</div>
				<div class="hero-head">
					<div>
						<h1>IoTSharp 工作台</h1>
						<p>统一查看设备在线率、消息链路、平台健康和告警压力，作为登录后的运营入口。</p>
					</div>
					<div class="hero-tags">
						<span class="tag">{{ versionText }}</span>
						<span class="tag tag-muted">最近更新 {{ lastUpdatedText }}</span>
					</div>
				</div>
				<div class="hero-actions">
					<el-button type="primary" @click="refreshDashboard">
						<el-icon><RefreshRight /></el-icon>
						刷新面板
					</el-button>
					<el-button @click="openDocs">
						<el-icon><Reading /></el-icon>
						文档中心
					</el-button>
					<el-button @click="openGithub">
						<el-icon><Promotion /></el-icon>
						项目仓库
					</el-button>
				</div>
				<div class="hero-stats">
					<div v-for="item in heroStats" :key="item.label" class="hero-stat">
						<span>{{ item.label }}</span>
						<strong>{{ item.value }}</strong>
						<small>{{ item.hint }}</small>
					</div>
				</div>
			</div>

			<div class="hero-side">
				<div class="card side-card">
					<div class="side-top">
						<div>
							<div class="eyebrow">Focus</div>
							<h2>{{ workspaceHeadline.title }}</h2>
						</div>
						<span class="status-pill" :class="`is-${workspaceHeadline.tone}`">{{ workspaceHeadline.tag }}</span>
					</div>
					<p class="side-desc">{{ workspaceHeadline.description }}</p>
					<div class="notice-list">
						<div v-for="item in operationalNotices" :key="item.label" class="notice-item">
							<div class="notice-item__value">{{ item.value }}</div>
							<div class="notice-item__body">
								<strong>{{ item.label }}</strong>
								<small>{{ item.hint }}</small>
							</div>
						</div>
					</div>
				</div>

				<div class="card side-card">
					<div class="eyebrow">Quick Actions</div>
					<div class="quick-grid">
						<button v-for="action in quickActions" :key="action.key" type="button" class="quick-item" @click="onQuickAction(action)">
							<span class="quick-item__icon">
								<el-icon><component :is="action.icon" /></el-icon>
							</span>
							<strong>{{ action.label }}</strong>
							<small>{{ action.description }}</small>
						</button>
					</div>
				</div>
			</div>
		</section>

		<section class="summary-section">
			<div class="summary-head">
				<div>
					<div class="eyebrow">Overview</div>
					<h3>核心指标概览</h3>
					<p>保留最常看的四项平台数据，进入控制台后先看全局，再继续深入排查。</p>
				</div>
				<span class="summary-tag">实时同步</span>
			</div>
			<el-row :gutter="16" class="summary-row">
				<HomeCardItem v-for="(item, index) in summaryCards" :key="item.label" :item="item" :index="index" />
			</el-row>
		</section>

		<section class="workspace-grid">
			<article class="card panel panel-trend">
				<div class="panel-head">
					<div>
						<div class="eyebrow">Message Trend</div>
						<h3>消息总线趋势</h3>
						<p>按小时观察发布与订阅的成功、失败变化，用来快速发现消息波动。</p>
					</div>
					<div class="chip-row">
						<span class="chip">发布成功 {{ formatCount(messageMetrics.publishedSucceeded) }}</span>
						<span class="chip">接收成功 {{ formatCount(messageMetrics.receivedSucceeded) }}</span>
					</div>
				</div>
				<div ref="messageChartRef" class="chart chart-xl"></div>
			</article>

			<article class="card panel panel-status">
				<div class="panel-head">
					<div>
						<div class="eyebrow">Platform Pulse</div>
						<h3>运行状态</h3>
						<p>将连接、健康、告警和消息成功率汇总成一组快速判断指标。</p>
					</div>
				</div>
				<div class="status-list">
					<div v-for="item in statusMetrics" :key="item.label" class="status-item">
						<div class="status-item__line">
							<span>{{ item.label }}</span>
							<strong>{{ item.value }}</strong>
						</div>
						<el-progress :percentage="item.percentage" :stroke-width="10" :show-text="false" :color="item.color" />
						<p>{{ item.hint }}</p>
					</div>
				</div>
				<div class="mini-grid">
					<div v-for="item in systemOverviewItems" :key="item.label" class="mini-card">
						<span>{{ item.label }}</span>
						<strong>{{ item.value }}</strong>
						<small>{{ item.hint }}</small>
					</div>
				</div>
			</article>

			<article class="card panel panel-availability">
				<div class="panel-head">
					<div>
						<div class="eyebrow">Availability</div>
						<h3>设备可用率</h3>
						<p>直观看到在线与离线设备分布，便于追踪连接异常。</p>
					</div>
				</div>
				<div ref="onlineChartRef" class="chart"></div>
				<div class="breakdown-list">
					<div class="breakdown-item">
						<div class="breakdown-label"><span class="dot dot-online"></span>在线设备</div>
						<strong>{{ formatCount(kanban.onlineDeviceCount) }}</strong>
					</div>
					<div class="breakdown-item">
						<div class="breakdown-label"><span class="dot dot-offline"></span>离线设备</div>
						<strong>{{ formatCount(offlineDevices) }}</strong>
					</div>
				</div>
			</article>

			<article class="card panel panel-health">
				<div class="panel-head">
					<div>
						<div class="eyebrow">Health Checks</div>
						<h3>平台健康检查</h3>
						<p>优先展示异常依赖和基础设施状态，帮助快速定位问题。</p>
					</div>
					<div class="chip-row">
						<span class="chip" :class="{ 'chip-warn': hasUnhealthyChecks }">
							{{ hasUnhealthyChecks ? '存在待处理健康项' : '所有健康项正常' }}
						</span>
					</div>
				</div>
				<div v-if="healthEntries.length" class="health-list">
					<div v-for="item in healthEntries" :key="item.name" class="health-item">
						<div class="health-main">
							<div class="health-dot" :class="`health-dot--${item.status.toLowerCase()}`"></div>
							<div>
								<div class="health-name">{{ item.name }}</div>
								<div class="health-desc">{{ item.description || '暂无补充描述' }}</div>
							</div>
						</div>
						<div class="health-meta">
							<span class="health-tag" :class="`health-tag--${item.status.toLowerCase()}`">{{ item.status }}</span>
							<span v-if="item.duration">{{ item.duration }}</span>
						</div>
					</div>
				</div>
				<el-empty v-else description="暂无健康检查数据" :image-size="80" />
			</article>

			<article class="card panel panel-resource">
				<div class="panel-head">
					<div>
						<div class="eyebrow">Resource Mix</div>
						<h3>资源结构</h3>
						<p>从设备、产品、规则和用户数量上快速判断平台建设重点。</p>
					</div>
				</div>
				<div ref="resourceChartRef" class="chart"></div>
			</article>

			<article class="card panel panel-snapshot">
				<div class="panel-head">
					<div>
						<div class="eyebrow">Operations Snapshot</div>
						<h3>运营观察</h3>
						<p>把当前最值得关注的运营变化压缩成一组简明提示，方便日常巡检。</p>
					</div>
				</div>
				<div class="snapshot-list">
					<div v-for="item in snapshotItems" :key="item.label" class="snapshot-item" :class="`is-${item.tone}`">
						<span>{{ item.label }}</span>
						<strong>{{ item.value }}</strong>
						<p>{{ item.hint }}</p>
					</div>
				</div>
			</article>
		</section>
	</div>
</template>

<script lang="ts" setup>
import dayjs from 'dayjs';
import * as echarts from 'echarts';
import { computed, nextTick, onActivated, onBeforeUnmount, onMounted, ref, watch } from 'vue';
import { useRouter } from 'vue-router';
import { useI18n } from 'vue-i18n';
import { storeToRefs } from 'pinia';
import { ElMessage } from 'element-plus';
import type { EChartsOption } from 'echarts';
import { Bell, Connection, DataAnalysis, Monitor, Promotion, Reading, RefreshRight } from '@element-plus/icons-vue';
import HomeCardItem from '/@/views/dashboard/HomeCardItem.vue';
import { homeCardItemsConfig, type HomeCardMetricKey } from '/@/views/dashboard/homeCardItems';
import { useThemeConfig } from '/@/stores/themeConfig';
import { useAppInfo } from '/@/stores/appInfo';
import { useRoutesList } from '/@/stores/routesList';
import { getHealthChecks, getKanban, getMessageInfo } from '/@/api/dashboard';

interface KanbanData {
	eventCount: number;
	onlineDeviceCount: number;
	attributesDataCount: number;
	deviceCount: number;
	alarmsCount: number;
	userCount: number;
	produceCount: number;
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

interface QuickAction {
	key: string;
	label: string;
	description: string;
	icon: any;
	type: 'refresh' | 'external' | 'route';
	href?: string;
	path?: string;
}

type ChartKey = 'message' | 'online' | 'resource';

const messageChartRef = ref<HTMLDivElement>();
const onlineChartRef = ref<HTMLDivElement>();
const resourceChartRef = ref<HTMLDivElement>();

const router = useRouter();
const { t } = useI18n();
const storesThemeConfig = useThemeConfig();
const storesAppInfo = useAppInfo();
const storesRoutesList = useRoutesList();
const { themeConfig } = storeToRefs(storesThemeConfig);
const { appInfo } = storeToRefs(storesAppInfo);
const { routesList } = storeToRefs(storesRoutesList);

const loading = ref(false);
const lastUpdated = ref<Date | null>(null);
const healthEntries = ref<HealthEntry[]>([]);
const kanban = ref<KanbanData>({ eventCount: 0, onlineDeviceCount: 0, attributesDataCount: 0, deviceCount: 0, alarmsCount: 0, userCount: 0, produceCount: 0, rulesCount: 0 });
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

const chartInstances: Record<ChartKey, echarts.ECharts | null> = { message: null, online: null, resource: null };
const versionText = computed(() => (appInfo.value.version ? `版本 ${appInfo.value.version}` : '自托管部署'));
const lastUpdatedText = computed(() => (lastUpdated.value ? dayjs(lastUpdated.value).format('YYYY-MM-DD HH:mm:ss') : '尚未同步'));
const offlineDevices = computed(() => Math.max(kanban.value.deviceCount - kanban.value.onlineDeviceCount, 0));
const onlineRate = computed(() => ratio(kanban.value.onlineDeviceCount, kanban.value.deviceCount));
const alarmRate = computed(() => ratio(kanban.value.alarmsCount, kanban.value.deviceCount));
const healthyChecksCount = computed(() => healthEntries.value.filter((item) => item.status === 'Healthy').length);
const healthRate = computed(() => ratio(healthyChecksCount.value, healthEntries.value.length));
const hasUnhealthyChecks = computed(() => healthEntries.value.some((item) => item.status !== 'Healthy'));
const messageTotal24h = computed(() => sumValues(messageMetrics.value.publishSuccessed) + sumValues(messageMetrics.value.publishFailed) + sumValues(messageMetrics.value.subscribeSuccessed) + sumValues(messageMetrics.value.subscribeFailed));
const messageFailureTotal = computed(() => messageMetrics.value.publishedFailed + messageMetrics.value.receivedFailed);
const messageSuccessRate = computed(() => ratio(messageMetrics.value.publishedSucceeded + messageMetrics.value.receivedSucceeded, messageMetrics.value.publishedSucceeded + messageMetrics.value.receivedSucceeded + messageFailureTotal.value));

const heroStats = computed(() => [
	{ label: '设备在线率', value: percentText(onlineRate.value), hint: `在线 ${formatCount(kanban.value.onlineDeviceCount)} / 总数 ${formatCount(kanban.value.deviceCount)}` },
	{ label: '平台健康度', value: percentText(healthRate.value), hint: `健康项 ${healthyChecksCount.value} / ${healthEntries.value.length || 0}` },
	{ label: '24 小时消息', value: formatCount(messageTotal24h.value), hint: `成功率 ${percentText(messageSuccessRate.value)}` },
	{ label: '告警设备', value: formatCount(kanban.value.alarmsCount), hint: `覆盖率 ${percentText(alarmRate.value)}` },
]);

const workspaceHeadline = computed(() => {
	if (hasUnhealthyChecks.value) return { title: '存在待处理健康检查', description: `${healthEntries.value.length - healthyChecksCount.value} 个健康项异常，建议优先处理依赖问题。`, tag: '需要关注', tone: 'danger' };
	if (messageFailureTotal.value > 0) return { title: '消息链路出现波动', description: `当前有 ${formatCount(messageFailureTotal.value)} 条失败消息，需要结合总线趋势继续排查。`, tag: '持续观察', tone: 'warning' };
	return { title: '平台整体运行稳定', description: '连接、消息和健康检查状态正常，当前可以继续关注容量与业务增长。', tag: '运行良好', tone: 'success' };
});

const operationalNotices = computed(() => [
	{ label: '离线设备', value: formatCount(offlineDevices.value), hint: offlineDevices.value > 0 ? '建议优先核查最近掉线设备' : '当前没有离线设备需要处理' },
	{ label: '失败消息', value: formatCount(messageFailureTotal.value), hint: messageFailureTotal.value > 0 ? '需要检查失败波峰时段' : '最近消息处理稳定' },
	{ label: '消息节点', value: formatCount(messageMetrics.value.servers), hint: `订阅客户端 ${formatCount(messageMetrics.value.subscribers)}` },
	{ label: '规则规模', value: formatCount(kanban.value.rulesCount), hint: `产品模型 ${formatCount(kanban.value.produceCount)} 个` },
]);

const summaryCards = computed(() => {
	const values: Record<HomeCardMetricKey, number> = {
		deviceCount: kanban.value.deviceCount,
		onlineDeviceCount: kanban.value.onlineDeviceCount,
		attributesDataCount: kanban.value.attributesDataCount,
		eventCount: kanban.value.eventCount,
		alarmsCount: kanban.value.alarmsCount,
		userCount: kanban.value.userCount,
		produceCount: kanban.value.produceCount,
		rulesCount: kanban.value.rulesCount,
	};
	const hints: Record<HomeCardMetricKey, string> = {
		deviceCount: `在线 ${formatCount(kanban.value.onlineDeviceCount)} 台`,
		onlineDeviceCount: `离线 ${formatCount(offlineDevices.value)} 台`,
		attributesDataCount: `平均每设备 ${averageText(kanban.value.attributesDataCount, kanban.value.deviceCount)}`,
		eventCount: `规则规模 ${formatCount(kanban.value.rulesCount)}`,
		alarmsCount: `告警覆盖率 ${percentText(alarmRate.value)}`,
		userCount: `人均管理 ${averageText(kanban.value.deviceCount, kanban.value.userCount)} 台`,
		produceCount: `覆盖设备 ${averageText(kanban.value.deviceCount, kanban.value.produceCount)} 台`,
		rulesCount: `每产品规则 ${averageText(kanban.value.rulesCount, kanban.value.produceCount)}`,
	};
	return homeCardItemsConfig.slice(0, 4).map((item) => ({ ...item, value: formatCount(values[item.key]), hint: hints[item.key] }));
});

const statusMetrics = computed(() => [
	{ label: '连接稳定度', value: percentText(onlineRate.value), percentage: onlineRate.value, color: '#165dff', hint: `在线 ${formatCount(kanban.value.onlineDeviceCount)} / 总数 ${formatCount(kanban.value.deviceCount)}` },
	{ label: '健康通过率', value: percentText(healthRate.value), percentage: healthRate.value, color: '#00b42a', hint: `健康项 ${healthyChecksCount.value} / ${healthEntries.value.length || 0}` },
	{ label: '告警压力', value: percentText(alarmRate.value), percentage: Math.min(alarmRate.value, 100), color: '#ff7d00', hint: `${formatCount(kanban.value.alarmsCount)} 台设备处于告警中` },
	{ label: '消息成功率', value: percentText(messageSuccessRate.value), percentage: messageSuccessRate.value, color: '#0fc6c2', hint: `失败消息 ${formatCount(messageFailureTotal.value)} 条` },
]);

const systemOverviewItems = computed(() => [
	{ label: '消息节点', value: formatCount(messageMetrics.value.servers), hint: '消息服务器实例数' },
	{ label: '订阅客户端', value: formatCount(messageMetrics.value.subscribers), hint: '当前订阅终端规模' },
	{ label: '规则总数', value: formatCount(kanban.value.rulesCount), hint: '自动化流程与联动' },
	{ label: '产品模型', value: formatCount(kanban.value.produceCount), hint: '设备模型与模板' },
	{ label: '系统用户', value: formatCount(kanban.value.userCount), hint: '协同管理成员数' },
	{ label: '事件总量', value: formatCount(kanban.value.eventCount), hint: '近 24 小时平台事件' },
]);

const snapshotItems = computed(() => [
	{ label: '在线覆盖', value: percentText(onlineRate.value), hint: offlineDevices.value > 0 ? `仍有 ${formatCount(offlineDevices.value)} 台设备离线` : '当前设备全部在线', tone: offlineDevices.value > 0 ? 'warning' : 'success' },
	{ label: '告警范围', value: formatCount(kanban.value.alarmsCount), hint: kanban.value.alarmsCount > 0 ? '建议优先追踪高频告警源' : '当前没有告警压力', tone: kanban.value.alarmsCount > 0 ? 'warning' : 'success' },
	{ label: '健康风险', value: `${healthEntries.value.length - healthyChecksCount.value}`, hint: hasUnhealthyChecks.value ? '存在待处理健康项' : '健康检查全部通过', tone: hasUnhealthyChecks.value ? 'danger' : 'success' },
	{ label: '消息失败', value: formatCount(messageFailureTotal.value), hint: messageFailureTotal.value > 0 ? '需要关注失败波峰' : '消息处理稳定', tone: messageFailureTotal.value > 0 ? 'warning' : 'success' },
]);

const quickIcons = [Monitor, Connection, DataAnalysis, Bell];
const quickActions = computed<QuickAction[]>(() => [
	{ key: 'refresh', label: '刷新数据', description: '重新拉取首页接口', icon: RefreshRight, type: 'refresh' },
	{ key: 'docs', label: '文档中心', description: '查看部署与接入说明', icon: Reading, type: 'external', href: 'http://docs.iotsharp.net/' },
	{ key: 'github', label: '项目仓库', description: '访问 GitHub 源码', icon: Promotion, type: 'external', href: 'https://github.com/IoTSharp' },
	...flattenVisibleRoutes(routesList.value)
		.filter((item) => item.path && item.path !== '/dashboard')
		.slice(0, 3)
		.map((item, index) => ({
			key: `route-${item.path}`,
			label: normalizeRouteTitle(item.meta?.title),
			description: '进入常用业务页面',
			icon: quickIcons[index % quickIcons.length],
			type: 'route' as const,
			path: item.path,
		})),
]);

function flattenVisibleRoutes(routes: any[] = []): any[] {
	return routes.flatMap((item: any) => [...(item?.meta?.isHide ? [] : [item]), ...(Array.isArray(item?.children) ? flattenVisibleRoutes(item.children) : [])]);
}

function normalizeRouteTitle(title?: string) {
	return !title ? '业务入口' : title.startsWith('message.') ? t(title) : title;
}

function onQuickAction(action: QuickAction) {
	if (action.type === 'refresh') return refreshDashboard();
	if (action.type === 'external' && action.href) return window.open(action.href, '_blank');
	if (action.type === 'route' && action.path) router.push(action.path);
}

function openDocs() {
	window.open('http://docs.iotsharp.net/', '_blank');
}

function openGithub() {
	window.open('https://github.com/IoTSharp', '_blank');
}

async function refreshDashboard() {
	await fetchDashboardData();
}

function formatCount(value: number) {
	return new Intl.NumberFormat('zh-CN').format(value || 0);
}

function percentText(value: number) {
	return `${value.toFixed(1)}%`;
}

function averageText(total: number, divisor: number) {
	return divisor ? (total / divisor).toFixed(total / divisor >= 100 ? 0 : 1) : '0';
}

function ratio(value: number, total: number) {
	return total ? (value / total) * 100 : 0;
}

function sumValues(values: number[]) {
	return values.reduce((sum, current) => sum + current, 0);
}

function normalizeHealthEntries(payload: any): HealthEntry[] {
	const latestExecution = Array.isArray(payload) ? payload[0] : undefined;
	const rawEntries = latestExecution?.entries ? Object.entries(latestExecution.entries) : [];
	return rawEntries
		.map(([name, entry]: [string, any]) => ({ name, status: entry?.status ?? 'Unknown', description: entry?.description ?? '', duration: entry?.duration ?? '' }))
		.sort((left, right) => Number(left.status === 'Healthy') - Number(right.status === 'Healthy'));
}

function disposeCharts() {
	(Object.keys(chartInstances) as ChartKey[]).forEach((key) => {
		chartInstances[key]?.dispose();
		chartInstances[key] = null;
	});
}

function initChart(key: ChartKey, target: typeof messageChartRef, option: EChartsOption) {
	if (!target.value) return;
	chartInstances[key]?.dispose();
	chartInstances[key] = echarts.init(target.value, themeConfig.value.isIsDark ? 'dark' : undefined);
	chartInstances[key]?.setOption(option);
}

function buildLineSeries(name: string, data: number[], color: string) {
	return { name, type: 'line', smooth: true, showSymbol: false, lineStyle: { width: 3, color }, areaStyle: { color: new echarts.graphic.LinearGradient(0, 0, 0, 1, [{ offset: 0, color: `${color}33` }, { offset: 1, color: `${color}05` }]) }, data };
}

function renderCharts() {
	const axisLabelColor = themeConfig.value.isIsDark ? '#cbd5e1' : '#6b7785';
	const primaryTextColor = themeConfig.value.isIsDark ? '#f8fafc' : '#1d2129';
	const splitLineColor = themeConfig.value.isIsDark ? 'rgba(148, 163, 184, 0.18)' : 'rgba(199, 210, 221, 0.42)';

	initChart('message', messageChartRef, {
		tooltip: { trigger: 'axis', backgroundColor: themeConfig.value.isIsDark ? '#0f172a' : '#ffffff', borderColor: themeConfig.value.isIsDark ? 'rgba(148, 163, 184, 0.18)' : '#e5e6eb', textStyle: { color: primaryTextColor } },
		legend: { top: 0, textStyle: { color: axisLabelColor } },
		grid: { top: 52, right: 18, bottom: 18, left: 18, containLabel: true },
		xAxis: { type: 'category', boundaryGap: false, data: messageMetrics.value.dayHour, axisLine: { lineStyle: { color: splitLineColor } }, axisLabel: { color: axisLabelColor } },
		yAxis: { type: 'value', axisLabel: { color: axisLabelColor }, splitLine: { lineStyle: { color: splitLineColor, type: 'dashed' } } },
		series: [
			buildLineSeries('发布成功', messageMetrics.value.publishSuccessed, '#165dff'),
			buildLineSeries('发布失败', messageMetrics.value.publishFailed, '#ff7d00'),
			buildLineSeries('订阅成功', messageMetrics.value.subscribeSuccessed, '#00b42a'),
			buildLineSeries('订阅失败', messageMetrics.value.subscribeFailed, '#f53f3f'),
		],
	});

	initChart('online', onlineChartRef, {
		tooltip: { trigger: 'item' },
		title: { text: percentText(onlineRate.value), subtext: '在线率', left: 'center', top: '41%', textStyle: { color: primaryTextColor, fontSize: 24, fontWeight: 700 }, subtextStyle: { color: axisLabelColor, fontSize: 13 } },
		series: [{ type: 'pie', radius: ['66%', '82%'], center: ['50%', '48%'], label: { show: false }, labelLine: { show: false }, data: [{ value: kanban.value.onlineDeviceCount, name: '在线设备', itemStyle: { color: '#165dff' } }, { value: offlineDevices.value, name: '离线设备', itemStyle: { color: '#dbeafe' } }] }],
	});

	initChart('resource', resourceChartRef, {
		grid: { top: 8, right: 20, bottom: 8, left: 70, containLabel: true },
		xAxis: { type: 'value', splitLine: { lineStyle: { color: splitLineColor, type: 'dashed' } }, axisLabel: { color: axisLabelColor } },
		yAxis: { type: 'category', axisTick: { show: false }, axisLine: { show: false }, axisLabel: { color: axisLabelColor }, data: ['设备', '在线', '产品', '规则', '用户', '告警'] },
		series: [{
			type: 'bar',
			barWidth: 18,
			data: [
				{ value: kanban.value.deviceCount, itemStyle: { color: '#165dff' } },
				{ value: kanban.value.onlineDeviceCount, itemStyle: { color: '#00b42a' } },
				{ value: kanban.value.produceCount, itemStyle: { color: '#0fc6c2' } },
				{ value: kanban.value.rulesCount, itemStyle: { color: '#722ed1' } },
				{ value: kanban.value.userCount, itemStyle: { color: '#3491fa' } },
				{ value: kanban.value.alarmsCount, itemStyle: { color: '#ff7d00' } },
			],
			label: { show: true, position: 'right', color: primaryTextColor },
			itemStyle: { borderRadius: [0, 10, 10, 0] },
		}],
	});
}

function resizeCharts() {
	nextTick(() => (Object.keys(chartInstances) as ChartKey[]).forEach((key) => chartInstances[key]?.resize()));
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
		renderCharts();
	} catch (error) {
		ElMessage.error('首页数据加载失败');
	} finally {
		loading.value = false;
	}
}

onMounted(async () => {
	await fetchDashboardData();
	window.addEventListener('resize', resizeCharts);
});

onActivated(() => resizeCharts());
onBeforeUnmount(() => {
	window.removeEventListener('resize', resizeCharts);
	disposeCharts();
});

watch(
	() => themeConfig.value.isIsDark,
	() => nextTick(() => renderCharts())
);
</script>

<style scoped lang="scss">
.workspace-page {
	display: flex;
	flex-direction: column;
	gap: 16px;
}

.card {
	border: 1px solid #e5e6eb;
	border-radius: 24px;
	background: #fff;
	box-shadow: 0 10px 24px rgba(15, 23, 42, 0.04);
}

.workspace-hero {
	display: grid;
	grid-template-columns: minmax(0, 1.65fr) minmax(320px, 0.95fr);
	gap: 16px;
}

.hero-main,
.side-card,
.panel {
	padding: 22px;
}

.hero-main {
	position: relative;
	overflow: hidden;
	background:
		radial-gradient(circle at top right, rgba(22, 93, 255, 0.12), transparent 34%),
		linear-gradient(135deg, #f7fbff 0%, #eef5ff 55%, #f9fbff 100%);
}

.hero-main::after {
	position: absolute;
	right: -48px;
	bottom: -70px;
	width: 220px;
	height: 220px;
	content: '';
	border-radius: 50%;
	background: radial-gradient(circle, rgba(14, 165, 233, 0.16), transparent 68%);
}

.eyebrow {
	margin-bottom: 8px;
	color: #86909c;
	font-size: 12px;
	font-weight: 700;
	letter-spacing: 0.16em;
	text-transform: uppercase;
}

.hero-head,
.panel-head,
.side-top {
	display: flex;
	align-items: flex-start;
	justify-content: space-between;
	gap: 16px;
}

.hero-head h1,
.side-top h2,
.panel-head h3 {
	margin: 0 0 8px;
	color: #1d2129;
	letter-spacing: -0.03em;
}

.hero-head h1 {
	font-size: clamp(30px, 4vw, 40px);
	line-height: 1.05;
}

.side-top h2 {
	font-size: 24px;
}

.panel-head h3 {
	font-size: 22px;
}

.hero-head p,
.panel-head p,
.side-desc {
	margin: 0;
	color: #4e5969;
	font-size: 14px;
	line-height: 1.75;
}

.hero-tags,
.hero-actions,
.chip-row {
	display: flex;
	flex-wrap: wrap;
	gap: 10px;
}

.tag,
.chip,
.status-pill {
	display: inline-flex;
	align-items: center;
	border-radius: 999px;
	font-size: 12px;
	font-weight: 600;
	white-space: nowrap;
}

.tag {
	height: 36px;
	padding: 0 14px;
	background: rgba(22, 93, 255, 0.1);
	color: #165dff;
}

.tag-muted {
	background: rgba(255, 255, 255, 0.86);
	color: #4e5969;
	border: 1px solid rgba(229, 230, 235, 0.9);
}

.hero-actions {
	position: relative;
	z-index: 1;
	margin-top: 20px;
}

.hero-actions .el-button {
	height: 38px;
	padding: 0 16px;
	border-radius: 12px;
}

.hero-stats {
	position: relative;
	z-index: 1;
	display: grid;
	grid-template-columns: repeat(4, minmax(0, 1fr));
	gap: 14px;
	margin-top: 22px;
}

.hero-stat,
.mini-card,
.breakdown-item,
.snapshot-item {
	padding: 16px;
	border-radius: 18px;
	border: 1px solid rgba(229, 230, 235, 0.92);
	background: rgba(255, 255, 255, 0.92);
}

.hero-stat span,
.mini-card span,
.snapshot-item span {
	display: block;
	color: #4e5969;
	font-size: 13px;
}

.hero-stat strong,
.mini-card strong,
.snapshot-item strong {
	display: block;
	margin-top: 10px;
	color: #1d2129;
	font-weight: 700;
	letter-spacing: -0.04em;
}

.hero-stat strong {
	font-size: 24px;
}

.hero-stat small,
.mini-card small,
.snapshot-item p {
	display: block;
	margin-top: 6px;
	color: #86909c;
	font-size: 12px;
	line-height: 1.6;
}

.hero-side,
.notice-list,
.status-list {
	display: grid;
	gap: 16px;
}

.status-pill {
	height: 30px;
	padding: 0 12px;
	font-weight: 700;
}

.is-success {
	background: rgba(0, 180, 42, 0.12);
	color: #009a29;
}

.is-warning {
	background: rgba(255, 125, 0, 0.12);
	color: #d25f00;
}

.is-danger {
	background: rgba(245, 63, 63, 0.12);
	color: #cb2634;
}

.notice-item,
.quick-item,
.health-item {
	display: flex;
	gap: 14px;
	padding: 14px 16px;
	border-radius: 18px;
	border: 1px solid rgba(229, 230, 235, 0.92);
	background: #fff;
}

.notice-item__value {
	min-width: 72px;
	color: #165dff;
	font-size: 22px;
	font-weight: 700;
	letter-spacing: -0.04em;
}

.notice-item__body,
.health-main {
	display: flex;
	gap: 14px;
	min-width: 0;
}

.notice-item__body {
	flex-direction: column;
	gap: 4px;
}

.notice-item__body strong,
.quick-item strong,
.health-name {
	color: #1d2129;
	font-size: 14px;
	font-weight: 600;
}

.notice-item__body small,
.quick-item small,
.health-desc,
.status-item p {
	color: #86909c;
	font-size: 12px;
	line-height: 1.6;
}

.quick-grid,
.mini-grid,
.snapshot-list,
.status-grid,
.breakdown-list {
	display: grid;
	grid-template-columns: repeat(2, minmax(0, 1fr));
	gap: 12px;
}

.quick-item {
	flex-direction: column;
	align-items: flex-start;
	text-align: left;
	cursor: pointer;
	transition:
		transform 0.2s ease,
		border-color 0.2s ease,
		box-shadow 0.2s ease;
}

.quick-item:hover {
	transform: translateY(-2px);
	border-color: rgba(22, 93, 255, 0.26);
	box-shadow: 0 12px 24px rgba(22, 93, 255, 0.08);
}

.quick-item__icon {
	display: inline-flex;
	align-items: center;
	justify-content: center;
	width: 40px;
	height: 40px;
	border-radius: 14px;
	background: rgba(22, 93, 255, 0.1);
	color: #165dff;
	font-size: 18px;
}

.summary-row {
	row-gap: 16px;
}

.summary-section {
	display: flex;
	flex-direction: column;
	gap: 14px;
}

.summary-head {
	display: flex;
	align-items: flex-end;
	justify-content: space-between;
	gap: 16px;
}

.summary-head h3 {
	margin: 0 0 8px;
	color: #1d2129;
	font-size: 22px;
	letter-spacing: -0.03em;
}

.summary-head p {
	margin: 0;
	color: #4e5969;
	font-size: 14px;
	line-height: 1.75;
}

.summary-tag {
	display: inline-flex;
	align-items: center;
	height: 34px;
	padding: 0 14px;
	border-radius: 999px;
	border: 1px solid rgba(229, 230, 235, 0.96);
	background: rgba(255, 255, 255, 0.9);
	color: #4e5969;
	font-size: 12px;
	font-weight: 600;
	white-space: nowrap;
}

.workspace-grid {
	display: grid;
	grid-template-columns: repeat(12, minmax(0, 1fr));
	gap: 16px;
}

.panel {
	display: flex;
	flex-direction: column;
	gap: 18px;
}

.panel-trend {
	grid-column: span 8;
}

.panel-status {
	grid-column: span 4;
}

.panel-availability {
	grid-column: span 4;
}

.panel-health {
	grid-column: span 8;
}

.panel-resource {
	grid-column: span 7;
}

.panel-snapshot {
	grid-column: span 5;
}

.chip {
	padding: 9px 12px;
	background: rgba(22, 93, 255, 0.08);
	color: #165dff;
}

.chip-warn {
	background: rgba(255, 125, 0, 0.12);
	color: #d25f00;
}

.chart {
	min-height: 260px;
	width: 100%;
}

.chart-xl {
	min-height: 340px;
}

.status-item {
	display: flex;
	flex-direction: column;
	gap: 10px;
}

.status-item__line {
	display: flex;
	align-items: center;
	justify-content: space-between;
	gap: 12px;
	color: #4e5969;
	font-size: 14px;
}

.status-item__line strong,
.breakdown-item strong {
	color: #1d2129;
	font-size: 18px;
}

.mini-card {
	background: #f9fbff;
}

.breakdown-item {
	display: flex;
	align-items: center;
	justify-content: space-between;
	gap: 12px;
	background: #f9fbff;
}

.breakdown-label {
	display: inline-flex;
	align-items: center;
	gap: 8px;
	color: #4e5969;
	font-size: 13px;
}

.dot,
.health-dot {
	width: 10px;
	height: 10px;
	border-radius: 50%;
	flex-shrink: 0;
}

.dot-online {
	background: #165dff;
}

.dot-offline {
	background: #dbeafe;
}

.health-list {
	display: grid;
	grid-template-columns: repeat(2, minmax(0, 1fr));
	gap: 14px;
}

.health-main {
	align-items: center;
}

.health-dot {
	width: 14px;
	height: 14px;
}

.health-dot--healthy {
	background: #00b42a;
	box-shadow: 0 0 0 6px rgba(0, 180, 42, 0.12);
}

.health-dot--unhealthy,
.health-dot--degraded,
.health-dot--unknown {
	background: #f53f3f;
	box-shadow: 0 0 0 6px rgba(245, 63, 63, 0.12);
}

.health-meta {
	display: flex;
	flex-direction: column;
	align-items: flex-end;
	gap: 8px;
	color: #86909c;
	font-size: 12px;
	white-space: nowrap;
}

.health-tag {
	padding: 6px 10px;
	border-radius: 999px;
	font-weight: 600;
}

.health-tag--healthy {
	background: rgba(0, 180, 42, 0.12);
	color: #009a29;
}

.health-tag--unhealthy,
.health-tag--degraded,
.health-tag--unknown {
	background: rgba(245, 63, 63, 0.12);
	color: #cb2634;
}

.snapshot-item.is-success {
	background: linear-gradient(180deg, rgba(240, 255, 244, 0.7), #fff);
}

.snapshot-item.is-warning {
	background: linear-gradient(180deg, rgba(255, 247, 232, 0.8), #fff);
}

.snapshot-item.is-danger {
	background: linear-gradient(180deg, rgba(255, 236, 232, 0.82), #fff);
}

@media (max-width: 1440px) {
	.workspace-hero {
		grid-template-columns: 1fr;
	}

	.hero-stats {
		grid-template-columns: repeat(2, minmax(0, 1fr));
	}

	.panel-trend,
	.panel-health {
		grid-column: span 7;
	}

	.panel-status,
	.panel-availability {
		grid-column: span 5;
	}

	.panel-resource,
	.panel-snapshot {
		grid-column: span 6;
	}
}

@media (max-width: 1180px) {
	.workspace-grid {
		grid-template-columns: 1fr;
	}

	.panel-trend,
	.panel-status,
	.panel-availability,
	.panel-health,
	.panel-resource,
	.panel-snapshot {
		grid-column: span 1;
	}

	.health-list {
		grid-template-columns: 1fr;
	}
}

@media (max-width: 767px) {
	.card {
		border-radius: 20px;
	}

	.hero-main,
	.side-card,
	.panel {
		padding: 18px;
	}

	.hero-head,
	.panel-head,
	.side-top,
	.summary-head,
	.notice-item,
	.health-item {
		flex-direction: column;
	}

	.hero-tags,
	.chip-row {
		justify-content: flex-start;
	}

	.hero-stats,
	.quick-grid,
	.mini-grid,
	.snapshot-list,
	.breakdown-list {
		grid-template-columns: 1fr;
	}

	.health-meta {
		align-items: flex-start;
	}

	.chart-xl {
		min-height: 300px;
	}
}
</style>
