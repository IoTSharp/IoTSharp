<template>
	<el-col :xs="24" :sm="12" :md="12" :lg="6" :xl="6">
		<article class="summary-card" :style="cardStyle">
			<div class="summary-card__head">
				<div class="summary-card__eyebrow">{{ item.description }}</div>
				<div class="summary-card__icon">
					<component :is="item.icon" />
				</div>
			</div>
			<div class="summary-card__body">
				<div class="summary-card__label">{{ item.label }}</div>
				<div class="summary-card__value">{{ item.value }}</div>
			</div>
			<div class="summary-card__footer">
				<div class="summary-card__hint">{{ item.hint }}</div>
				<div class="summary-card__signal">
					<span class="summary-card__signal-dot"></span>
					实时同步
				</div>
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
	index: {
		type: Number,
		default: 0,
	},
});

const cardStyle = computed(() => ({
	'--summary-accent': props.item.accentColor,
	'--summary-accent-soft': `${props.item.accentColor}14`,
	'--summary-accent-border': `${props.item.accentColor}30`,
	'--summary-icon-bg': props.item.iconBackgroundColor,
	'--summary-icon-shadow': `${props.item.accentColor}26`,
	'--summary-delay': `${props.index * 0.06}s`,
}));
</script>

<style scoped lang="scss">
.summary-card {
	position: relative;
	display: flex;
	flex-direction: column;
	gap: 16px;
	min-height: 186px;
	padding: 18px 20px;
	border-radius: 24px;
	border: 1px solid rgba(229, 230, 235, 0.96);
	background: linear-gradient(180deg, #ffffff 0%, #fbfdff 100%);
	box-shadow: 0 14px 30px rgba(15, 23, 42, 0.05);
	overflow: hidden;
	transition:
		transform 0.3s ease,
		box-shadow 0.3s ease,
		border-color 0.3s ease;
	animation: card-rise 0.45s ease both;
	animation-delay: var(--summary-delay);

	&::before {
		position: absolute;
		top: 0;
		left: 0;
		width: 100%;
		height: 1px;
		content: '';
		background: linear-gradient(90deg, var(--summary-accent), transparent 75%);
		pointer-events: none;
	}

	&::after {
		position: absolute;
		top: -44px;
		right: -28px;
		width: 180px;
		height: 180px;
		content: '';
		border-radius: 50%;
		background: radial-gradient(circle, var(--summary-accent-soft), transparent 68%);
		pointer-events: none;
	}

	&:hover {
		transform: translateY(-4px);
		border-color: var(--summary-accent-border);
		box-shadow: 0 18px 36px rgba(15, 23, 42, 0.08);
	}
}

.summary-card__head {
	display: flex;
	align-items: flex-start;
	justify-content: space-between;
	gap: 12px;
	position: relative;
	z-index: 1;
}

.summary-card__eyebrow {
	display: inline-flex;
	align-items: center;
	min-height: 30px;
	padding: 0 12px;
	border-radius: 999px;
	background: var(--summary-accent-soft);
	color: var(--summary-accent);
	font-size: 12px;
	font-weight: 700;
}

.summary-card__icon {
	display: inline-flex;
	align-items: center;
	justify-content: center;
	width: 46px;
	height: 46px;
	border-radius: 16px;
	background: var(--summary-icon-bg);
	color: #fff;
	font-size: 20px;
	box-shadow: 0 14px 28px var(--summary-icon-shadow);
}

.summary-card__signal-dot {
	width: 8px;
	height: 8px;
	border-radius: 50%;
	background: var(--summary-accent);
	box-shadow: 0 0 0 4px var(--summary-accent-soft);
}

.summary-card__body {
	display: flex;
	flex: 1;
	flex-direction: column;
	justify-content: flex-end;
	gap: 10px;
	position: relative;
	z-index: 1;
}

.summary-card__label {
	color: #4e5969;
	font-size: 16px;
	font-weight: 600;
}

.summary-card__value {
	color: #1d2129;
	font-size: clamp(30px, 3.2vw, 40px);
	font-weight: 700;
	line-height: 1;
	letter-spacing: -0.04em;
}

.summary-card__footer {
	position: relative;
	z-index: 1;
	display: flex;
	align-items: flex-end;
	justify-content: space-between;
	gap: 12px;
	padding-top: 14px;
	border-top: 1px solid rgba(229, 230, 235, 0.92);
}

.summary-card__hint {
	flex: 1;
	color: #6b7785;
	font-size: 12px;
	line-height: 1.7;
}

.summary-card__signal {
	display: inline-flex;
	align-items: center;
	gap: 8px;
	padding: 7px 10px;
	border-radius: 999px;
	border: 1px solid rgba(229, 230, 235, 0.96);
	background: rgba(255, 255, 255, 0.9);
	color: #86909c;
	font-size: 12px;
	white-space: nowrap;
}

@media (max-width: 767px) {
	.summary-card {
		min-height: 176px;
		padding: 18px;
	}

	.summary-card__footer {
		flex-direction: column;
		align-items: flex-start;
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
