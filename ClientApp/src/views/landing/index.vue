<template>
	<div class="landing-page">
		<header class="landing-header">
			<RouterLink class="landing-header__brand" to="/">
				<AppLogo />
			</RouterLink>

			<nav class="landing-header__nav">
				<a href="#platform">平台能力</a>
				<a href="#features">核心能力</a>
				<a href="#scenarios">应用场景</a>
				<a href="#console">控制台</a>
			</nav>

			<div class="landing-header__actions">
				<RouterLink class="landing-header__login" to="/login">登录</RouterLink>
				<RouterLink class="landing-header__signup" to="/signup">注册</RouterLink>
			</div>
		</header>

		<main class="landing-main">
			<section class="landing-hero" id="platform">
				<div class="landing-hero__content">
					<div class="landing-hero__eyebrow">Open Source IoT Platform</div>
					<h1>让设备接入、规则编排和运营控制台回到一个清晰统一的入口。</h1>
					<p>
						首页参考了 .NET 官网的蓝白科技感，右上角保留清晰的登录和注册按钮。未登录时先看到产品价值，登录后再进入轻量化的控制台工作区。
					</p>

					<div class="landing-hero__actions">
						<RouterLink class="landing-hero__primary" to="/login">立即登录</RouterLink>
						<RouterLink class="landing-hero__secondary" to="/signup">创建账号</RouterLink>
					</div>

					<div class="landing-hero__metrics">
						<div class="landing-metric">
							<strong>MQTT / HTTP</strong>
							<span>统一设备接入能力</span>
						</div>
						<div class="landing-metric">
							<strong>Rule Engine</strong>
							<span>事件与自动化编排</span>
						</div>
						<div class="landing-metric">
							<strong>Dashboard</strong>
							<span>平台状态与告警总览</span>
						</div>
					</div>
				</div>

				<div class="landing-hero__visual">
					<div class="landing-floating landing-floating--primary">
						<label>平台健康</label>
						<strong>99.97%</strong>
						<p>核心服务、设备连接与规则执行一屏查看。</p>
					</div>

					<div class="landing-floating landing-floating--secondary">
						<label>实时消息</label>
						<strong>25.8k</strong>
						<p>24 小时设备遥测与事件吞吐。</p>
					</div>

					<div class="landing-console-card">
						<div class="landing-console-card__top">
							<span></span>
							<span></span>
							<span></span>
						</div>

						<div class="landing-console-card__body">
							<div class="landing-console-card__left">
								<div class="landing-console-card__title">控制台预览</div>
								<div class="landing-console-bars">
									<span style="height: 42%"></span>
									<span style="height: 78%"></span>
									<span style="height: 60%"></span>
									<span style="height: 92%"></span>
									<span style="height: 68%"></span>
								</div>
							</div>

							<div class="landing-console-card__right">
								<div class="landing-console-ring">
									<div class="landing-console-ring__inner">89%</div>
								</div>
								<div class="landing-console-card__hint">设备在线率</div>
							</div>
						</div>
					</div>
				</div>
			</section>

			<section class="landing-section" id="features">
				<div class="landing-section__header">
					<div>
						<div class="landing-section__eyebrow">Core Capabilities</div>
						<h2>把 IoTSharp 首页从“直接进后台”重构为真正的产品入口。</h2>
					</div>
					<p>这一版首页更偏产品官网，用来承接介绍、注册和登录，而不是把未登录用户直接扔进系统布局。</p>
				</div>

				<div class="landing-grid">
					<article v-for="item in featureCards" :key="item.title" class="landing-feature">
						<div class="landing-feature__icon">{{ item.icon }}</div>
						<h3>{{ item.title }}</h3>
						<p>{{ item.description }}</p>
					</article>
				</div>
			</section>

			<section class="landing-section landing-section--scenarios" id="scenarios">
				<div class="landing-section__header">
					<div>
						<div class="landing-section__eyebrow">Scenario Fit</div>
						<h2>适合设备平台、行业项目交付和多租户运维协作。</h2>
					</div>
				</div>

				<div class="landing-scenarios">
					<article v-for="item in scenarioCards" :key="item.title" class="landing-scenario">
						<h3>{{ item.title }}</h3>
						<p>{{ item.description }}</p>
					</article>
				</div>
			</section>

			<section class="landing-section landing-section--console" id="console">
				<div class="landing-section__header">
					<div>
						<div class="landing-section__eyebrow">Console Experience</div>
						<h2>登录后进入新的控制台布局，顶部品牌区从最左开始，仪表盘更贴近现代 SaaS 后台。</h2>
					</div>
					<RouterLink class="landing-section__cta" to="/login">进入控制台</RouterLink>
				</div>

				<div class="landing-console-preview">
					<aside class="landing-console-preview__sidebar">
						<div class="landing-console-preview__brand">
							<AppLogo />
						</div>
						<div class="landing-console-preview__menu">
							<span class="is-active">仪表盘</span>
							<span>设备接入</span>
							<span>规则链</span>
							<span>消息中心</span>
							<span>系统设置</span>
						</div>
					</aside>

					<div class="landing-console-preview__content">
						<div class="landing-console-preview__topbar">
							<div>
								<h3>控制台概览</h3>
								<p>白色侧栏、浅色顶部和更通透的卡片布局。</p>
							</div>
							<div class="landing-console-preview__chips">
								<span>在线设备 1,276</span>
								<span>告警 12</span>
							</div>
						</div>

						<div class="landing-console-preview__cards">
							<div v-for="item in consoleCards" :key="item.label" class="preview-kpi">
								<label>{{ item.label }}</label>
								<strong>{{ item.value }}</strong>
								<p>{{ item.caption }}</p>
							</div>
						</div>
					</div>
				</div>
			</section>
		</main>
	</div>
</template>

<script setup lang="ts">
import { RouterLink } from 'vue-router';
import AppLogo from '/@/components/AppLogo.vue';

const featureCards = [
	{ icon: '01', title: '设备接入', description: '统一管理终端、网关、产品模型和协议接入方式。' },
	{ icon: '02', title: '遥测与属性', description: '实时采集、时序数据存储和可视化状态展示集中处理。' },
	{ icon: '03', title: '规则自动化', description: '事件触发、规则链与动作执行在同一套界面中编排。' },
	{ icon: '04', title: '运维控制台', description: '重做登录后工作台，让运营和实施都能更快上手。' },
];

const scenarioCards = [
	{ title: '行业解决方案', description: '适合作为智慧园区、能源、制造或城市物联网项目的统一平台底座。' },
	{ title: '项目交付后台', description: '从首页承接介绍与注册，再进入控制台进行设备、租户和规则管理。' },
	{ title: '多租户运营', description: '租户、管理员与运营视角可以共享同一套控制台体系和视觉秩序。' },
];

const consoleCards = [
	{ label: '消息吞吐', value: '25,872', caption: '今日事件总量与实时链路状态' },
	{ label: '在线率', value: '89%', caption: '设备连接稳定性持续监控' },
	{ label: '告警数', value: '12', caption: '需要优先处理的告警设备' },
	{ label: '平均响应', value: '126ms', caption: '核心接口与服务访问延迟' },
];
</script>

<style scoped lang="scss">
.landing-page {
	min-height: 100vh;
	background:
		radial-gradient(circle at top, rgba(191, 219, 254, 0.5), transparent 26%),
		linear-gradient(180deg, #f6fbff 0%, #ffffff 38%, #f7fbff 100%);
	color: #0f172a;
}

.landing-header {
	position: sticky;
	top: 0;
	z-index: 20;
	display: flex;
	align-items: center;
	justify-content: space-between;
	gap: 20px;
	padding: 18px 32px;
	background: rgba(255, 255, 255, 0.88);
	backdrop-filter: blur(20px);
	border-bottom: 1px solid rgba(191, 219, 254, 0.42);
}

.landing-header__brand {
	display: inline-flex;
	align-items: center;
	text-decoration: none;
}

.landing-header__nav,
.landing-header__actions {
	display: flex;
	align-items: center;
	gap: 18px;
}

.landing-header__nav a,
.landing-header__login {
	color: #325377;
	text-decoration: none;
	font-size: 14px;
	font-weight: 600;
}

.landing-header__signup {
	display: inline-flex;
	align-items: center;
	justify-content: center;
	height: 42px;
	padding: 0 18px;
	border-radius: 999px;
	background: linear-gradient(135deg, #1d4ed8 0%, #2563eb 54%, #0ea5e9 100%);
	color: #fff;
	text-decoration: none;
	font-size: 14px;
	font-weight: 700;
	box-shadow: 0 14px 30px rgba(37, 99, 235, 0.22);
}

.landing-main {
	width: min(1320px, calc(100% - 48px));
	margin: 0 auto;
	padding: 36px 0 72px;
}

.landing-hero {
	display: grid;
	grid-template-columns: 1.08fr 0.92fr;
	gap: 32px;
	padding: 28px 0 52px;
}

.landing-hero__content {
	padding-top: 36px;
}

.landing-hero__eyebrow,
.landing-section__eyebrow {
	margin-bottom: 14px;
	color: #2563eb;
	font-size: 12px;
	font-weight: 700;
	letter-spacing: 0.18em;
	text-transform: uppercase;
}

.landing-hero h1 {
	margin: 0;
	max-width: 740px;
	font-size: clamp(44px, 6vw, 72px);
	line-height: 1.02;
	letter-spacing: -0.06em;
}

.landing-hero p {
	max-width: 660px;
	margin: 20px 0 0;
	color: #52667d;
	font-size: 18px;
	line-height: 1.85;
}

.landing-hero__actions {
	display: flex;
	align-items: center;
	gap: 14px;
	margin-top: 28px;
}

.landing-hero__primary,
.landing-hero__secondary,
.landing-section__cta {
	display: inline-flex;
	align-items: center;
	justify-content: center;
	height: 48px;
	padding: 0 22px;
	border-radius: 999px;
	text-decoration: none;
	font-weight: 700;
}

.landing-hero__primary,
.landing-section__cta {
	background: linear-gradient(135deg, #1d4ed8 0%, #2563eb 54%, #0ea5e9 100%);
	color: #fff;
	box-shadow: 0 16px 32px rgba(37, 99, 235, 0.2);
}

.landing-hero__secondary {
	border: 1px solid rgba(148, 163, 184, 0.28);
	background: rgba(255, 255, 255, 0.92);
	color: #123b6d;
}

.landing-hero__metrics {
	display: grid;
	grid-template-columns: repeat(3, minmax(0, 1fr));
	gap: 16px;
	margin-top: 34px;
}

.landing-metric,
.landing-feature,
.landing-floating,
.preview-kpi,
.landing-console-card,
.landing-scenario {
	border-radius: 28px;
	border: 1px solid rgba(191, 219, 254, 0.58);
	background: rgba(255, 255, 255, 0.88);
	box-shadow: 0 24px 48px rgba(148, 163, 184, 0.12);
}

.landing-metric {
	padding: 20px 22px;
}

.landing-metric strong {
	display: block;
	font-size: 18px;
}

.landing-metric span {
	display: block;
	margin-top: 8px;
	color: #64748b;
	font-size: 13px;
}

.landing-hero__visual {
	position: relative;
	min-height: 580px;
}

.landing-floating {
	position: absolute;
	padding: 22px 24px;

	label {
		color: #64748b;
		font-size: 13px;
	}

	strong {
		display: block;
		margin-top: 10px;
		font-size: 36px;
		font-weight: 800;
		letter-spacing: -0.05em;
	}

	p {
		margin-top: 8px;
		font-size: 13px;
		color: #52667d;
	}
}

.landing-floating--primary {
	top: 18px;
	right: 18px;
	width: 260px;
	background: linear-gradient(135deg, rgba(29, 78, 216, 0.08), rgba(96, 165, 250, 0.12));
}

.landing-floating--secondary {
	left: 0;
	top: 176px;
	width: 240px;
	background: linear-gradient(135deg, rgba(14, 165, 233, 0.08), rgba(59, 130, 246, 0.12));
}

.landing-console-card {
	position: absolute;
	right: 0;
	bottom: 0;
	width: min(100%, 540px);
	padding: 18px;
	background: linear-gradient(180deg, rgba(255, 255, 255, 0.96), rgba(239, 248, 255, 0.96));
}

.landing-console-card__top {
	display: flex;
	align-items: center;
	gap: 8px;
	padding-bottom: 16px;

	span {
		width: 10px;
		height: 10px;
		border-radius: 50%;
		background: #bfdbfe;
	}
}

.landing-console-card__body {
	display: grid;
	grid-template-columns: 1.1fr 0.9fr;
	gap: 16px;
}

.landing-console-card__left,
.landing-console-card__right {
	padding: 22px;
	border-radius: 24px;
	background: rgba(255, 255, 255, 0.92);
	border: 1px solid rgba(219, 234, 254, 0.9);
}

.landing-console-card__title,
.landing-console-card__hint {
	color: #334155;
	font-size: 14px;
	font-weight: 700;
}

.landing-console-bars {
	display: flex;
	align-items: flex-end;
	gap: 10px;
	height: 168px;
	margin-top: 20px;

	span {
		flex: 1;
		border-radius: 999px 999px 16px 16px;
		background: linear-gradient(180deg, #60a5fa, #2563eb);
	}
}

.landing-console-ring {
	display: grid;
	place-items: center;
	width: 172px;
	height: 172px;
	margin: 0 auto;
	border-radius: 50%;
	background: conic-gradient(#2563eb 0 89%, rgba(191, 219, 254, 0.4) 89% 100%);
}

.landing-console-ring__inner {
	display: grid;
	place-items: center;
	width: 112px;
	height: 112px;
	border-radius: 50%;
	background: #fff;
	font-size: 24px;
	font-weight: 800;
}

.landing-console-card__hint {
	margin-top: 18px;
	text-align: center;
}

.landing-section {
	padding: 56px 0 0;
}

.landing-section__header {
	display: flex;
	align-items: end;
	justify-content: space-between;
	gap: 20px;
	margin-bottom: 24px;
}

.landing-section__header h2 {
	margin: 0;
	max-width: 820px;
	font-size: clamp(30px, 4vw, 46px);
	line-height: 1.08;
	letter-spacing: -0.05em;
}

.landing-section__header p {
	max-width: 420px;
	color: #64748b;
	line-height: 1.8;
}

.landing-grid,
.landing-scenarios {
	display: grid;
	grid-template-columns: repeat(4, minmax(0, 1fr));
	gap: 18px;
}

.landing-scenarios {
	grid-template-columns: repeat(3, minmax(0, 1fr));
}

.landing-feature,
.landing-scenario {
	padding: 24px;
}

.landing-feature__icon {
	display: inline-flex;
	align-items: center;
	justify-content: center;
	width: 42px;
	height: 42px;
	border-radius: 14px;
	background: linear-gradient(135deg, rgba(37, 99, 235, 0.12), rgba(14, 165, 233, 0.14));
	color: #2563eb;
	font-size: 14px;
	font-weight: 800;
}

.landing-feature h3,
.landing-scenario h3 {
	margin: 16px 0 10px;
	font-size: 20px;
}

.landing-feature p,
.landing-scenario p {
	margin: 0;
	color: #64748b;
	line-height: 1.8;
}

.landing-console-preview {
	display: grid;
	grid-template-columns: 260px 1fr;
	border-radius: 34px;
	border: 1px solid rgba(191, 219, 254, 0.6);
	background: rgba(255, 255, 255, 0.9);
	box-shadow: 0 24px 54px rgba(15, 23, 42, 0.08);
	overflow: hidden;
}

.landing-console-preview__sidebar {
	padding: 24px 18px;
	background: linear-gradient(180deg, #ffffff, #f7fbff);
	border-right: 1px solid rgba(226, 232, 240, 0.82);
}

.landing-console-preview__brand {
	padding-bottom: 20px;
}

.landing-console-preview__menu {
	display: flex;
	flex-direction: column;
	gap: 10px;

	span {
		padding: 14px 16px;
		border-radius: 18px;
		color: #475569;
		font-weight: 600;
	}

	.is-active {
		background: linear-gradient(135deg, rgba(14, 165, 233, 0.12), rgba(37, 99, 235, 0.08));
		color: #0369a1;
	}
}

.landing-console-preview__content {
	padding: 24px;
	background: linear-gradient(180deg, #eff8ff 0%, #ffffff 44%);
}

.landing-console-preview__topbar {
	display: flex;
	align-items: center;
	justify-content: space-between;
	gap: 20px;
	padding-bottom: 22px;

	h3 {
		margin: 0;
		font-size: 24px;
	}

	p {
		margin: 6px 0 0;
		color: #64748b;
	}
}

.landing-console-preview__chips {
	display: flex;
	gap: 10px;

	span {
		padding: 10px 14px;
		border-radius: 999px;
		background: rgba(255, 255, 255, 0.96);
		border: 1px solid rgba(191, 219, 254, 0.6);
		font-size: 13px;
		font-weight: 700;
	}
}

.landing-console-preview__cards {
	display: grid;
	grid-template-columns: repeat(2, minmax(0, 1fr));
	gap: 16px;
}

.preview-kpi {
	padding: 20px 22px;
}

.preview-kpi label {
	color: #64748b;
	font-size: 13px;
}

.preview-kpi strong {
	display: block;
	margin-top: 10px;
	font-size: 32px;
	letter-spacing: -0.05em;
}

.preview-kpi p {
	margin: 8px 0 0;
	color: #64748b;
	font-size: 13px;
}

@media (max-width: 1100px) {
	.landing-header {
		flex-wrap: wrap;
	}

	.landing-hero,
	.landing-console-preview {
		grid-template-columns: 1fr;
	}

	.landing-grid {
		grid-template-columns: repeat(2, minmax(0, 1fr));
	}

	.landing-scenarios {
		grid-template-columns: 1fr;
	}

	.landing-hero__visual {
		min-height: 640px;
	}
}

@media (max-width: 767px) {
	.landing-header {
		padding: 16px 18px;
	}

	.landing-header__nav {
		display: none;
	}

	.landing-main {
		width: min(100%, calc(100% - 28px));
		padding-top: 18px;
	}

	.landing-hero__metrics,
	.landing-grid,
	.landing-console-preview__cards,
	.landing-console-card__body {
		grid-template-columns: 1fr;
	}

	.landing-section__header,
	.landing-console-preview__topbar,
	.landing-header__actions {
		flex-direction: column;
		align-items: flex-start;
	}

	.landing-hero__visual {
		min-height: 780px;
	}

	.landing-floating--primary,
	.landing-floating--secondary,
	.landing-console-card {
		position: static;
		width: 100%;
	}

	.landing-hero__actions {
		flex-direction: column;
		align-items: stretch;
	}

	.landing-hero__primary,
	.landing-hero__secondary,
	.landing-section__cta {
		width: 100%;
	}
}
</style>
