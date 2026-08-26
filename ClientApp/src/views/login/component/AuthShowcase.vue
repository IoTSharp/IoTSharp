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
	background: var(--iotsharp-nav-gradient);
	color: #ffffff;
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

.auth-showcase__focus {
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
