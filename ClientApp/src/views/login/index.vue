<template>
	<div class="auth-page">
		<div class="auth-page__aurora"></div>
		<div class="auth-shell">
			<section class="auth-showcase">
				<header class="auth-showcase__header">
					<RouterLink class="auth-showcase__home" to="/">
						<AppLogo />
					</RouterLink>
					<RouterLink class="auth-showcase__link" to="/">返回首页</RouterLink>
				</header>

				<div class="auth-showcase__body">
					<div class="auth-showcase__eyebrow">IoTSharp Console</div>
					<h1>{{ pageTitle }}</h1>
					<p>
						统一管理设备接入、消息总线、规则引擎和平台健康。登录后直接进入新的蓝白色控制台，查看设备连接率、告警压力和系统状态。
					</p>
				</div>

				<div class="auth-showcase__panel">
					<div class="auth-showcase__panel-head">
						<span class="auth-showcase__signal"></span>
						<span class="auth-showcase__signal"></span>
						<span class="auth-showcase__signal"></span>
					</div>
					<div class="auth-showcase__stats">
						<article v-for="item in featureCards" :key="item.title" class="auth-stat-card">
							<div class="auth-stat-card__label">{{ item.title }}</div>
							<div class="auth-stat-card__value">{{ item.value }}</div>
							<p>{{ item.description }}</p>
						</article>
					</div>
				</div>

				<footer class="auth-showcase__footer">
					<div class="auth-showcase__tag">设备接入</div>
					<div class="auth-showcase__tag">规则编排</div>
					<div class="auth-showcase__tag">可观测运维</div>
				</footer>
			</section>

			<section class="auth-panel">
				<div class="auth-panel__header">
					<div class="auth-panel__eyebrow">Secure Sign In</div>
					<h2>登录到 {{ pageTitle }}</h2>
					<p>使用管理员账号进入控制台。右上角导航和整体留白参考了你给出的登录页方向，但配色统一收回到更清爽的蓝白体系。</p>
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
import { RouterLink } from 'vue-router';
import { storeToRefs } from 'pinia';
import { useThemeConfig } from '/@/stores/themeConfig';
import { NextLoading } from '/@/utils/loading';
import Account from '/@/views/login/component/account.vue';
import AppLogo from '/@/components/AppLogo.vue';

const storesThemeConfig = useThemeConfig();
const { themeConfig } = storeToRefs(storesThemeConfig);

const pageTitle = computed(() => themeConfig.value.globalTitle || 'IoTSharp');
const currentYear = new Date().getFullYear();

const featureCards = [
	{
		title: '消息总线',
		value: 'MQTT / HTTP',
		description: '统一接入网关、设备与应用系统。',
	},
	{
		title: '规则引擎',
		value: 'Flow + Rule',
		description: '事件触发、脚本动作与自动化编排集中管理。',
	},
	{
		title: '控制台',
		value: 'Live Overview',
		description: '用更轻的布局查看平台健康、告警与资源趋势。',
	},
];

onMounted(() => {
	NextLoading.done();
});
</script>

<style scoped lang="scss">
.auth-page {
	position: relative;
	display: flex;
	align-items: center;
	justify-content: center;
	min-height: 100vh;
	padding: 24px;
	background:
		radial-gradient(circle at top left, rgba(14, 165, 233, 0.18), transparent 26%),
		radial-gradient(circle at bottom right, rgba(59, 130, 246, 0.18), transparent 28%),
		linear-gradient(180deg, #f3f8ff 0%, #edf6ff 44%, #f9fcff 100%);
	overflow: hidden;
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
	grid-template-columns: 1.15fr minmax(400px, 468px);
	width: min(1240px, 100%);
	min-height: min(780px, calc(100vh - 48px));
	border-radius: 34px;
	border: 1px solid rgba(255, 255, 255, 0.72);
	background: rgba(255, 255, 255, 0.38);
	box-shadow: 0 30px 80px rgba(15, 23, 42, 0.12);
	backdrop-filter: blur(18px);
	overflow: hidden;
}

.auth-showcase,
.auth-panel {
	padding: 36px 40px;
}

.auth-showcase {
	display: flex;
	flex-direction: column;
	gap: 28px;
	justify-content: space-between;
	background:
		radial-gradient(circle at top right, rgba(96, 165, 250, 0.22), transparent 25%),
		linear-gradient(160deg, #0f3463 0%, #124276 42%, #13538e 100%);
	color: #eff6ff;

	:deep(.app-logo) {
		--app-logo-text: #ffffff;
		--app-logo-subtext: #bfdbfe;
	}
}

.auth-showcase__header,
.auth-showcase__footer {
	display: flex;
	align-items: center;
	justify-content: space-between;
	gap: 14px;
	flex-wrap: wrap;
}

.auth-showcase__home,
.auth-showcase__link {
	text-decoration: none;
}

.auth-showcase__link {
	color: rgba(239, 246, 255, 0.9);
	font-size: 13px;
	font-weight: 600;
}

.auth-showcase__eyebrow,
.auth-panel__eyebrow {
	margin-bottom: 12px;
	font-size: 12px;
	font-weight: 700;
	letter-spacing: 0.18em;
	text-transform: uppercase;
}

.auth-showcase__eyebrow {
	color: #93c5fd;
}

.auth-showcase__body {
	max-width: 640px;

	h1 {
		margin: 0 0 18px;
		font-size: clamp(38px, 5vw, 56px);
		line-height: 1;
		letter-spacing: -0.05em;
	}

	p {
		margin: 0;
		color: rgba(226, 232, 240, 0.88);
		font-size: 16px;
		line-height: 1.9;
	}
}

.auth-showcase__panel {
	padding: 18px;
	border-radius: 28px;
	border: 1px solid rgba(255, 255, 255, 0.12);
	background: rgba(255, 255, 255, 0.08);
	box-shadow: inset 0 1px 0 rgba(255, 255, 255, 0.08);
}

.auth-showcase__panel-head {
	display: flex;
	align-items: center;
	gap: 8px;
	margin-bottom: 18px;
}

.auth-showcase__signal {
	width: 10px;
	height: 10px;
	border-radius: 50%;
	background: rgba(191, 219, 254, 0.85);
}

.auth-showcase__stats {
	display: grid;
	grid-template-columns: repeat(3, minmax(0, 1fr));
	gap: 14px;
}

.auth-stat-card {
	padding: 18px;
	border-radius: 22px;
	background: rgba(255, 255, 255, 0.08);
	border: 1px solid rgba(255, 255, 255, 0.08);

	p {
		margin: 10px 0 0;
		color: rgba(226, 232, 240, 0.78);
		font-size: 13px;
		line-height: 1.7;
	}
}

.auth-stat-card__label {
	color: rgba(191, 219, 254, 0.88);
	font-size: 13px;
}

.auth-stat-card__value {
	margin-top: 12px;
	font-size: 26px;
	font-weight: 700;
	letter-spacing: -0.04em;
}

.auth-showcase__footer {
	justify-content: flex-start;
}

.auth-showcase__tag {
	padding: 8px 12px;
	border-radius: 999px;
	border: 1px solid rgba(255, 255, 255, 0.12);
	background: rgba(255, 255, 255, 0.08);
	color: rgba(239, 246, 255, 0.92);
	font-size: 12px;
	font-weight: 600;
}

.auth-panel {
	display: flex;
	flex-direction: column;
	justify-content: center;
	gap: 24px;
	background: rgba(255, 255, 255, 0.94);
}

.auth-panel__header {
	h2 {
		margin: 0 0 12px;
		color: #123b6d;
		font-size: 32px;
		letter-spacing: -0.04em;
	}

	p {
		margin: 0;
		color: #64748b;
		font-size: 14px;
		line-height: 1.85;
	}
}

.auth-panel__eyebrow {
	color: #2563eb;
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
		max-width: 640px;
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

	.auth-showcase,
	.auth-panel {
		padding: 24px;
	}

	.auth-showcase__stats {
		grid-template-columns: 1fr;
	}

	.auth-panel__footer {
		flex-direction: column;
		align-items: flex-start;
	}
}
</style>
