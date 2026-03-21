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
					<div class="focus-summary" :class="`focus-summary--${focusPrimaryAction.tone}`">
						<div class="focus-summary__head">
							<span class="focus-summary__label">{{ focusPrimaryAction.label }}</span>
							<span class="focus-summary__value">{{ focusPrimaryAction.value }}</span>
						</div>
						<strong>{{ focusPrimaryAction.title }}</strong>
						<small>{{ focusPrimaryAction.description }}</small>
					</div>
					<div class="focus-strip">
						<div v-for="item in focusSignals" :key="item.label" class="focus-item" :class="`focus-item--${item.tone}`">
							<span>{{ item.label }}</span>
							<strong>{{ item.value }}</strong>
							<small>{{ item.hint }}</small>
						</div>
					</div>
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
					<div class="side-card__head">
						<div>
							<div class="eyebrow">Quick Actions</div>
							<h3>常用入口</h3>
							<p>把刷新、文档和高频业务页集中到这里，进入控制台后可以直接继续处理事务。</p>
						</div>
						<span class="side-kicker">{{ quickActions.length }} 个入口</span>
					</div>
					<div class="quick-grid">
						<button v-for="action in quickActions" :key="action.key" type="button" class="quick-item" :style="getQuickActionStyle(action)" @click="onQuickAction(action)">
							<div class="quick-item__top">
								<span class="quick-item__icon">
									<el-icon><component :is="action.icon" /></el-icon>
								</span>
								<span class="quick-item__badge">{{ action.badge }}</span>
							</div>
							<strong>{{ action.label }}</strong>
							<small>{{ action.description }}</small>
							<span class="quick-item__meta">
								{{ action.meta }}
								<el-icon><ArrowRight /></el-icon>
							</span>
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
				<div class="trend-briefs">
					<div v-for="item in trendHighlights" :key="item.label" class="trend-brief">
						<span>{{ item.label }}</span>
						<strong>{{ item.value }}</strong>
						<small>{{ item.hint }}</small>
					</div>
				</div>
			</article>

			<article class="card panel panel-status">
				<div class="panel-head">
					<div>
						<div class="eyebrow">Platform Pulse</div>
						<h3>运行状态</h3>
						<p>将连接、健康、告警和消息成功率汇总成一组快速判断指标。</p>
					</div>
				</div>
				<div class="status-score">
					<div class="status-score__body">
						<div class="status-score__eyebrow">Platform Score</div>
						<h4>{{ systemScoreCard.title }}</h4>
						<p>{{ systemScoreCard.description }}</p>
						<span class="status-pill" :class="`is-${systemScoreCard.tone}`">{{ systemScoreCard.tag }}</span>
					</div>
					<div class="status-score__ring">
						<strong>{{ systemScoreCard.score }}</strong>
						<small>分</small>
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
				<div class="health-overview">
					<div v-for="item in healthSummaryItems" :key="item.label" class="health-overview__item" :class="`health-overview__item--${item.tone}`">
						<span>{{ item.label }}</span>
						<strong>{{ item.value }}</strong>
						<small>{{ item.hint }}</small>
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
				<div class="resource-list">
					<div v-for="item in resourceHighlights" :key="item.label" class="resource-item">
						<div class="resource-item__main">
							<span class="resource-item__dot" :style="{ background: item.color }"></span>
							<div>
								<strong>{{ item.label }}</strong>
								<small>{{ item.hint }}</small>
							</div>
						</div>
						<span class="resource-item__value">{{ item.value }}</span>
					</div>
				</div>
			</article>

			<article class="card panel panel-snapshot">
				<div class="panel-head">
					<div>
						<div class="eyebrow">Operations Snapshot</div>
						<h3>运营建议</h3>
						<p>把当前最需要处理的动作列成短清单，便于巡检后直接推进处置。</p>
					</div>
					<span class="summary-tag">今日待办 {{ actionRecommendations.length }}</span>
				</div>
				<div class="recommendation-list">
					<div v-for="item in actionRecommendations" :key="item.title" class="recommendation-item" :class="`tone-${item.tone}`">
						<div class="recommendation-item__head">
							<span class="recommendation-item__label">{{ item.label }}</span>
							<span class="recommendation-item__value">{{ item.value }}</span>
						</div>
						<strong>{{ item.title }}</strong>
						<p>{{ item.description }}</p>
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
import { ArrowRight, Bell, Connection, DataAnalysis, Monitor, Promotion, Reading, RefreshRight } from '@element-plus/icons-vue';
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
	badge: string;
	meta: string;
	icon: any;
	type: 'refresh' | 'external' | 'route';
	href?: string;
	path?: string;
}

interface RecommendationItem {
	label: string;
	value: string;
	title: string;
	description: string;
	tone: StatusTone;
}

type StatusTone = 'success' | 'warning' | 'danger';
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
const unhealthyChecksCount = computed(() => Math.max(healthEntries.value.length - healthyChecksCount.value, 0));
const healthRate = computed(() => ratio(healthyChecksCount.value, healthEntries.value.length));
const hasUnhealthyChecks = computed(() => healthEntries.value.some((item) => item.status !== 'Healthy'));
const messageTotal24h = computed(() => sumValues(messageMetrics.value.publishSuccessed) + sumValues(messageMetrics.value.publishFailed) + sumValues(messageMetrics.value.subscribeSuccessed) + sumValues(messageMetrics.value.subscribeFailed));
const messageFailureTotal = computed(() => messageMetrics.value.publishedFailed + messageMetrics.value.receivedFailed);
const messageFailureRate = computed(() => ratio(messageFailureTotal.value, messageMetrics.value.publishedSucceeded + messageMetrics.value.receivedSucceeded + messageFailureTotal.value));
const messageSuccessRate = computed(() => ratio(messageMetrics.value.publishedSucceeded + messageMetrics.value.receivedSucceeded, messageMetrics.value.publishedSucceeded + messageMetrics.value.receivedSucceeded + messageFailureTotal.value));
const lastUpdatedClockText = computed(() => (lastUpdated.value ? dayjs(lastUpdated.value).format('HH:mm:ss') : '--'));
const actionQueueCount = computed(() => {
	const count =
		Number(unhealthyChecksCount.value > 0) +
		Number(offlineDevices.value > 0) +
		Number(messageFailureTotal.value > 0) +
		Number(kanban.value.alarmsCount > 0);
	return count || 1;
});
const hourlyTraffic = computed(() =>
	messageMetrics.value.dayHour.map((hour, index) => ({
		hour,
		total:
			(messageMetrics.value.publishSuccessed[index] || 0) +
			(messageMetrics.value.publishFailed[index] || 0) +
			(messageMetrics.value.subscribeSuccessed[index] || 0) +
			(messageMetrics.value.subscribeFailed[index] || 0),
	}))
);
const peakTraffic = computed(() =>
	hourlyTraffic.value.reduce(
		(peak, current) => (current.total > peak.total ? current : peak),
		{ hour: '--', total: 0 }
	)
);
const platformScore = computed(() => {
	const score =
		onlineRate.value * 0.32 +
		healthRate.value * 0.3 +
		messageSuccessRate.value * 0.23 +
		Math.max(0, 100 - Math.min(alarmRate.value, 100)) * 0.15;
	return Math.round(score);
});

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

const systemScoreCard = computed(() => {
	if (platformScore.value >= 90) return { score: platformScore.value, title: '平台运行稳定', description: '连接质量、消息链路和健康检查保持在较高水位，当前可以把重点放到容量与业务扩展。', tag: '稳定', tone: 'success' as const };
	if (platformScore.value >= 75) return { score: platformScore.value, title: '平台总体可控', description: '基础服务运行正常，但仍有局部风险需要持续观察，建议优先处理当前预警项。', tag: '关注中', tone: 'warning' as const };
	return { score: platformScore.value, title: '平台需要干预', description: '核心指标出现明显波动，建议把健康项、离线设备和失败消息作为本轮巡检的优先级。', tag: '待处理', tone: 'danger' as const };
});

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

const resourceHighlights = computed(() => [
	{ label: '设备规模', value: formatCount(kanban.value.deviceCount), hint: '当前纳管终端总量', color: '#165dff' },
	{ label: '在线终端', value: formatCount(kanban.value.onlineDeviceCount), hint: '处于活跃连接的设备', color: '#00b42a' },
	{ label: '产品模型', value: formatCount(kanban.value.produceCount), hint: '设备模板与模型沉淀', color: '#0fc6c2' },
	{ label: '规则总数', value: formatCount(kanban.value.rulesCount), hint: '联动与自动化编排能力', color: '#722ed1' },
	{ label: '系统用户', value: formatCount(kanban.value.userCount), hint: '协同管理的成员规模', color: '#3491fa' },
	{ label: '告警设备', value: formatCount(kanban.value.alarmsCount), hint: '当前需要重点跟进的范围', color: '#ff7d00' },
]);

const trendHighlights = computed(() => [
	{ label: '波峰时段', value: peakTraffic.value.hour, hint: peakTraffic.value.total > 0 ? `峰值 ${formatCount(peakTraffic.value.total)} 条消息` : '等待流量数据同步' },
	{ label: '失败占比', value: percentText(messageFailureRate.value), hint: messageFailureTotal.value > 0 ? `近 24 小时失败 ${formatCount(messageFailureTotal.value)} 条` : '当前没有失败消息' },
	{
		label: '节点负载',
		value: messageMetrics.value.servers ? `${averageText(messageTotal24h.value, messageMetrics.value.servers)} 条` : '--',
		hint: messageMetrics.value.servers ? `${formatCount(messageMetrics.value.servers)} 个节点 / ${formatCount(messageMetrics.value.subscribers)} 个订阅端` : '暂无消息节点数据',
	},
]);

const healthSummaryItems = computed(() => [
	{ label: '通过项', value: formatCount(healthyChecksCount.value), hint: `共 ${healthEntries.value.length || 0} 项健康检查`, tone: 'success' as StatusTone },
	{ label: '待处理', value: formatCount(unhealthyChecksCount.value), hint: hasUnhealthyChecks.value ? '建议优先排查依赖与基础服务' : '当前没有待处理健康项', tone: hasUnhealthyChecks.value ? ('warning' as StatusTone) : ('success' as StatusTone) },
	{ label: '最近巡检', value: lastUpdatedClockText.value, hint: lastUpdated.value ? dayjs(lastUpdated.value).format('YYYY-MM-DD') : '等待同步', tone: 'success' as StatusTone },
]);

const actionRecommendations = computed<RecommendationItem[]>(() => {
	const items: RecommendationItem[] = [];
	if (hasUnhealthyChecks.value) {
		items.push({
			label: 'P1',
			value: `${unhealthyChecksCount.value} 项`,
			title: '处理健康检查异常',
			description: '优先检查失败依赖和基础服务，让平台先恢复到稳定状态。',
			tone: 'danger',
		});
	}
	if (offlineDevices.value > 0) {
		items.push({
			label: 'P2',
			value: `${formatCount(offlineDevices.value)} 台`,
			title: '跟进离线设备恢复',
			description: '建议先排查最近掉线和集中掉线的设备，尽快拉回在线覆盖率。',
			tone: 'warning',
		});
	}
	if (messageFailureTotal.value > 0) {
		items.push({
			label: 'P2',
			value: `${formatCount(messageFailureTotal.value)} 条`,
			title: '排查失败消息波峰',
			description: '结合消息总线趋势，重点检查失败消息出现最密集的时间段。',
			tone: 'warning',
		});
	}
	if (items.length < 3 && kanban.value.alarmsCount > 0) {
		items.push({
			label: 'P3',
			value: `${formatCount(kanban.value.alarmsCount)} 台`,
			title: '压降高频告警设备',
			description: '先收敛重复触发的告警源，降低值班噪音和排障干扰。',
			tone: 'warning',
		});
	}
	if (items.length < 3) {
		items.push({
			label: 'Routine',
			value: percentText(onlineRate.value),
			title: '复核平台容量与增长',
			description: '当前基础面可控，建议把产品模型、规则和设备增长放到本轮复盘里。',
			tone: 'success',
		});
	}
	if (items.length < 3) {
		items.push({
			label: 'Daily',
			value: lastUpdatedText.value,
			title: '同步今日巡检结论',
			description: '记录这次面板刷新后的结论，方便团队接力处理后续动作。',
			tone: 'success',
		});
	}
	return items.slice(0, 3);
});

const focusPrimaryAction = computed<RecommendationItem>(() => actionRecommendations.value[0] ?? { label: 'Daily', value: '--', title: '继续例行巡检', description: '当前没有需要升级处理的问题，可以继续复核容量、增长和业务趋势。', tone: 'success' });

const focusSignals = computed(() => [
	{ label: '平台评分', value: `${systemScoreCard.value.score} 分`, hint: systemScoreCard.value.tag, tone: systemScoreCard.value.tone },
	{ label: '待办事项', value: `${actionQueueCount.value} 项`, hint: focusPrimaryAction.value.title, tone: actionQueueCount.value > 1 ? ('warning' as StatusTone) : ('success' as StatusTone) },
	{ label: '波峰时段', value: peakTraffic.value.hour, hint: peakTraffic.value.total > 0 ? `峰值 ${formatCount(peakTraffic.value.total)} 条` : '等待流量数据', tone: messageFailureTotal.value > 0 ? ('warning' as StatusTone) : ('success' as StatusTone) },
]);

const quickIcons = [Monitor, Connection, DataAnalysis, Bell];
const quickActions = computed<QuickAction[]>(() => [
	{ key: 'refresh', label: '刷新数据', description: '重新拉取首页接口', badge: '同步', meta: '立即获取最新状态', icon: RefreshRight, type: 'refresh' },
	{ key: 'docs', label: '文档中心', description: '查看部署与接入说明', badge: '文档', meta: '部署与接入指南', icon: Reading, type: 'external', href: 'http://docs.iotsharp.net/' },
	{ key: 'github', label: '项目仓库', description: '访问 GitHub 源码', badge: '开源', meta: '查看项目源码', icon: Promotion, type: 'external', href: 'https://github.com/IoTSharp' },
	...flattenVisibleRoutes(routesList.value)
		.filter((item) => item.path && item.path !== '/dashboard')
		.slice(0, 3)
		.map((item, index) => ({
			key: `route-${item.path}`,
			label: normalizeRouteTitle(item.meta?.title),
			description: '进入常用业务页面',
			badge: '业务',
			meta: '控制台直达',
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

function getQuickActionStyle(action: QuickAction) {
	const colorMap: Record<QuickAction['type'], string> = {
		refresh: '#165dff',
		external: '#0fc6c2',
		route: '#ff7d00',
	};
	const color = colorMap[action.type];
	return {
		'--quick-accent': color,
		'--quick-accent-soft': `${color}14`,
		'--quick-accent-border': `${color}32`,
	};
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
.breakdown-item {
	padding: 16px;
	border-radius: 18px;
	border: 1px solid rgba(229, 230, 235, 0.92);
	background: rgba(255, 255, 255, 0.92);
}

.hero-stat span,
.mini-card span {
	display: block;
	color: #4e5969;
	font-size: 13px;
}

.hero-stat strong,
.mini-card strong {
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
.mini-card small {
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

.side-card__head {
	display: flex;
	align-items: flex-start;
	justify-content: space-between;
	gap: 16px;
}

.side-card__head h3 {
	margin: 0 0 8px;
	color: #1d2129;
	font-size: 22px;
	letter-spacing: -0.03em;
}

.side-card__head p {
	margin: 0;
	color: #4e5969;
	font-size: 13px;
	line-height: 1.7;
}

.side-kicker {
	display: inline-flex;
	align-items: center;
	height: 32px;
	padding: 0 12px;
	border-radius: 999px;
	border: 1px solid rgba(229, 230, 235, 0.96);
	background: rgba(255, 255, 255, 0.9);
	color: #4e5969;
	font-size: 12px;
	font-weight: 600;
	white-space: nowrap;
}

.side-card > * + * {
	margin-top: 16px;
}

.focus-summary {
	padding: 16px 18px;
	border-radius: 20px;
	border: 1px solid rgba(229, 230, 235, 0.96);
}

.focus-summary--success {
	background: linear-gradient(180deg, rgba(240, 255, 244, 0.76), #ffffff);
}

.focus-summary--warning {
	background: linear-gradient(180deg, rgba(255, 247, 232, 0.86), #ffffff);
}

.focus-summary--danger {
	background: linear-gradient(180deg, rgba(255, 236, 232, 0.88), #ffffff);
}

.focus-summary__head {
	display: flex;
	align-items: center;
	justify-content: space-between;
	gap: 12px;
	margin-bottom: 12px;
}

.focus-summary__label,
.focus-summary__value {
	display: inline-flex;
	align-items: center;
	min-height: 28px;
	padding: 0 10px;
	border-radius: 999px;
	background: rgba(255, 255, 255, 0.9);
	border: 1px solid rgba(229, 230, 235, 0.96);
	color: #4e5969;
	font-size: 12px;
	font-weight: 700;
	white-space: nowrap;
}

.focus-summary strong {
	display: block;
	color: #1d2129;
	font-size: 16px;
	font-weight: 700;
}

.focus-summary small {
	display: block;
	margin-top: 6px;
	color: #6b7785;
	font-size: 12px;
	line-height: 1.65;
}

.focus-strip {
	display: grid;
	grid-template-columns: repeat(auto-fit, minmax(110px, 1fr));
	gap: 12px;
}

.focus-item {
	padding: 14px;
	border-radius: 18px;
	border: 1px solid rgba(229, 230, 235, 0.92);
	background: #fff;
}

.focus-item span {
	display: block;
	color: #4e5969;
	font-size: 12px;
}

.focus-item strong {
	display: block;
	margin-top: 10px;
	color: #1d2129;
	font-size: 20px;
	font-weight: 700;
	letter-spacing: -0.04em;
}

.focus-item small {
	display: block;
	margin-top: 6px;
	color: #86909c;
	font-size: 12px;
	line-height: 1.6;
}

.focus-item--success {
	background: linear-gradient(180deg, rgba(240, 255, 244, 0.52), #ffffff);
}

.focus-item--warning {
	background: linear-gradient(180deg, rgba(255, 247, 232, 0.58), #ffffff);
}

.focus-item--danger {
	background: linear-gradient(180deg, rgba(255, 236, 232, 0.58), #ffffff);
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
.breakdown-list {
	display: grid;
	grid-template-columns: repeat(2, minmax(0, 1fr));
	gap: 12px;
}

.quick-item {
	--quick-accent: #165dff;
	--quick-accent-soft: rgba(22, 93, 255, 0.08);
	--quick-accent-border: rgba(22, 93, 255, 0.22);
	flex-direction: column;
	align-items: flex-start;
	min-height: 160px;
	text-align: left;
	cursor: pointer;
	background:
		radial-gradient(circle at top right, var(--quick-accent-soft), transparent 42%),
		linear-gradient(180deg, #ffffff 0%, #fbfdff 100%);
	transition:
		transform 0.2s ease,
		border-color 0.2s ease,
		box-shadow 0.2s ease;
}

.quick-item:hover {
	transform: translateY(-2px);
	border-color: var(--quick-accent-border);
	box-shadow: 0 12px 24px rgba(15, 23, 42, 0.08);
}

.quick-item__top {
	display: flex;
	align-items: center;
	justify-content: space-between;
	width: 100%;
	gap: 12px;
}

.quick-item__icon {
	display: inline-flex;
	align-items: center;
	justify-content: center;
	width: 40px;
	height: 40px;
	border-radius: 14px;
	background: var(--quick-accent-soft);
	color: var(--quick-accent);
	font-size: 18px;
}

.quick-item__badge {
	display: inline-flex;
	align-items: center;
	height: 28px;
	padding: 0 10px;
	border-radius: 999px;
	background: rgba(255, 255, 255, 0.92);
	border: 1px solid rgba(229, 230, 235, 0.96);
	color: #4e5969;
	font-size: 12px;
	font-weight: 600;
}

.quick-item__meta {
	display: inline-flex;
	align-items: center;
	gap: 6px;
	margin-top: auto;
	color: var(--quick-accent);
	font-size: 12px;
	font-weight: 600;
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

.trend-briefs,
.health-overview {
	display: grid;
	grid-template-columns: repeat(3, minmax(0, 1fr));
	gap: 12px;
}

.trend-brief,
.health-overview__item {
	padding: 14px 16px;
	border-radius: 18px;
	border: 1px solid rgba(229, 230, 235, 0.92);
	background: linear-gradient(180deg, #fbfdff 0%, #ffffff 100%);
}

.trend-brief span,
.health-overview__item span {
	display: block;
	color: #4e5969;
	font-size: 13px;
}

.trend-brief strong,
.health-overview__item strong {
	display: block;
	margin-top: 10px;
	color: #1d2129;
	font-size: 22px;
	font-weight: 700;
	letter-spacing: -0.04em;
}

.trend-brief small,
.health-overview__item small {
	display: block;
	margin-top: 6px;
	color: #86909c;
	font-size: 12px;
	line-height: 1.6;
}

.health-overview__item--success {
	background: linear-gradient(180deg, rgba(240, 255, 244, 0.5), #ffffff);
}

.health-overview__item--warning {
	background: linear-gradient(180deg, rgba(255, 247, 232, 0.56), #ffffff);
}

.health-overview__item--danger {
	background: linear-gradient(180deg, rgba(255, 236, 232, 0.56), #ffffff);
}

.status-score {
	display: flex;
	align-items: center;
	justify-content: space-between;
	gap: 18px;
	padding: 18px;
	border-radius: 22px;
	border: 1px solid rgba(229, 230, 235, 0.96);
	background:
		radial-gradient(circle at top right, rgba(22, 93, 255, 0.08), transparent 36%),
		linear-gradient(180deg, #fbfdff 0%, #ffffff 100%);
}

.status-score__body {
	display: flex;
	flex: 1;
	flex-direction: column;
	gap: 10px;
	min-width: 0;
}

.status-score__eyebrow {
	color: #86909c;
	font-size: 12px;
	font-weight: 700;
	letter-spacing: 0.16em;
	text-transform: uppercase;
}

.status-score__body h4 {
	margin: 0;
	color: #1d2129;
	font-size: 22px;
	letter-spacing: -0.03em;
}

.status-score__body p {
	margin: 0;
	color: #4e5969;
	font-size: 13px;
	line-height: 1.7;
}

.status-score__ring {
	display: flex;
	flex-direction: column;
	align-items: center;
	justify-content: center;
	width: 108px;
	height: 108px;
	border-radius: 50%;
	border: 8px solid rgba(22, 93, 255, 0.12);
	background: #fff;
	box-shadow: inset 0 0 0 1px rgba(229, 230, 235, 0.96);
	flex-shrink: 0;
}

.status-score__ring strong {
	color: #165dff;
	font-size: 36px;
	font-weight: 700;
	line-height: 1;
	letter-spacing: -0.04em;
}

.status-score__ring small {
	margin-top: 4px;
	color: #86909c;
	font-size: 12px;
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

.resource-list,
.recommendation-list {
	display: grid;
	gap: 12px;
}

.resource-list {
	grid-template-columns: repeat(2, minmax(0, 1fr));
}

.resource-item,
.recommendation-item {
	padding: 14px 16px;
	border-radius: 18px;
	border: 1px solid rgba(229, 230, 235, 0.92);
	background: #fff;
}

.resource-item {
	display: flex;
	align-items: center;
	justify-content: space-between;
	gap: 12px;
}

.resource-item__main {
	display: flex;
	align-items: center;
	gap: 12px;
	min-width: 0;
}

.resource-item__dot {
	width: 10px;
	height: 10px;
	border-radius: 50%;
	flex-shrink: 0;
}

.resource-item strong,
.recommendation-item strong {
	display: block;
	color: #1d2129;
	font-size: 14px;
	font-weight: 600;
}

.resource-item small,
.recommendation-item p {
	display: block;
	margin-top: 4px;
	color: #86909c;
	font-size: 12px;
	line-height: 1.6;
}

.resource-item__value {
	color: #1d2129;
	font-size: 18px;
	font-weight: 700;
	letter-spacing: -0.04em;
	white-space: nowrap;
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

.recommendation-item__head {
	display: flex;
	align-items: center;
	justify-content: space-between;
	gap: 12px;
	margin-bottom: 12px;
}

.recommendation-item__label,
.recommendation-item__value {
	display: inline-flex;
	align-items: center;
	min-height: 28px;
	padding: 0 10px;
	border-radius: 999px;
	font-size: 12px;
	font-weight: 700;
	white-space: nowrap;
}

.tone-success {
	background: linear-gradient(180deg, rgba(240, 255, 244, 0.72), #fff);
}

.tone-success .recommendation-item__label,
.tone-success .recommendation-item__value {
	background: rgba(0, 180, 42, 0.12);
	color: #009a29;
}

.tone-warning {
	background: linear-gradient(180deg, rgba(255, 247, 232, 0.82), #fff);
}

.tone-warning .recommendation-item__label,
.tone-warning .recommendation-item__value {
	background: rgba(255, 125, 0, 0.12);
	color: #d25f00;
}

.tone-danger {
	background: linear-gradient(180deg, rgba(255, 236, 232, 0.84), #fff);
}

.tone-danger .recommendation-item__label,
.tone-danger .recommendation-item__value {
	background: rgba(245, 63, 63, 0.12);
	color: #cb2634;
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

	.trend-briefs,
	.health-overview {
		grid-template-columns: repeat(2, minmax(0, 1fr));
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

	.resource-list {
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
	.focus-summary__head,
	.side-card__head,
	.summary-head,
	.notice-item,
	.health-item,
	.status-score,
	.resource-item,
	.recommendation-item__head,
	.quick-item__top {
		flex-direction: column;
	}

	.hero-tags,
	.chip-row {
		justify-content: flex-start;
	}

	.hero-stats,
	.quick-grid,
	.mini-grid,
	.trend-briefs,
	.health-overview,
	.resource-list,
	.breakdown-list {
		grid-template-columns: 1fr;
	}

	.health-meta {
		align-items: flex-start;
	}

	.status-score__ring {
		width: 92px;
		height: 92px;
	}

	.chart-xl {
		min-height: 300px;
	}
}
</style>
