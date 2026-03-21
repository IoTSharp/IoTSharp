<template>
	<div class="console-page-shell">
		<section class="console-page-shell__hero">
			<div class="console-page-shell__head">
				<div class="console-page-shell__copy">
					<div class="console-page-shell__eyebrow">{{ eyebrow }}</div>
					<h1>{{ title }}</h1>
					<p>{{ description }}</p>
				</div>
				<div v-if="badges.length" class="console-page-shell__badges">
					<span v-for="badge in badges" :key="badge" class="console-page-shell__badge">{{ badge }}</span>
				</div>
			</div>

			<div class="console-page-shell__footer">
				<div v-if="$slots.actions" class="console-page-shell__actions">
					<slot name="actions" />
				</div>
				<div v-if="metrics.length" class="console-page-shell__metrics">
					<article
						v-for="item in metrics"
						:key="item.label"
						class="console-metric"
						:class="`tone-${item.tone || 'primary'}`"
					>
						<span>{{ item.label }}</span>
						<strong>{{ item.value }}</strong>
						<small>{{ item.hint }}</small>
					</article>
				</div>
			</div>
		</section>

		<section class="console-page-shell__body">
			<slot />
		</section>
	</div>
</template>

<script setup lang="ts">
interface ConsoleMetric {
	label: string;
	value: string | number;
	hint: string;
	tone?: 'primary' | 'accent' | 'success' | 'warning';
}

withDefaults(
	defineProps<{
		eyebrow: string;
		title: string;
		description: string;
		badges?: string[];
		metrics?: ConsoleMetric[];
	}>(),
	{
		badges: () => [],
		metrics: () => [],
	}
);
</script>

<style scoped lang="scss">
.console-page-shell {
	display: flex;
	flex-direction: column;
	gap: 18px;
}

.console-page-shell__hero {
	padding: 26px 28px;
	border-radius: 30px;
	border: 1px solid rgba(191, 219, 254, 0.78);
	background:
		radial-gradient(circle at top right, rgba(96, 165, 250, 0.16), transparent 28%),
		linear-gradient(180deg, rgba(248, 251, 255, 0.98), rgba(240, 247, 255, 0.96));
	box-shadow: 0 18px 42px rgba(15, 23, 42, 0.06);
}

.console-page-shell__head,
.console-page-shell__footer {
	display: flex;
	align-items: flex-start;
	justify-content: space-between;
	gap: 18px;
}

.console-page-shell__copy {
	max-width: 860px;
}

.console-page-shell__eyebrow {
	margin-bottom: 12px;
	color: #2563eb;
	font-size: 12px;
	font-weight: 700;
	letter-spacing: 0.18em;
	text-transform: uppercase;
}

.console-page-shell__copy h1 {
	margin: 0;
	color: #123b6d;
	font-size: clamp(28px, 4vw, 36px);
	letter-spacing: -0.05em;
}

.console-page-shell__copy p {
	margin: 12px 0 0;
	color: #5f7289;
	font-size: 14px;
	line-height: 1.85;
}

.console-page-shell__badges {
	display: flex;
	flex-wrap: wrap;
	justify-content: flex-end;
	gap: 10px;
}

.console-page-shell__badge {
	display: inline-flex;
	align-items: center;
	min-height: 34px;
	padding: 0 12px;
	border-radius: 999px;
	border: 1px solid rgba(191, 219, 254, 0.9);
	background: rgba(255, 255, 255, 0.72);
	color: #2563eb;
	font-size: 12px;
	font-weight: 700;
	white-space: nowrap;
}

.console-page-shell__footer {
	margin-top: 20px;
	align-items: stretch;
}

.console-page-shell__actions {
	display: flex;
	flex-wrap: wrap;
	gap: 10px;
}

.console-page-shell__actions :deep(.el-button) {
	height: 44px;
	padding: 0 18px;
	border-radius: 14px;
	font-weight: 600;
}

.console-page-shell__metrics {
	display: grid;
	grid-template-columns: repeat(4, minmax(0, 1fr));
	gap: 12px;
	flex: 1;
}

.console-metric {
	padding: 16px 18px;
	border-radius: 22px;
	border: 1px solid rgba(226, 232, 240, 0.92);
	background: rgba(255, 255, 255, 0.88);
}

.console-metric span {
	display: block;
	color: #64748b;
	font-size: 12px;
}

.console-metric strong {
	display: block;
	margin-top: 10px;
	color: #123b6d;
	font-size: 22px;
	font-weight: 700;
	letter-spacing: -0.04em;
}

.console-metric small {
	display: block;
	margin-top: 8px;
	color: #7c8da1;
	font-size: 12px;
	line-height: 1.6;
}

.tone-accent {
	background: linear-gradient(180deg, rgba(14, 165, 233, 0.08), rgba(255, 255, 255, 0.9));
}

.tone-success {
	background: linear-gradient(180deg, rgba(22, 163, 74, 0.08), rgba(255, 255, 255, 0.9));
}

.tone-warning {
	background: linear-gradient(180deg, rgba(249, 115, 22, 0.08), rgba(255, 255, 255, 0.9));
}

.console-page-shell__body {
	display: flex;
	flex-direction: column;
	gap: 16px;
}

@media (max-width: 1080px) {
	.console-page-shell__head,
	.console-page-shell__footer {
		flex-direction: column;
	}

	.console-page-shell__badges {
		justify-content: flex-start;
	}

	.console-page-shell__metrics {
		grid-template-columns: repeat(2, minmax(0, 1fr));
		width: 100%;
	}
}

@media (max-width: 767px) {
	.console-page-shell__hero {
		padding: 22px 20px;
		border-radius: 24px;
	}

	.console-page-shell__actions {
		width: 100%;
	}

	.console-page-shell__actions :deep(.el-button) {
		flex: 1;
		min-width: 0;
	}

	.console-page-shell__metrics {
		grid-template-columns: 1fr;
	}
}
</style>
