<template>
	<div class="login-page">
		<div class="login-page__mesh"></div>
		<div class="login-shell">
			<section class="login-showcase">
				<div class="login-showcase__brand">
					<AppLogo />
					<span class="login-showcase__badge">IoT Operations Platform</span>
				</div>
				<div class="login-showcase__content">
					<p class="login-showcase__eyebrow">Unified Device Control</p>
					<h1>{{ pageTitle }}</h1>
					<p class="login-showcase__desc">
						把设备接入、消息总线、时序数据和规则自动化放到同一套控制台里，让运维和值守有更清晰的全局视角。
					</p>
				</div>
				<div class="login-showcase__grid">
					<article v-for="item in featureCards" :key="item.title" class="showcase-card">
						<div class="showcase-card__label">{{ item.title }}</div>
						<div class="showcase-card__value">{{ item.value }}</div>
						<p>{{ item.description }}</p>
					</article>
				</div>
				<div class="login-showcase__footer">
					<div class="showcase-pill">
						<span class="showcase-pill__dot"></span>
						规则引擎与设备遥测统一编排
					</div>
					<div class="showcase-pill">
						<span class="showcase-pill__dot"></span>
						支持自托管与多租户部署
					</div>
				</div>
			</section>

			<section class="login-panel">
				<div class="login-panel__header">
					<div class="login-panel__eyebrow">Secure Sign In</div>
					<h2>登录到 {{ pageTitle }}</h2>
					<p>进入控制台，查看设备运行、消息吞吐与平台健康状态。</p>
				</div>
				<Account />
				<div class="login-panel__footer">
					<div>建议使用初始化后的管理员账号登录。</div>
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
import AppLogo from '/@/components/AppLogo.vue';

const storesThemeConfig = useThemeConfig();
const { themeConfig } = storeToRefs(storesThemeConfig);

const pageTitle = computed(() => themeConfig.value.globalTitle || 'IoTSharp');
const currentYear = new Date().getFullYear();

const featureCards = computed(() => [
	{
		title: '设备接入',
		value: 'MQTT / HTTP',
		description: '统一纳管终端、网关与产品模型。',
	},
	{
		title: '数据处理',
		value: 'Telemetry',
		description: '实时属性、历史时序与告警流转一体化。',
	},
	{
		title: '自动化',
		value: 'Rule Engine',
		description: '规则、脚本和任务动作在一个界面中编排。',
	},
]);

onMounted(() => {
	NextLoading.done();
});
</script>

<style scoped lang="scss">
.login-page {
	--login-panel-bg: rgba(255, 255, 255, 0.92);
	--login-border: rgba(148, 163, 184, 0.18);
	width: 100%;
	height: 100%;
	position: relative;
	display: flex;
	align-items: center;
	justify-content: center;
	padding: 24px;
	background:
		radial-gradient(circle at top left, rgba(14, 165, 233, 0.16), transparent 28%),
		radial-gradient(circle at bottom right, rgba(59, 130, 246, 0.18), transparent 28%),
		linear-gradient(135deg, #f4f8ff 0%, #eef6fb 42%, #f8fbff 100%);
	overflow: hidden;
}

.login-page__mesh {
	position: absolute;
	inset: 0;
	background-image:
		linear-gradient(rgba(148, 163, 184, 0.08) 1px, transparent 1px),
		linear-gradient(90deg, rgba(148, 163, 184, 0.08) 1px, transparent 1px);
	background-size: 48px 48px;
	mask-image: radial-gradient(circle at center, #000 35%, transparent 88%);
	pointer-events: none;
}

.login-shell {
	position: relative;
	z-index: 1;
	display: grid;
	grid-template-columns: 1.2fr minmax(380px, 460px);
	width: min(1220px, 100%);
	min-height: min(760px, calc(100vh - 48px));
	border-radius: 32px;
	border: 1px solid rgba(255, 255, 255, 0.54);
	background: rgba(255, 255, 255, 0.36);
	box-shadow: 0 30px 70px rgba(15, 23, 42, 0.14);
	backdrop-filter: blur(20px);
	overflow: hidden;
}

.login-showcase,
.login-panel {
	padding: 40px;
}

.login-showcase {
	position: relative;
	display: flex;
	flex-direction: column;
	justify-content: space-between;
	gap: 28px;
	background:
		radial-gradient(circle at top right, rgba(16, 185, 129, 0.16), transparent 24%),
		linear-gradient(155deg, #0f172a 0%, #10243a 48%, #0f3b53 100%);
	color: #f8fafc;

	&::after {
		position: absolute;
		right: -80px;
		top: -60px;
		width: 240px;
		height: 240px;
		content: '';
		border-radius: 50%;
		background: radial-gradient(circle, rgba(59, 130, 246, 0.3), transparent 68%);
	}
}

.login-showcase__brand,
.login-showcase__footer {
	position: relative;
	z-index: 1;
	display: flex;
	align-items: center;
	flex-wrap: wrap;
	gap: 14px;
}

.login-showcase__badge,
.showcase-pill {
	display: inline-flex;
	align-items: center;
	gap: 8px;
	padding: 8px 12px;
	border-radius: 999px;
	border: 1px solid rgba(255, 255, 255, 0.12);
	background: rgba(255, 255, 255, 0.08);
	color: rgba(248, 250, 252, 0.92);
	font-size: 12px;
	font-weight: 600;
}

.login-showcase__content {
	position: relative;
	z-index: 1;
	max-width: 640px;
}

.login-showcase__eyebrow,
.login-panel__eyebrow {
	margin-bottom: 12px;
	font-size: 12px;
	font-weight: 700;
	letter-spacing: 0.18em;
	text-transform: uppercase;
}

.login-showcase__eyebrow {
	color: rgba(125, 211, 252, 0.9);
}

.login-showcase h1 {
	margin: 0 0 18px;
	font-size: clamp(36px, 5vw, 54px);
	line-height: 1.02;
	letter-spacing: -0.05em;
}

.login-showcase__desc {
	max-width: 620px;
	color: rgba(226, 232, 240, 0.88);
	font-size: 16px;
	line-height: 1.9;
}

.login-showcase__grid {
	position: relative;
	z-index: 1;
	display: grid;
	grid-template-columns: repeat(3, minmax(0, 1fr));
	gap: 16px;
}

.showcase-card {
	padding: 20px;
	border-radius: 24px;
	border: 1px solid rgba(255, 255, 255, 0.08);
	background: rgba(255, 255, 255, 0.08);
	box-shadow: inset 0 1px 0 rgba(255, 255, 255, 0.08);

	p {
		margin: 10px 0 0;
		color: rgba(226, 232, 240, 0.78);
		font-size: 13px;
		line-height: 1.7;
	}
}

.showcase-card__label {
	color: rgba(191, 219, 254, 0.84);
	font-size: 13px;
}

.showcase-card__value {
	margin-top: 12px;
	font-size: 26px;
	font-weight: 700;
	letter-spacing: -0.04em;
}

.showcase-pill__dot {
	width: 8px;
	height: 8px;
	border-radius: 50%;
	background: #34d399;
}

.login-panel {
	display: flex;
	flex-direction: column;
	justify-content: center;
	gap: 24px;
	background: var(--login-panel-bg);
}

.login-panel__header {
	h2 {
		margin: 0 0 12px;
		color: #0f172a;
		font-size: 32px;
		letter-spacing: -0.04em;
	}

	p {
		color: #64748b;
		font-size: 14px;
		line-height: 1.8;
	}
}

.login-panel__eyebrow {
	color: #0f766e;
}

.login-panel__footer {
	display: flex;
	align-items: center;
	justify-content: space-between;
	gap: 16px;
	padding-top: 18px;
	border-top: 1px solid var(--login-border);
	color: #64748b;
	font-size: 12px;
}

@media (max-width: 1080px) {
	.login-shell {
		grid-template-columns: 1fr;
		max-width: 620px;
	}

	.login-showcase {
		padding-bottom: 28px;
	}
}

@media (max-width: 767px) {
	.login-page {
		padding: 0;
	}

	.login-shell {
		min-height: 100vh;
		border-radius: 0;
	}

	.login-showcase,
	.login-panel {
		padding: 24px;
	}

	.login-showcase__grid {
		grid-template-columns: repeat(1, minmax(0, 1fr));
	}

	.login-panel__footer {
		flex-direction: column;
		align-items: flex-start;
	}
}
</style>
