<template>
	<el-col :xs="24" :sm="12" :md="12" :lg="6" :xl="6">
		<article class="summary-card" :style="cardStyle">
			<div class="summary-card__head">
				<div class="summary-card__label">{{ item.label }}</div>
				<div class="summary-card__icon">
					<component :is="{ ...item.icon }" />
				</div>
			</div>
			<div class="summary-card__body">
				<div class="summary-card__value">{{ item.value }}</div>
				<div class="summary-card__hint">{{ item.hint }}</div>
			</div>
			<div class="summary-card__meta">
				<span class="summary-card__signal-dot"></span>
				{{ item.description }}
			</div>
		</article>
	</el-col>
</template>

<script lang="ts" setup>
import { computed, PropType } from 'vue';

interface CardItem {
	label: string;
	value: string;
	description: string;
	hint: string;
	icon: any;
	accentColor: string;
	iconBackgroundColor: string;
}

const props = defineProps({
	item: {
		type: Object as PropType<CardItem>,
		required: true,
	},
});

const cardStyle = computed(() => ({
	'--summary-accent': props.item.accentColor,
	'--summary-icon-bg': props.item.iconBackgroundColor,
}));
</script>

<style scoped lang="scss">
.summary-card {
	position: relative;
	display: flex;
	flex-direction: column;
	gap: 14px;
	min-height: 150px;
	padding: 18px 20px;
	border-radius: 22px;
	border: 1px solid rgba(224, 232, 242, 0.96);
	background: linear-gradient(180deg, rgba(255, 255, 255, 0.98), rgba(249, 251, 255, 0.96));
	box-shadow: 0 12px 28px rgba(15, 23, 42, 0.05);
	overflow: hidden;
	transition:
		transform 0.3s ease,
		box-shadow 0.3s ease,
		border-color 0.3s ease;
	animation: card-rise 0.45s ease both;

	&::before {
		position: absolute;
		inset: auto 0 0;
		height: 4px;
		content: '';
		background: var(--summary-accent);
		pointer-events: none;
	}

	&:hover {
		transform: translateY(-3px);
		border-color: rgba(166, 188, 255, 0.9);
		box-shadow: 0 16px 34px rgba(15, 23, 42, 0.08);
	}
}

.summary-card__head {
	display: flex;
	align-items: center;
	justify-content: space-between;
	gap: 12px;
}

.summary-card__icon {
	display: inline-flex;
	align-items: center;
	justify-content: center;
	width: 42px;
	height: 42px;
	border-radius: 14px;
	background: var(--summary-icon-bg);
	color: #fff;
	font-size: 20px;
	box-shadow: 0 12px 24px rgba(79, 70, 229, 0.16);
}

.summary-card__signal-dot {
	width: 8px;
	height: 8px;
	border-radius: 50%;
	background: var(--summary-accent);
	box-shadow: 0 0 0 4px rgba(79, 70, 229, 0.08);
}

.summary-card__body {
	display: flex;
	flex: 1;
	flex-direction: column;
	justify-content: center;
	gap: 8px;
}

.summary-card__label {
	color: #6b7280;
	font-size: 14px;
	font-weight: 600;
}

.summary-card__value {
	color: #0f172a;
	font-size: clamp(28px, 3vw, 36px);
	font-weight: 700;
	line-height: 1;
	letter-spacing: -0.04em;
}

.summary-card__hint {
	color: #7b8794;
	font-size: 12px;
	line-height: 1.6;
}

.summary-card__meta {
	display: inline-flex;
	align-items: center;
	gap: 8px;
	color: #9aa7b6;
	font-size: 12px;
}

@media (max-width: 767px) {
	.summary-card {
		min-height: 168px;
		padding: 18px;
	}

	.summary-card__top,
	.summary-card__footer {
		flex-wrap: wrap;
	}
}

@keyframes card-rise {
	from {
		opacity: 0;
		transform: translateY(16px);
	}

	to {
		opacity: 1;
		transform: translateY(0);
	}
}
</style>
