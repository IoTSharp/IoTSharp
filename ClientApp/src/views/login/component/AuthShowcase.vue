<template>
	<section class="auth-showcase">
		<header class="auth-showcase__header">
			<RouterLink class="auth-showcase__home" to="/">
				<AppLogo />
			</RouterLink>
			<RouterLink class="auth-showcase__link" :to="linkTo">{{ linkLabel }}</RouterLink>
		</header>

		<div class="auth-showcase__body">
			<h1>{{ title }}</h1>
			<p>{{ description }}</p>
		</div>

		<div class="auth-showcase__visual" aria-hidden="true">
			<div class="auth-showcase__halo auth-showcase__halo--one"></div>
			<div class="auth-showcase__halo auth-showcase__halo--two"></div>
			<svg class="auth-showcase__network" viewBox="0 0 520 240" fill="none" xmlns="http://www.w3.org/2000/svg">
				<path d="M40 178C124 176 137 92 226 107C292 118 308 178 382 157C428 144 446 89 490 66" stroke="rgba(255,255,255,.32)" stroke-width="1.5" stroke-dasharray="6 8" />
				<path d="M84 50C145 92 183 85 226 107C273 131 304 90 354 92C397 94 414 127 462 136" stroke="var(--iotsharp-theme-glow)" stroke-opacity=".54" stroke-width="1.5" />
				<path d="M226 107V44M226 107V190M226 107H382M226 107H94" stroke="rgba(255,255,255,.18)" stroke-width="1" />
				<circle cx="226" cy="107" r="30" fill="rgba(255,255,255,.1)" stroke="rgba(255,255,255,.58)" />
				<circle cx="226" cy="107" r="8" fill="var(--iotsharp-theme-signal-soft)" />
				<circle cx="94" cy="50" r="5" fill="var(--iotsharp-theme-glow)" />
				<circle cx="382" cy="157" r="5" fill="var(--iotsharp-theme-signal)" />
				<circle cx="462" cy="136" r="5" fill="var(--iotsharp-theme-contrast)" />
			</svg>
			<div class="auth-showcase__node auth-showcase__node--top">
				<span>权限边界</span>
				<strong>RBAC</strong>
			</div>
			<div class="auth-showcase__node auth-showcase__node--bottom">
				<span>证据留存</span>
				<strong>AUDIT</strong>
			</div>
			<div class="auth-showcase__center-label">IoTSharp <span>control plane</span></div>
		</div>

		<div class="auth-showcase__focus">
			<div>
				<span>{{ primaryCard.label }}</span>
				<strong>{{ primaryCard.title }}</strong>
			</div>
			<b>{{ primaryCard.value }}</b>
			<p>{{ primaryCard.description }}</p>
		</div>

		<dl class="auth-showcase__stats">
			<div v-for="item in metrics" :key="item.label" class="auth-stat-row">
				<dt>{{ item.label }}</dt>
				<dd>{{ item.value }}</dd>
				<small>{{ item.description }}</small>
			</div>
		</dl>

		<footer class="auth-showcase__footer">
			<span v-for="tag in tags" :key="tag">{{ tag }}</span>
		</footer>
	</section>
</template>

<script setup lang="ts">
import { RouterLink } from 'vue-router';
import AppLogo from '/@/components/AppLogo.vue';

type ShowcaseTone = 'primary' | 'accent' | 'success' | 'warning';

interface ShowcaseMetric {
	label: string;
	value: string;
	description: string;
	tone?: ShowcaseTone;
}

interface ShowcasePrimaryCard {
	label: string;
	value: string;
	title: string;
	description: string;
}

defineProps<{
	eyebrow: string;
	title: string;
	description: string;
	linkTo: string;
	linkLabel: string;
	primaryCard: ShowcasePrimaryCard;
	metrics: ShowcaseMetric[];
	tags: string[];
}>();
</script>

<style scoped lang="scss">
.auth-showcase {
	display: flex;
	min-height: 100%;
	flex-direction: column;
	justify-content: space-between;
	gap: 22px;
	padding: 28px 32px;
	position: relative;
	overflow: hidden;
	background:
		radial-gradient(circle at 80% 12%, var(--iotsharp-theme-glow), transparent 30%),
		linear-gradient(160deg, var(--iotsharp-theme-deep) 0%, var(--iotsharp-theme-dark) 62%, var(--iotsharp-theme-signal) 132%);
	color: #ffffff;
}

.auth-showcase::after {
	position: absolute;
	inset: auto -22% -34% 16%;
	height: 52%;
	content: '';
	background: radial-gradient(ellipse, var(--iotsharp-theme-glow), transparent 67%);
	pointer-events: none;
}

.auth-showcase :deep(.app-logo) {
	--app-logo-text: #ffffff;
	--app-logo-subtext: rgba(255, 255, 255, 0.68);
}

.auth-showcase__header,
.auth-showcase__focus,
.auth-showcase__footer {
	display: flex;
	align-items: center;
	justify-content: space-between;
	gap: 14px;
}

.auth-showcase__home,
.auth-showcase__link {
	color: inherit;
	text-decoration: none;
}

.auth-showcase__link {
	font-size: 12px;
	font-weight: 600;
}

.auth-showcase__body {
	max-width: 60ch;
	position: relative;
	z-index: 1;
}

.auth-showcase__body h1 {
	margin: 0;
	font-size: 34px;
	font-weight: 700;
	letter-spacing: 0;
	line-height: 1.2;
}

.auth-showcase__body p {
	margin: 10px 0 0;
	color: rgba(255, 255, 255, 0.72);
	font-size: 13px;
	line-height: 1.7;
}

.auth-showcase__visual {
	position: relative;
	min-height: 190px;
	margin: 2px -8px 0;
	overflow: hidden;
	border: 1px solid rgba(255, 255, 255, 0.16);
	border-radius: 18px;
	background: linear-gradient(140deg, rgba(255, 255, 255, 0.11), var(--iotsharp-theme-signal-soft));
	box-shadow: inset 0 1px 0 rgba(255, 255, 255, 0.15), 0 20px 50px rgba(var(--iotsharp-accent-rgb), 0.14);
}

.auth-showcase__network {
	position: absolute;
	inset: 0;
	width: 100%;
	height: 100%;
}

.auth-showcase__halo {
	position: absolute;
	border: 1px solid var(--iotsharp-theme-glow);
	border-radius: 50%;
}

.auth-showcase__halo--one {
	width: 250px;
	height: 250px;
	top: -84px;
	right: -52px;
}

.auth-showcase__halo--two {
	width: 150px;
	height: 150px;
	bottom: -70px;
	left: -24px;
	border-color: var(--iotsharp-theme-signal);
}

.auth-showcase__node {
	position: absolute;
	display: grid;
	gap: 3px;
	min-width: 94px;
	padding: 8px 10px;
	border: 1px solid rgba(255, 255, 255, 0.18);
	border-radius: 10px;
	background: rgba(255, 255, 255, 0.12);
	backdrop-filter: blur(8px);
}

.auth-showcase__node span {
	color: rgba(255, 255, 255, 0.58);
	font-size: 9px;
}

.auth-showcase__node strong {
	font-size: 14px;
	font-weight: 700;
	letter-spacing: 0.01em;
}

.auth-showcase__node--top {
	top: 20px;
	right: 22px;
}

.auth-showcase__node--bottom {
	bottom: 18px;
	left: 22px;
}

.auth-showcase__center-label {
	position: absolute;
	top: 50%;
	left: 50%;
	transform: translate(-50%, -50%);
	color: #f7fcff;
	font-size: 12px;
	font-weight: 700;
	letter-spacing: 0.02em;
	white-space: nowrap;
}

.auth-showcase__center-label span {
	margin-left: 5px;
	color: var(--iotsharp-theme-light);
	font-size: 9px;
	font-weight: 500;
	opacity: 0.68;
}

.auth-showcase__focus {
	position: relative;
	z-index: 1;
	display: grid;
	grid-template-columns: minmax(0, 1fr) auto;
	padding: 16px 0;
	border-top: 1px solid rgba(255, 255, 255, 0.14);
	border-bottom: 1px solid rgba(255, 255, 255, 0.14);
}

.auth-showcase__focus div {
	display: grid;
	gap: 4px;
}

.auth-showcase__focus span {
	color: rgba(255, 255, 255, 0.58);
	font-size: 10px;
}

.auth-showcase__focus strong {
	font-size: 16px;
}

.auth-showcase__focus b {
	color: var(--iotsharp-theme-light);
	font-size: 13px;
}

.auth-showcase__focus p {
	grid-column: 1 / -1;
	margin: 6px 0 0;
	color: rgba(255, 255, 255, 0.68);
	font-size: 11px;
	line-height: 1.6;
}

.auth-showcase__stats {
	display: grid;
	margin: 0;
}

.auth-stat-row {
	display: grid;
	grid-template-columns: 88px minmax(0, 1fr);
	gap: 2px 12px;
	padding: 10px 0;
	border-bottom: 1px solid rgba(255, 255, 255, 0.1);
}

.auth-stat-row dt {
	color: rgba(255, 255, 255, 0.58);
	font-size: 11px;
}

.auth-stat-row dd {
	margin: 0;
	font-size: 12px;
	font-weight: 650;
}

.auth-stat-row small {
	grid-column: 2;
	color: rgba(255, 255, 255, 0.62);
	font-size: 10px;
	line-height: 1.5;
}

.auth-showcase__footer {
	justify-content: flex-start;
	flex-wrap: wrap;
}

.auth-showcase__footer span {
	padding: 4px 8px;
	border: 1px solid rgba(255, 255, 255, 0.14);
	border-radius: 999px;
	color: rgba(255, 255, 255, 0.72);
	font-size: 10px;
}

@media (max-width: 760px) {
	.auth-showcase {
		padding: 22px;
	}

	.auth-showcase__body h1 {
		font-size: 27px;
	}
}
</style>
