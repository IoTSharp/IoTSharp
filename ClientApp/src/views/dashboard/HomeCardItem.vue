<template>
	<el-col :xs="24" :sm="12" :md="12" :lg="6" :xl="6">
		<article class="summary-card" :style="cardStyle">
			<div class="summary-card__top">
				<div class="summary-card__icon">
					<component :is="{ ...item.icon }" />
				</div>
				<div class="summary-card__signal">
					<span class="summary-card__signal-dot"></span>
					{{ item.description }}
				</div>
			</div>
			<div class="summary-card__body">
				<div class="summary-card__label">{{ item.label }}</div>
				<div class="summary-card__value">{{ item.value }}</div>
			</div>
			<div class="summary-card__footer">
				<span class="summary-card__hint">{{ item.hint }}</span>
				<span class="summary-card__badge">Overview</span>
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
	gap: 18px;
	min-height: 188px;
	padding: 20px;
	border-radius: 22px;
	border: 1px solid rgba(148, 163, 184, 0.16);
	background:
		radial-gradient(circle at top right, rgba(255, 255, 255, 0.92), rgba(255, 255, 255, 0.74) 55%, rgba(255, 255, 255, 0.68) 100%),
		linear-gradient(160deg, rgba(255, 255, 255, 0.92), rgba(248, 250, 252, 0.9));
	box-shadow: 0 20px 45px rgba(15, 23, 42, 0.08);
	overflow: hidden;
	transition:
		transform 0.3s ease,
		box-shadow 0.3s ease,
		border-color 0.3s ease;
	animation: card-rise 0.45s ease both;

	&::before {
		position: absolute;
		inset: 0;
		content: '';
		background: linear-gradient(135deg, rgba(255, 255, 255, 0.26), transparent 45%);
		pointer-events: none;
	}

	&::after {
		position: absolute;
		top: 0;
		left: 0;
		width: 100%;
		height: 4px;
		content: '';
		background: var(--summary-accent);
	}

	&:hover {
		transform: translateY(-4px);
		border-color: rgba(79, 70, 229, 0.16);
		box-shadow: 0 24px 50px rgba(15, 23, 42, 0.12);
	}
}

.summary-card__top,
.summary-card__footer {
	display: flex;
	align-items: center;
	justify-content: space-between;
	gap: 12px;
}

.summary-card__icon {
	display: inline-flex;
	align-items: center;
	justify-content: center;
	width: 52px;
	height: 52px;
	border-radius: 16px;
	background: var(--summary-icon-bg);
	color: #fff;
	font-size: 24px;
	box-shadow: 0 12px 24px rgba(79, 70, 229, 0.22);
}

.summary-card__signal {
	display: inline-flex;
	align-items: center;
	gap: 8px;
	padding: 8px 12px;
	border-radius: 999px;
	background: rgba(255, 255, 255, 0.72);
	color: #475569;
	font-size: 12px;
	white-space: nowrap;
}

.summary-card__signal-dot {
	width: 8px;
	height: 8px;
	border-radius: 50%;
	background: var(--summary-accent);
	box-shadow: 0 0 0 5px rgba(79, 70, 229, 0.12);
}

.summary-card__body {
	display: flex;
	flex: 1;
	flex-direction: column;
	justify-content: flex-end;
	gap: 10px;
}

.summary-card__label {
	color: #64748b;
	font-size: 14px;
	letter-spacing: 0.02em;
}

.summary-card__value {
	color: #0f172a;
	font-size: clamp(28px, 3vw, 34px);
	font-weight: 700;
	line-height: 1;
	letter-spacing: -0.04em;
}

.summary-card__hint {
	color: #334155;
	font-size: 13px;
	font-weight: 500;
}

.summary-card__badge {
	padding: 6px 10px;
	border-radius: 999px;
	background: rgba(15, 23, 42, 0.06);
	color: #475569;
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
