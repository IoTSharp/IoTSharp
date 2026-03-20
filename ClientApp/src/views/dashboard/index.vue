<template>
	<div class="dashboard-page" v-loading="loading">
		<section class="dashboard-hero">
			<div class="dashboard-hero__content">
				<div class="dashboard-hero__eyebrow">IoT Operations Center</div>
				<div class="dashboard-hero__heading">
					<div>
						<Breadcrumb class="dashboard-hero__breadcrumb" />
						<p>把设备在线、消息吞吐、规则规模和平台健康放到同一个视图里，便于值守与分析。</p>
					</div>
					<div class="dashboard-hero__version">{{ versionText }}</div>
				</div>
				<div class="dashboard-hero__meta">
					<div class="dashboard-hero__meta-item">
						<span>在线率</span>
						<strong>{{ percentText(onlineRate) }}</strong>
					</div>
					<div class="dashboard-hero__meta-item">
						<span>健康通过率</span>
						<strong>{{ percentText(healthRate) }}</strong>
					</div>
					<div class="dashboard-hero__meta-item">
						<span>24h 消息量</span>
						<strong>{{ formatCount(messageTotal24h) }}</strong>
					</div>
					<div class="dashboard-hero__meta-item">
						<span>最近更新</span>
						<strong>{{ lastUpdatedText }}</strong>
					</div>
				</div>
			</div>
			<div class="dashboard-hero__aside">
				<div class="hero-kpi">
					<span class="hero-kpi__label">设备连接</span>
					<strong class="hero-kpi__value">{{ formatCount(kanban.onlineDeviceCount) }}/{{ formatCount(kanban.deviceCount) }}</strong>
					<p class="hero-kpi__hint">离线 {{ formatCount(offlineDevices) }} 台</p>
				</div>
				<div class="hero-kpi">
					<span class="hero-kpi__label">消息节点</span>
					<strong class="hero-kpi__value">{{ formatCount(messageMetrics.servers) }}</strong>
					<p class="hero-kpi__hint">订阅器 {{ formatCount(messageMetrics.subscribers) }}</p>
				</div>
				<div class="hero-kpi">
					<span class="hero-kpi__label">告警压力</span>
					<strong class="hero-kpi__value">{{ formatCount(kanban.alarmsCount) }}</strong>
					<p class="hero-kpi__hint">覆盖 {{ percentText(alarmRate) }} 的设备</p>
				</div>
			</div>
		</section>

		<el-row :gutter="18" class="dashboard-summary">
			<home-card-item v-for="item in summaryCards" :key="item.label" :item="item" />
		</el-row>

		<section class="dashboard-grid">
			<article class="dashboard-panel dashboard-panel--trend">
				<div class="dashboard-panel__header">
					<div>
						<div class="dashboard-panel__eyebrow">Message Trend</div>
						<h2>消息总线趋势</h2>
						<p>按小时观察发布与订阅成功、失败曲线，快速定位吞吐和异常波动。</p>
					</div>
					<div class="dashboard-chip-group">
						<span class="dashboard-chip">
							<CheckIcon class="dashboard-chip__icon" />
							发布成功 {{ formatCount(messageMetrics.publishedSucceeded) }}
						</span>
						<span class="dashboard-chip">
							<ConnectionIcon class="dashboard-chip__icon" />
							接收成功 {{ formatCount(messageMetrics.receivedSucceeded) }}
						</span>
					</div>
				</div>
				<div ref="messageChartRef" class="dashboard-chart dashboard-chart--xl"></div>
			</article>

			<article class="dashboard-panel dashboard-panel--status">
				<div class="dashboard-panel__header">
					<div>
						<div class="dashboard-panel__eyebrow">Platform Pulse</div>
						<h2>运行态势</h2>
						<p>把连接、健康、告警三类核心指标收敛成可快速判断的状态面板。</p>
					</div>
				</div>
				<div class="status-metrics">
					<div v-for="item in statusMetrics" :key="item.label" class="status-metric">
						<div class="status-metric__line">
							<span>{{ item.label }}</span>
							<strong>{{ item.value }}</strong>
						</div>
						<el-progress :percentage="item.percentage" :stroke-width="10" :show-text="false" :color="item.color" />
						<p>{{ item.hint }}</p>
					</div>
				</div>
				<div class="status-grid">
					<div class="status-grid__item">
						<span>消息失败</span>
						<strong>{{ formatCount(messageFailureTotal) }}</strong>
					</div>
					<div class="status-grid__item">
						<span>规则总数</span>
						<strong>{{ formatCount(kanban.rulesCount) }}</strong>
					</div>
					<div class="status-grid__item">
						<span>产品模型</span>
						<strong>{{ formatCount(kanban.produceCount) }}</strong>
					</div>
					<div class="status-grid__item">
						<span>健康项</span>
						<strong>{{ healthyChecksCount }}/{{ healthEntries.length }}</strong>
					</div>
				</div>
			</article>

			<article class="dashboard-panel dashboard-panel--availability">
				<div class="dashboard-panel__header">
					<div>
						<div class="dashboard-panel__eyebrow">Availability</div>
						<h2>设备可用性</h2>
						<p>用更直接的方式展示在线与离线分布，替代旧版依赖固定示例值的展示方式。</p>
					</div>
				</div>
				<div ref="onlineChartRef" class="dashboard-chart"></div>
				<div class="breakdown-list">
					<div class="breakdown-item">
						<div class="breakdown-item__label">
							<span class="breakdown-item__dot breakdown-item__dot--online"></span>
							在线设备
						</div>
						<strong>{{ formatCount(kanban.onlineDeviceCount) }}</strong>
					</div>
					<div class="breakdown-item">
						<div class="breakdown-item__label">
							<span class="breakdown-item__dot breakdown-item__dot--offline"></span>
							离线设备
						</div>
						<strong>{{ formatCount(offlineDevices) }}</strong>
					</div>
				</div>
			</article>

			<article class="dashboard-panel dashboard-panel--resource">
				<div class="dashboard-panel__header">
					<div>
						<div class="dashboard-panel__eyebrow">Resource Mix</div>
						<h2>资源构成</h2>
						<p>把设备、产品、规则、用户等关键资源放在同一尺度下，帮助判断平台建设重心。</p>
					</div>
				</div>
				<div ref="resourceChartRef" class="dashboard-chart"></div>
			</article>

			<article class="dashboard-panel dashboard-panel--health">
				<div class="dashboard-panel__header">
					<div>
						<div class="dashboard-panel__eyebrow">Health Checks</div>
						<h2>平台健康检查</h2>
						<p>按项展示基础设施与依赖状态，当前健康项会优先靠前显示。</p>
					</div>
					<div class="dashboard-chip-group">
						<span class="dashboard-chip" :class="{ 'dashboard-chip--warn': hasUnhealthyChecks }">
							<CircleCheckFilled v-if="!hasUnhealthyChecks" class="dashboard-chip__icon" />
							<WarningFilled v-else class="dashboard-chip__icon" />
							{{ hasUnhealthyChecks ? '存在待处理健康项' : '所有健康项正常' }}
						</span>
					</div>
				</div>
				<div v-if="healthEntries.length" class="health-list">
					<div v-for="item in healthEntries" :key="item.name" class="health-item">
						<div class="health-item__main">
							<div class="health-item__status" :class="`health-item__status--${item.status.toLowerCase()}`">
								<CircleCheckFilled v-if="item.status === 'Healthy'" />
								<CircleCloseFilled v-else />
							</div>
							<div>
								<div class="health-item__name">{{ item.name }}</div>
								<div class="health-item__desc">{{ item.description || '暂无额外描述' }}</div>
							</div>
						</div>
						<div class="health-item__meta">
							<span class="health-item__tag" :class="`health-item__tag--${item.status.toLowerCase()}`">{{ item.status }}</span>
							<span v-if="item.duration">{{ item.duration }}</span>
						</div>
					</div>
				</div>
				<el-empty v-else description="暂无健康检查数据" :image-size="80" />
			</article>
		</section>
	</div>
</template>

<script lang="ts" setup>
import dayjs from 'dayjs';
import * as echarts from 'echarts';
import { computed, nextTick, onActivated, onBeforeUnmount, onMounted, ref, watch } from 'vue';
import { storeToRefs } from 'pinia';
import type { EChartsOption } from 'echarts';
import CheckIcon from '~icons/ic/round-check';
import ConnectionIcon from '~icons/mdi/connection';
import HomeCardItem from '/@/views/dashboard/HomeCardItem.vue';
import Breadcrumb from '/@/layout/navBars/breadcrumb/breadcrumb.vue';
import { homeCardItemsConfig, type HomeCardMetricKey } from '/@/views/dashboard/homeCardItems';
import { useThemeConfig } from '/@/stores/themeConfig';
import { useAppInfo } from '/@/stores/appInfo';
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

type ChartKey = 'message' | 'online' | 'resource';

const messageChartRef = ref();
const onlineChartRef = ref();
const resourceChartRef = ref();

const storesThemeConfig = useThemeConfig();
const storesAppInfo = useAppInfo();
const { themeConfig } = storeToRefs(storesThemeConfig);
const { appInfo } = storeToRefs(storesAppInfo);

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
	produceCount: 0,
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

const chartInstances: Record<ChartKey, echarts.ECharts | null> = {
	message: null,
	online: null,
	resource: null,
};

const versionText = computed(() => (appInfo.value.version ? `Version ${appInfo.value.version}` : 'Self-hosted'));
const lastUpdatedText = computed(() => (lastUpdated.value ? dayjs(lastUpdated.value).format('YYYY-MM-DD HH:mm:ss') : '未同步'));

const offlineDevices = computed(() => Math.max(kanban.value.deviceCount - kanban.value.onlineDeviceCount, 0));
const onlineRate = computed(() => ratio(kanban.value.onlineDeviceCount, kanban.value.deviceCount));
const alarmRate = computed(() => ratio(kanban.value.alarmsCount, kanban.value.deviceCount));
const healthyChecksCount = computed(() => healthEntries.value.filter((item) => item.status === 'Healthy').length);
const healthRate = computed(() => ratio(healthyChecksCount.value, healthEntries.value.length));
const hasUnhealthyChecks = computed(() => healthEntries.value.some((item) => item.status !== 'Healthy'));

const messageTotal24h = computed(
	() =>
		sumValues(messageMetrics.value.publishSuccessed) +
		sumValues(messageMetrics.value.publishFailed) +
		sumValues(messageMetrics.value.subscribeSuccessed) +
		sumValues(messageMetrics.value.subscribeFailed)
);

const messageFailureTotal = computed(() => messageMetrics.value.publishedFailed + messageMetrics.value.receivedFailed);

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
		alarmsCount: `设备覆盖率 ${percentText(alarmRate.value)}`,
		userCount: `每位用户管理 ${averageText(kanban.value.deviceCount, kanban.value.userCount)} 台`,
		produceCount: `支撑设备 ${averageText(kanban.value.deviceCount, kanban.value.produceCount)} 台`,
		rulesCount: `每产品规则 ${averageText(kanban.value.rulesCount, kanban.value.produceCount)}`,
	};

	return homeCardItemsConfig.slice(0, 4).map((item) => ({
		...item,
		value: formatCount(values[item.key]),
		hint: hints[item.key],
	}));
});

const statusMetrics = computed(() => [
	{
		label: '连接稳定度',
		value: percentText(onlineRate.value),
		percentage: onlineRate.value,
		color: '#4f46e5',
		hint: `在线 ${formatCount(kanban.value.onlineDeviceCount)} / 总数 ${formatCount(kanban.value.deviceCount)}`,
	},
	{
		label: '平台健康度',
		value: percentText(healthRate.value),
		percentage: healthRate.value,
		color: '#10b981',
		hint: `健康项 ${healthyChecksCount.value} / ${healthEntries.value.length || 0}`,
	},
	{
		label: '告警压力',
		value: percentText(alarmRate.value),
		percentage: Math.min(alarmRate.value, 100),
		color: '#f59e0b',
		hint: `${formatCount(kanban.value.alarmsCount)} 台设备处于告警中`,
	},
	{
		label: '消息处理成功率',
		value: percentText(messageSuccessRate.value),
		percentage: messageSuccessRate.value,
		color: '#0ea5e9',
		hint: `失败 ${formatCount(messageFailureTotal.value)} 条`,
	},
]);

const messageSuccessRate = computed(() => {
	const success = messageMetrics.value.publishedSucceeded + messageMetrics.value.receivedSucceeded;
	const total = success + messageFailureTotal.value;
	return ratio(success, total);
});

function formatCount(value: number) {
	return new Intl.NumberFormat('zh-CN').format(value || 0);
}

function percentText(value: number) {
	return `${value.toFixed(1)}%`;
}

function averageText(total: number, divisor: number) {
	if (!divisor) return '0';
	return (total / divisor).toFixed(total / divisor >= 100 ? 0 : 1);
}

function ratio(value: number, total: number) {
	if (!total) return 0;
	return (value / total) * 100;
}

function sumValues(values: number[]) {
	return values.reduce((sum, current) => sum + current, 0);
}

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
		.sort((left, right) => Number(left.status !== 'Healthy') - Number(right.status !== 'Healthy'));
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

function renderCharts() {
	const axisLabelColor = themeConfig.value.isIsDark ? '#cbd5e1' : '#64748b';
	const primaryTextColor = themeConfig.value.isIsDark ? '#f8fafc' : '#0f172a';
	const splitLineColor = themeConfig.value.isIsDark ? 'rgba(148, 163, 184, 0.18)' : 'rgba(148, 163, 184, 0.16)';

	initChart('message', messageChartRef, {
		tooltip: {
			trigger: 'axis',
			backgroundColor: themeConfig.value.isIsDark ? '#0f172a' : '#ffffff',
			borderColor: themeConfig.value.isIsDark ? 'rgba(148, 163, 184, 0.16)' : '#e2e8f0',
			textStyle: {
				color: primaryTextColor,
			},
		},
		legend: {
			top: 0,
			textStyle: {
				color: axisLabelColor,
			},
		},
		grid: {
			top: 52,
			right: 18,
			bottom: 18,
			left: 18,
			containLabel: true,
		},
		xAxis: {
			type: 'category',
			boundaryGap: false,
			data: messageMetrics.value.dayHour,
			axisLine: { lineStyle: { color: splitLineColor } },
			axisLabel: { color: axisLabelColor },
		},
		yAxis: {
			type: 'value',
			axisLabel: { color: axisLabelColor },
			splitLine: { lineStyle: { color: splitLineColor, type: 'dashed' } },
		},
		series: [
			buildLineSeries('发布成功', messageMetrics.value.publishSuccessed, '#4f46e5'),
			buildLineSeries('发布失败', messageMetrics.value.publishFailed, '#f97316'),
			buildLineSeries('订阅成功', messageMetrics.value.subscribeSuccessed, '#14b8a6'),
			buildLineSeries('订阅失败', messageMetrics.value.subscribeFailed, '#ef4444'),
		],
	});

	initChart('online', onlineChartRef, {
		tooltip: {
			trigger: 'item',
		},
		title: {
			text: percentText(onlineRate.value),
			subtext: '在线率',
			left: 'center',
			top: '41%',
			textStyle: {
				color: primaryTextColor,
				fontSize: 24,
				fontWeight: 700,
			},
			subtextStyle: {
				color: axisLabelColor,
				fontSize: 13,
			},
		},
		series: [
			{
				type: 'pie',
				radius: ['66%', '82%'],
				center: ['50%', '48%'],
				label: { show: false },
				labelLine: { show: false },
				data: [
					{ value: kanban.value.onlineDeviceCount, name: '在线设备', itemStyle: { color: '#4f46e5' } },
					{ value: offlineDevices.value, name: '离线设备', itemStyle: { color: '#dbe4f0' } },
				],
			},
		],
	});

	initChart('resource', resourceChartRef, {
		grid: {
			top: 8,
			right: 20,
			bottom: 8,
			left: 70,
			containLabel: true,
		},
		xAxis: {
			type: 'value',
			splitLine: { lineStyle: { color: splitLineColor, type: 'dashed' } },
			axisLabel: { color: axisLabelColor },
		},
		yAxis: {
			type: 'category',
			axisTick: { show: false },
			axisLine: { show: false },
			axisLabel: { color: axisLabelColor },
			data: ['设备', '在线', '产品', '规则', '用户', '告警'],
		},
		series: [
			{
				type: 'bar',
				barWidth: 18,
				data: [
					{ value: kanban.value.deviceCount, itemStyle: { color: '#4f46e5' } },
					{ value: kanban.value.onlineDeviceCount, itemStyle: { color: '#10b981' } },
					{ value: kanban.value.produceCount, itemStyle: { color: '#14b8a6' } },
					{ value: kanban.value.rulesCount, itemStyle: { color: '#f59e0b' } },
					{ value: kanban.value.userCount, itemStyle: { color: '#0ea5e9' } },
					{ value: kanban.value.alarmsCount, itemStyle: { color: '#ef4444' } },
				],
				label: {
					show: true,
					position: 'right',
					color: primaryTextColor,
				},
				itemStyle: {
					borderRadius: [0, 10, 10, 0],
				},
			},
		],
	});
}

function buildLineSeries(name: string, data: number[], color: string) {
	return {
		name,
		type: 'line',
		smooth: true,
		showSymbol: false,
		lineStyle: {
			width: 3,
			color,
		},
		areaStyle: {
			color: new echarts.graphic.LinearGradient(0, 0, 0, 1, [
				{ offset: 0, color: `${color}33` },
				{ offset: 1, color: `${color}05` },
			]),
		},
		data,
	};
}

function resizeCharts() {
	nextTick(() => {
		(Object.keys(chartInstances) as ChartKey[]).forEach((key) => {
			chartInstances[key]?.resize();
		});
	});
}

async function fetchDashboardData() {
	loading.value = true;
	try {
		const [kanbanRes, messageRes, healthRes] = await Promise.all([getKanban(), getMessageInfo(), getHealthChecks()]);
		kanban.value = {
			...kanban.value,
			...kanbanRes.data,
		};
		messageMetrics.value = {
			...messageMetrics.value,
			...messageRes.data,
		};
		healthEntries.value = normalizeHealthEntries(healthRes);
		lastUpdated.value = new Date();
		await nextTick();
		renderCharts();
	}
	catch (error) {
		ElMessage.error('首页数据加载失败');
	}
	finally {
		loading.value = false;
	}
}

onMounted(async () => {
	await fetchDashboardData();
	window.addEventListener('resize', resizeCharts);
});

onActivated(() => {
	resizeCharts();
});

onBeforeUnmount(() => {
	window.removeEventListener('resize', resizeCharts);
	disposeCharts();
});

watch(
	() => themeConfig.value.isIsDark,
	() => {
		nextTick(() => {
			renderCharts();
		});
	}
);
</script>

<style scoped lang="scss">
.dashboard-page {
	display: flex;
	flex-direction: column;
	gap: 18px;
	padding: 8px 6px 4px;
}

.dashboard-hero,
.dashboard-panel {
	position: relative;
	border: 1px solid rgba(224, 232, 242, 0.96);
	background: linear-gradient(180deg, rgba(255, 255, 255, 0.98), rgba(248, 251, 255, 0.96));
	box-shadow: 0 16px 36px rgba(15, 23, 42, 0.05);
	backdrop-filter: blur(12px);
}

.dashboard-hero {
	display: grid;
	grid-template-columns: 1fr;
	gap: 18px;
	padding: 28px 28px 24px;
	border-radius: 28px;
	overflow: hidden;

	&::after {
		position: absolute;
		right: -40px;
		top: -48px;
		width: 200px;
		height: 200px;
		content: '';
		border-radius: 50%;
		background: radial-gradient(circle, rgba(88, 100, 255, 0.12), transparent 68%);
	}
}

.dashboard-hero__content,
.dashboard-hero__aside {
	position: relative;
	z-index: 1;
}

.dashboard-hero__eyebrow,
.dashboard-panel__eyebrow {
	margin-bottom: 8px;
	color: #5b6b80;
	font-size: 12px;
	font-weight: 700;
	letter-spacing: 0.18em;
	text-transform: uppercase;
}

.dashboard-hero__heading {
	display: flex;
	align-items: flex-start;
	justify-content: space-between;
	gap: 16px;

	h1 {
		margin: 0 0 10px;
		color: #0f172a;
		font-size: clamp(28px, 4vw, 36px);
		line-height: 1.05;
		letter-spacing: -0.04em;
	}

	p {
		max-width: 760px;
		color: #67768a;
		font-size: 15px;
		line-height: 1.8;
	}
}

.dashboard-hero__breadcrumb {
	min-height: 30px;
	margin-bottom: 10px;
}

.dashboard-hero__version {
	padding: 8px 14px;
	border-radius: 999px;
	background: rgba(88, 100, 255, 0.08);
	color: #5864ff;
	font-size: 12px;
	font-weight: 600;
	white-space: nowrap;
}

.dashboard-hero__meta {
	display: grid;
	grid-template-columns: repeat(4, minmax(0, 1fr));
	gap: 14px;
	margin-top: 20px;
}

.dashboard-hero__meta-item,
.hero-kpi,
.status-grid__item {
	padding: 16px 18px;
	border-radius: 20px;
	background: linear-gradient(180deg, rgba(255, 255, 255, 1), rgba(249, 251, 255, 0.98));
	border: 1px solid rgba(229, 236, 244, 0.98);
}

.dashboard-hero__meta-item {
	display: flex;
	flex-direction: column;
	gap: 8px;

	span {
		color: #64748b;
		font-size: 13px;
	}

		strong {
			color: #0f172a;
			font-size: 20px;
			letter-spacing: -0.04em;
		}
}

.dashboard-hero__aside {
	display: grid;
	grid-template-columns: repeat(3, minmax(0, 1fr));
	gap: 14px;
}

.hero-kpi__label,
.status-grid__item span {
	color: #64748b;
	font-size: 13px;
}

.hero-kpi__value,
.status-grid__item strong {
	display: block;
	margin: 10px 0 8px;
	color: #0f172a;
	font-size: 24px;
	font-weight: 700;
	line-height: 1;
	letter-spacing: -0.05em;
}

.hero-kpi__hint {
	color: #475569;
	font-size: 13px;
}

.dashboard-summary {
	row-gap: 18px;
}

.dashboard-grid {
	display: grid;
	grid-template-columns: repeat(12, minmax(0, 1fr));
	gap: 18px;
}

.dashboard-panel {
	display: flex;
	flex-direction: column;
	gap: 18px;
	min-height: 100%;
	padding: 22px;
	border-radius: 24px;
}

.dashboard-panel--trend {
	grid-column: span 8;
}

.dashboard-panel--status {
	grid-column: span 4;
}

.dashboard-panel--availability {
	grid-column: span 4;
}

.dashboard-panel--resource {
	grid-column: span 8;
}

.dashboard-panel--health {
	grid-column: span 12;
}

.dashboard-panel__header {
	display: flex;
	align-items: flex-start;
	justify-content: space-between;
	gap: 18px;

	h2 {
		margin: 0 0 8px;
		color: #0f172a;
		font-size: 22px;
		letter-spacing: -0.03em;
	}

	p {
		margin: 0;
		color: #7b8794;
		font-size: 14px;
		line-height: 1.7;
	}
}

.dashboard-chip-group {
	display: flex;
	flex-wrap: wrap;
	gap: 10px;
	justify-content: flex-end;
}

.dashboard-chip {
	display: inline-flex;
	align-items: center;
	gap: 8px;
	padding: 9px 12px;
	border-radius: 999px;
	background: rgba(88, 100, 255, 0.08);
	color: #5864ff;
	font-size: 13px;
	font-weight: 600;
	white-space: nowrap;
}

.dashboard-chip--warn {
	background: rgba(245, 158, 11, 0.12);
	color: #b45309;
}

.dashboard-chip__icon {
	font-size: 14px;
}

.dashboard-chart {
	min-height: 260px;
	width: 100%;
}

.dashboard-chart--xl {
	min-height: 340px;
}

.status-metrics {
	display: flex;
	flex-direction: column;
	gap: 18px;
}

.status-metric {
	display: flex;
	flex-direction: column;
	gap: 10px;
}

.status-metric__line {
	display: flex;
	align-items: center;
	justify-content: space-between;
	gap: 12px;
	color: #475569;
	font-size: 14px;

	strong {
		color: #0f172a;
		font-size: 16px;
	}
}

.status-metric p {
	color: #64748b;
	font-size: 12px;
}

.status-grid {
	display: grid;
	grid-template-columns: repeat(2, minmax(0, 1fr));
	gap: 12px;
}

.breakdown-list {
	display: grid;
	grid-template-columns: repeat(2, minmax(0, 1fr));
	gap: 12px;
}

.breakdown-item {
	display: flex;
	align-items: center;
	justify-content: space-between;
	gap: 12px;
	padding: 14px 16px;
	border-radius: 18px;
	background: rgba(248, 250, 252, 0.92);
	border: 1px solid rgba(148, 163, 184, 0.12);

	strong {
		color: #0f172a;
		font-size: 20px;
	}
}

.breakdown-item__label {
	display: inline-flex;
	align-items: center;
	gap: 8px;
	color: #475569;
	font-size: 13px;
}

.breakdown-item__dot {
	width: 10px;
	height: 10px;
	border-radius: 50%;
}

.breakdown-item__dot--online {
	background: #4f46e5;
}

.breakdown-item__dot--offline {
	background: #cbd5e1;
}

.health-list {
	display: grid;
	grid-template-columns: repeat(2, minmax(0, 1fr));
	gap: 14px;
}

.health-item {
	display: flex;
	align-items: center;
	justify-content: space-between;
	gap: 16px;
	padding: 18px;
	border-radius: 20px;
	border: 1px solid rgba(148, 163, 184, 0.14);
	background: rgba(255, 255, 255, 0.7);
}

.health-item__main {
	display: flex;
	align-items: center;
	gap: 14px;
	min-width: 0;
}

.health-item__status {
	display: inline-flex;
	align-items: center;
	justify-content: center;
	width: 42px;
	height: 42px;
	border-radius: 14px;
	font-size: 18px;
	flex-shrink: 0;
}

.health-item__status--healthy {
	background: rgba(16, 185, 129, 0.12);
	color: #059669;
}

.health-item__status--unhealthy,
.health-item__status--degraded,
.health-item__status--unknown {
	background: rgba(239, 68, 68, 0.12);
	color: #dc2626;
}

.health-item__name {
	color: #0f172a;
	font-size: 15px;
	font-weight: 600;
}

.health-item__desc {
	margin-top: 6px;
	color: #64748b;
	font-size: 13px;
	line-height: 1.6;
}

.health-item__meta {
	display: flex;
	flex-direction: column;
	align-items: flex-end;
	gap: 10px;
	color: #64748b;
	font-size: 12px;
	white-space: nowrap;
}

.health-item__tag {
	padding: 7px 10px;
	border-radius: 999px;
	font-weight: 600;
}

.health-item__tag--healthy {
	background: rgba(16, 185, 129, 0.12);
	color: #059669;
}

.health-item__tag--unhealthy,
.health-item__tag--degraded,
.health-item__tag--unknown {
	background: rgba(239, 68, 68, 0.12);
	color: #dc2626;
}

@media (max-width: 1440px) {
	.dashboard-hero__meta {
		grid-template-columns: repeat(2, minmax(0, 1fr));
	}

	.dashboard-panel--trend,
	.dashboard-panel--resource {
		grid-column: span 7;
	}

	.dashboard-panel--status,
	.dashboard-panel--availability {
		grid-column: span 5;
	}
}

@media (max-width: 1100px) {
	.dashboard-grid {
		grid-template-columns: repeat(1, minmax(0, 1fr));
	}

	.dashboard-panel--trend,
	.dashboard-panel--status,
	.dashboard-panel--availability,
	.dashboard-panel--resource,
	.dashboard-panel--health {
		grid-column: span 1;
	}

	.health-list {
		grid-template-columns: repeat(1, minmax(0, 1fr));
	}
}

@media (max-width: 767px) {
	.dashboard-page {
		gap: 14px;
	}

	.dashboard-hero,
	.dashboard-panel {
		padding: 18px;
		border-radius: 20px;
	}

	.dashboard-hero__heading,
	.dashboard-panel__header {
		flex-direction: column;
	}

	.dashboard-hero__meta,
	.dashboard-hero__aside,
	.status-grid,
	.breakdown-list {
		grid-template-columns: repeat(1, minmax(0, 1fr));
	}

	.health-item {
		flex-direction: column;
		align-items: flex-start;
	}

	.health-item__meta {
		align-items: flex-start;
	}

	.dashboard-chart--xl {
		min-height: 300px;
	}
}
</style>
