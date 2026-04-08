<template>
	<div class="auth-page">
		<div class="auth-page__aurora"></div>
		<div class="auth-shell">
			<AuthShowcase
				eyebrow="IoTSharp Console"
				:title="pageTitle"
				:description="showcaseDescription"
				link-to="/"
				link-label="返回首页"
				:primary-card="showcasePrimaryCard"
				:metrics="showcaseMetrics"
				:tags="showcaseTags"
			/>

			<section class="auth-panel">
				<div class="auth-panel__header">
					<div class="auth-panel__eyebrow">Secure Sign In</div>
					<h2>登录到 {{ pageTitle }}</h2>
					<p>使用管理员账号进入控制台。登录后会直接进入新的工作台布局，先看结论，再看指标和建议。</p>
				</div>

				<div class="auth-panel__highlights">
					<div v-for="item in panelHighlights" :key="item.label" class="auth-panel__highlight">
						<span>{{ item.label }}</span>
						<strong>{{ item.value }}</strong>
						<small>{{ item.hint }}</small>
					</div>
				</div>

				<Account />

				<div class="auth-panel__footer">
					<div>建议首次登录后立即修改默认密码。</div>
					<div>{{ currentYear }} {{ pageTitle }}</div>
				</div>
			</section>
		</div>
	</div>
</template>

<script setup lang="ts">
import { computed, onMounted } from 'vue';
import { storeToRefs } from 'pinia';
import { useThemeConfig } from '/@/stores/themeConfig';
import { NextLoading } from '/@/utils/loading';
import Account from '/@/views/login/component/account.vue';
import AuthShowcase from '/@/views/login/component/AuthShowcase.vue';

const storesThemeConfig = useThemeConfig();
const { themeConfig } = storeToRefs(storesThemeConfig);

const pageTitle = computed(() => themeConfig.value.globalTitle || 'IoTSharp');
const currentYear = new Date().getFullYear();
const showcaseDescription = '统一管理设备接入、消息总线、规则引擎和平台健康。未登录时先看清入口和能力，登录后再进入新的控制台工作区。';

const showcasePrimaryCard = {
	label: 'Console Access',
	value: 'Admin',
	title: '用管理员入口进入工作台',
	description: '完成密码输入和滑块拼图校验后，系统会自动跳转到新的控制台首页。',
};

const showcaseMetrics = [
	{
		label: '平台入口',
		value: 'Dashboard',
		description: '登录后先查看平台评分、重点事项和实时状态。',
		tone: 'primary' as const,
	},
	{
		label: '接入能力',
		value: 'MQTT / HTTP',
		description: '设备、网关与第三方系统接入统一管理。',
		tone: 'accent' as const,
	},
	{
		label: '运维方式',
		value: 'Rules + Health',
		description: '规则编排、消息链路和平台健康一起监控。',
		tone: 'success' as const,
	},
];

const showcaseTags = ['设备接入', '规则编排', '消息链路', '可观测运维'];

const panelHighlights = [
	{ label: '登录方式', value: '管理员账号', hint: '默认用户名已预填，方便初始化后直接进入。' },
	{ label: '安全校验', value: '滑块拼图', hint: '登录前完成一次拼图验证，防止批量撞库。' },
	{ label: '进入后', value: '工作台首页', hint: '先看重点事项，再看平台评分与趋势面板。' },
];

onMounted(() => {
	NextLoading.done();
});
</script>

<style scoped lang="scss">
.auth-page {
	position: relative;
	height: 100vh;
	min-height: 100vh;
	padding: 24px;
	background:
		radial-gradient(circle at top left, rgba(14, 165, 233, 0.18), transparent 26%),
		radial-gradient(circle at bottom right, rgba(59, 130, 246, 0.18), transparent 28%),
		linear-gradient(180deg, #f3f8ff 0%, #edf6ff 44%, #f9fcff 100%);
	overflow-x: hidden;
	overflow-y: auto;
	overscroll-behavior-y: contain;
	scrollbar-gutter: stable;
	-webkit-overflow-scrolling: touch;
}

.auth-page__aurora {
	position: absolute;
	inset: 0;
	background:
		linear-gradient(rgba(148, 163, 184, 0.08) 1px, transparent 1px),
		linear-gradient(90deg, rgba(148, 163, 184, 0.08) 1px, transparent 1px);
	background-size: 48px 48px;
	mask-image: radial-gradient(circle at center, #000 42%, transparent 88%);
	pointer-events: none;
}

.auth-shell {
	position: relative;
	z-index: 1;
	display: grid;
	grid-template-columns: 1.15fr minmax(400px, 480px);
	width: min(1240px, 100%);
	margin: 0 auto;
	min-height: min(780px, calc(100vh - 48px));
	border-radius: 34px;
	border: 1px solid rgba(255, 255, 255, 0.72);
	background: rgba(255, 255, 255, 0.38);
	box-shadow: 0 30px 80px rgba(15, 23, 42, 0.12);
	backdrop-filter: blur(18px);
	overflow: hidden;
}

.auth-panel {
	display: flex;
	flex-direction: column;
	justify-content: center;
	gap: 24px;
	padding: 36px 40px;
	background: rgba(255, 255, 255, 0.95);
}

.auth-panel__eyebrow {
	margin-bottom: 12px;
	color: #2563eb;
	font-size: 12px;
	font-weight: 700;
	letter-spacing: 0.18em;
	text-transform: uppercase;
}

.auth-panel__header h2 {
	margin: 0 0 12px;
	color: #123b6d;
	font-size: 32px;
	letter-spacing: -0.04em;
}

.auth-panel__header p {
	margin: 0;
	color: #64748b;
	font-size: 14px;
	line-height: 1.85;
}

.auth-panel__highlights {
	display: grid;
	grid-template-columns: repeat(3, minmax(0, 1fr));
	gap: 12px;
}

.auth-panel__highlight {
	padding: 14px 16px;
	border-radius: 18px;
	border: 1px solid rgba(226, 232, 240, 0.92);
	background: linear-gradient(180deg, rgba(248, 251, 255, 0.96), rgba(255, 255, 255, 0.98));
}

.auth-panel__highlight span {
	display: block;
	color: #64748b;
	font-size: 12px;
}

.auth-panel__highlight strong {
	display: block;
	margin-top: 10px;
	color: #123b6d;
	font-size: 18px;
	font-weight: 700;
	letter-spacing: -0.03em;
}

.auth-panel__highlight small {
	display: block;
	margin-top: 6px;
	color: #7c8da1;
	font-size: 12px;
	line-height: 1.6;
}

.auth-panel__footer {
	display: flex;
	align-items: center;
	justify-content: space-between;
	gap: 16px;
	padding-top: 18px;
	border-top: 1px solid rgba(226, 232, 240, 0.9);
	color: #64748b;
	font-size: 12px;
}

@media (max-width: 1080px) {
	.auth-shell {
		grid-template-columns: 1fr;
		max-width: 720px;
	}
}

@media (max-width: 767px) {
	.auth-page {
		padding: 0;
	}

	.auth-shell {
		min-height: 100vh;
		border-radius: 0;
	}

	.auth-panel {
		padding: 24px;
	}

	.auth-panel__highlights {
		grid-template-columns: 1fr;
	}

	.auth-panel__footer {
		flex-direction: column;
		align-items: flex-start;
	}
}
</style>
