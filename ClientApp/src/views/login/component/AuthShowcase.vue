<template>
	<section class="auth-showcase">
		<header class="auth-showcase__header">
			<RouterLink class="auth-showcase__home" to="/">
				<AppLogo />
			</RouterLink>
			<RouterLink class="auth-showcase__link" :to="linkTo">{{ linkLabel }}</RouterLink>
		</header>

		<div class="auth-showcase__body">
			<div class="auth-showcase__eyebrow">{{ eyebrow }}</div>
			<h1>{{ title }}</h1>
			<p>{{ description }}</p>
		</div>

		<div class="auth-showcase__focus">
			<div class="auth-showcase__focus-head">
				<span class="auth-showcase__focus-label">{{ primaryCard.label }}</span>
				<span class="auth-showcase__focus-value">{{ primaryCard.value }}</span>
			</div>
			<strong>{{ primaryCard.title }}</strong>
			<p>{{ primaryCard.description }}</p>
		</div>

		<div class="auth-showcase__stats">
			<article v-for="item in metrics" :key="item.label" class="auth-stat-card" :class="`tone-${item.tone || 'primary'}`">
				<div class="auth-stat-card__label">{{ item.label }}</div>
				<div class="auth-stat-card__value">{{ item.value }}</div>
				<p>{{ item.description }}</p>
			</article>
		</div>

		<footer class="auth-showcase__footer">
			<div v-for="tag in tags" :key="tag" class="auth-showcase__tag">{{ tag }}</div>
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
	flex-direction: column;
	justify-content: space-between;
	gap: 28px;
	padding: 36px 40px;
	background:
		radial-gradient(circle at top right, rgba(96, 165, 250, 0.22), transparent 26%),
		linear-gradient(160deg, #0f3463 0%, #13457a 46%, #155a93 100%);
	color: #eff6ff;

	:deep(.app-logo) {
		--app-logo-text: #ffffff;
		--app-logo-subtext: #bfdbfe;
	}
}

.auth-showcase__header,
.auth-showcase__footer,
.auth-showcase__focus-head {
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
	color: rgba(239, 246, 255, 0.92);
	font-size: 13px;
	font-weight: 600;
}

.auth-showcase__eyebrow {
	margin-bottom: 12px;
	color: #93c5fd;
	font-size: 12px;
	font-weight: 700;
	letter-spacing: 0.18em;
	text-transform: uppercase;
}

.auth-showcase__body {
	max-width: 640px;
}

.auth-showcase__body h1 {
	margin: 0 0 18px;
	font-size: clamp(38px, 5vw, 56px);
	line-height: 1;
	letter-spacing: -0.05em;
}

.auth-showcase__body p {
	margin: 0;
	color: rgba(226, 232, 240, 0.88);
	font-size: 16px;
	line-height: 1.9;
}

.auth-showcase__focus {
	padding: 20px 22px;
	border-radius: 28px;
	border: 1px solid rgba(255, 255, 255, 0.12);
	background: rgba(255, 255, 255, 0.08);
	box-shadow: inset 0 1px 0 rgba(255, 255, 255, 0.08);
}

.auth-showcase__focus-label,
.auth-showcase__focus-value {
	display: inline-flex;
	align-items: center;
	min-height: 28px;
	padding: 0 10px;
	border-radius: 999px;
	border: 1px solid rgba(255, 255, 255, 0.12);
	background: rgba(255, 255, 255, 0.08);
	font-size: 12px;
	font-weight: 700;
	white-space: nowrap;
}

.auth-showcase__focus strong {
	display: block;
	margin-top: 14px;
	font-size: 24px;
	font-weight: 800;
	letter-spacing: -0.05em;
}

.auth-showcase__focus p {
	margin: 10px 0 0;
	color: rgba(226, 232, 240, 0.8);
	font-size: 13px;
	line-height: 1.7;
}

.auth-showcase__stats {
	display: grid;
	grid-template-columns: repeat(3, minmax(0, 1fr));
	gap: 14px;
}

.auth-stat-card {
	padding: 18px;
	border-radius: 22px;
	border: 1px solid rgba(255, 255, 255, 0.08);
	background: rgba(255, 255, 255, 0.08);
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

.auth-stat-card p {
	margin: 10px 0 0;
	color: rgba(226, 232, 240, 0.78);
	font-size: 13px;
	line-height: 1.7;
}

.tone-accent {
	background: linear-gradient(180deg, rgba(14, 165, 233, 0.14), rgba(255, 255, 255, 0.08));
}

.tone-success {
	background: linear-gradient(180deg, rgba(16, 185, 129, 0.14), rgba(255, 255, 255, 0.08));
}

.tone-warning {
	background: linear-gradient(180deg, rgba(249, 115, 22, 0.16), rgba(255, 255, 255, 0.08));
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

@media (max-width: 767px) {
	.auth-showcase {
		padding: 24px;
	}

	.auth-showcase__stats {
		grid-template-columns: 1fr;
	}

	.auth-showcase__focus-head {
		flex-direction: column;
		align-items: flex-start;
	}
}
</style>
