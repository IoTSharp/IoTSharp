<template>
	<div class="landing-page">
		<header class="landing-header">
			<RouterLink class="landing-header__brand" to="/">
				<AppLogo />
			</RouterLink>

			<div class="landing-header__actions">
				<RouterLink class="landing-header__link" to="/installer">初始化</RouterLink>
				<RouterLink class="landing-header__button landing-header__button--plain" to="/signup">注册</RouterLink>
				<RouterLink class="landing-header__button" to="/login">登录</RouterLink>
			</div>
		</header>

		<main class="landing-main">
			<section class="landing-entry">
				<div class="landing-entry__main">
					<div class="landing-entry__eyebrow">IoTSharp 控制台</div>
					<h1>{{ pageTitle }}</h1>
					<p>选择入口继续操作。未初始化的实例请先完成安装向导。</p>

					<div class="landing-entry__actions">
						<RouterLink class="landing-entry__primary" to="/login">登录控制台</RouterLink>
						<RouterLink class="landing-entry__secondary" to="/installer">系统初始化</RouterLink>
					</div>
				</div>

				<aside class="landing-status" aria-label="实例状态">
					<div class="landing-status__head">
						<span>实例状态</span>
						<strong :class="{ 'is-ready': isInstalled }">{{ statusText }}</strong>
					</div>

					<div class="landing-status__grid">
						<div v-for="item in statusItems" :key="item.label" class="landing-status__item">
							<span>{{ item.label }}</span>
							<strong>{{ item.value }}</strong>
						</div>
					</div>
				</aside>
			</section>

			<section class="landing-workbench">
				<div class="landing-workbench__header">
					<h2>常用入口</h2>
					<p>这里保留进入系统前最常用的操作，不展示额外宣传内容。</p>
				</div>

				<div class="landing-workbench__grid">
					<RouterLink v-for="item in actionCards" :key="item.title" class="landing-action" :to="item.to">
						<span>{{ item.label }}</span>
						<strong>{{ item.title }}</strong>
						<p>{{ item.description }}</p>
					</RouterLink>
				</div>
			</section>
		</main>
	</div>
</template>

<script setup lang="ts">
import { computed } from 'vue';
import { RouterLink } from 'vue-router';
import { storeToRefs } from 'pinia';
import AppLogo from '/@/components/AppLogo.vue';
import { useAppInfo } from '/@/stores/appInfo';
import { useThemeConfig } from '/@/stores/themeConfig';

const storesAppInfo = useAppInfo();
const storesThemeConfig = useThemeConfig();
const { themeConfig } = storeToRefs(storesThemeConfig);

const pageTitle = computed(() => themeConfig.value.globalTitle || 'IoTSharp');
const isInstalled = computed(() => Boolean(storesAppInfo.appInfo.installed));
const statusText = computed(() => (isInstalled.value ? '已初始化' : '待初始化'));
const versionText = computed(() => storesAppInfo.appInfo.version || '--');

const statusItems = computed(() => [
	{ label: '版本', value: versionText.value },
	{ label: '认证入口', value: '账号登录' },
	{ label: '初始化', value: statusText.value },
]);

const actionCards = [
	{
		label: '01',
		title: '登录',
		description: '使用已有账号进入控制台。',
		to: '/login',
	},
	{
		label: '02',
		title: '注册',
		description: '创建租户和管理员账号。',
		to: '/signup',
	},
	{
		label: '03',
		title: '初始化',
		description: '首次部署时创建系统管理员。',
		to: '/installer',
	},
];
</script>

<style scoped lang="scss">
.landing-page {
	min-height: 100vh;
	background:
		linear-gradient(rgba(148, 163, 184, 0.08) 1px, transparent 1px),
		linear-gradient(90deg, rgba(148, 163, 184, 0.08) 1px, transparent 1px),
		linear-gradient(180deg, #f6f9fc 0%, #eef4f8 100%);
	background-size: 44px 44px, 44px 44px, auto;
	color: #172033;
}

.landing-header {
	position: sticky;
	top: 0;
	z-index: 10;
	display: flex;
	align-items: center;
	justify-content: space-between;
	gap: 20px;
	padding: 18px 32px;
	border-bottom: 1px solid rgba(203, 213, 225, 0.72);
	background: rgba(255, 255, 255, 0.9);
	backdrop-filter: blur(16px);
}

.landing-header__brand,
.landing-header__link,
.landing-header__button,
.landing-entry__primary,
.landing-entry__secondary,
.landing-action {
	text-decoration: none;
}

.landing-header__actions,
.landing-entry__actions {
	display: flex;
	align-items: center;
	gap: 12px;
}

.landing-header__link {
	color: #475569;
	font-size: 14px;
	font-weight: 600;
}

.landing-header__button,
.landing-entry__primary,
.landing-entry__secondary {
	display: inline-flex;
	align-items: center;
	justify-content: center;
	height: 40px;
	padding: 0 18px;
	border-radius: 8px;
	font-size: 14px;
	font-weight: 700;
}

.landing-header__button,
.landing-entry__primary {
	background: #165dff;
	color: #fff;
	box-shadow: 0 12px 24px rgba(22, 93, 255, 0.16);
}

.landing-header__button--plain,
.landing-entry__secondary {
	border: 1px solid rgba(148, 163, 184, 0.5);
	background: #fff;
	color: #1e3a5f;
	box-shadow: none;
}

.landing-main {
	width: min(1120px, calc(100% - 48px));
	margin: 0 auto;
	padding: 64px 0;
}

.landing-entry {
	display: grid;
	grid-template-columns: minmax(0, 1.2fr) minmax(320px, 0.8fr);
	gap: 24px;
	align-items: stretch;
}

.landing-entry__main,
.landing-status,
.landing-workbench {
	border: 1px solid rgba(203, 213, 225, 0.82);
	border-radius: 12px;
	background: rgba(255, 255, 255, 0.94);
	box-shadow: 0 18px 40px rgba(15, 23, 42, 0.07);
}

.landing-entry__main {
	padding: 40px;
}

.landing-entry__eyebrow {
	margin-bottom: 14px;
	color: #165dff;
	font-size: 13px;
	font-weight: 700;
}

.landing-entry h1 {
	margin: 0;
	font-size: clamp(36px, 5vw, 56px);
	line-height: 1.08;
	letter-spacing: 0;
}

.landing-entry p {
	max-width: 560px;
	margin: 18px 0 0;
	color: #64748b;
	font-size: 16px;
	line-height: 1.8;
}

.landing-entry__actions {
	margin-top: 28px;
}

.landing-status {
	padding: 28px;
}

.landing-status__head {
	display: flex;
	align-items: center;
	justify-content: space-between;
	gap: 12px;
	padding-bottom: 18px;
	border-bottom: 1px solid rgba(226, 232, 240, 0.9);
}

.landing-status__head span,
.landing-status__item span {
	color: #64748b;
	font-size: 13px;
}

.landing-status__head strong {
	display: inline-flex;
	align-items: center;
	min-height: 30px;
	padding: 0 12px;
	border-radius: 999px;
	background: rgba(245, 158, 11, 0.12);
	color: #b45309;
	font-size: 13px;
}

.landing-status__head strong.is-ready {
	background: rgba(22, 163, 74, 0.1);
	color: #15803d;
}

.landing-status__grid {
	display: grid;
	gap: 12px;
	margin-top: 18px;
}

.landing-status__item {
	padding: 16px;
	border: 1px solid rgba(226, 232, 240, 0.9);
	border-radius: 10px;
	background: #f8fafc;
}

.landing-status__item strong {
	display: block;
	margin-top: 8px;
	color: #172033;
	font-size: 18px;
}

.landing-workbench {
	margin-top: 24px;
	padding: 28px;
}

.landing-workbench__header {
	display: flex;
	align-items: flex-end;
	justify-content: space-between;
	gap: 18px;
	margin-bottom: 18px;
}

.landing-workbench h2 {
	margin: 0;
	font-size: 24px;
	letter-spacing: 0;
}

.landing-workbench__header p {
	max-width: 420px;
	margin: 0;
	color: #64748b;
	line-height: 1.7;
}

.landing-workbench__grid {
	display: grid;
	grid-template-columns: repeat(3, minmax(0, 1fr));
	gap: 14px;
}

.landing-action {
	display: block;
	padding: 20px;
	border: 1px solid rgba(226, 232, 240, 0.94);
	border-radius: 10px;
	background: #fff;
	color: inherit;
	transition:
		border-color 0.18s ease,
		box-shadow 0.18s ease,
		transform 0.18s ease;
}

.landing-action:hover {
	border-color: rgba(22, 93, 255, 0.36);
	box-shadow: 0 14px 30px rgba(15, 23, 42, 0.08);
	transform: translateY(-2px);
}

.landing-action span {
	color: #165dff;
	font-size: 13px;
	font-weight: 800;
}

.landing-action strong {
	display: block;
	margin-top: 14px;
	font-size: 20px;
}

.landing-action p {
	margin: 8px 0 0;
	color: #64748b;
	line-height: 1.7;
}

@media (max-width: 900px) {
	.landing-entry,
	.landing-workbench__grid {
		grid-template-columns: 1fr;
	}

	.landing-workbench__header {
		display: block;
	}

	.landing-workbench__header p {
		margin-top: 10px;
	}
}

@media (max-width: 640px) {
	.landing-header {
		align-items: flex-start;
		flex-direction: column;
		padding: 16px 18px;
	}

	.landing-header__actions,
	.landing-entry__actions {
		width: 100%;
		flex-wrap: wrap;
	}

	.landing-header__button,
	.landing-entry__primary,
	.landing-entry__secondary {
		flex: 1;
	}

	.landing-main {
		width: min(100%, calc(100% - 28px));
		padding: 28px 0;
	}

	.landing-entry__main,
	.landing-status,
	.landing-workbench {
		padding: 22px;
	}
}
</style>
