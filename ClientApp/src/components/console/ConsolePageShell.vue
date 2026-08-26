<template>
	<div class="console-page-shell">
		<header class="console-page-shell__header">
			<div class="console-page-shell__headline">
				<div class="console-page-shell__copy">
					<h1>{{ title }}</h1>
					<p>{{ description }}</p>
				</div>
				<div v-if="badges.length" class="console-page-shell__badges" aria-label="页面状态">
					<span v-for="badge in badges" :key="badge" class="console-page-shell__badge">{{ badge }}</span>
				</div>
			</div>

			<div v-if="$slots.actions || metrics.length" class="console-page-shell__context">
				<div v-if="$slots.actions" class="console-page-shell__actions">
					<slot name="actions" />
				</div>
				<dl v-if="metrics.length" class="console-page-shell__metrics">
					<div v-for="item in metrics" :key="item.label" class="console-metric" :class="`tone-${item.tone || 'primary'}`">
						<dt>{{ item.label }}</dt>
						<dd>{{ item.value }}</dd>
						<small>{{ item.hint }}</small>
					</div>
				</dl>
			</div>
		</header>

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
	gap: 14px;
}

.console-page-shell__header {
	display: grid;
	gap: 14px;
	padding: 18px 20px;
	border: 1px solid rgba(var(--iotsharp-accent-rgb), 0.13);
	border-radius: var(--iotsharp-radius-panel);
	background: var(--iotsharp-quiet-gradient);
}

.console-page-shell__headline,
.console-page-shell__context {
	display: flex;
	align-items: flex-start;
	justify-content: space-between;
	gap: 18px;
}

.console-page-shell__copy {
	max-width: 74ch;
}

.console-page-shell__copy h1 {
	margin: 0;
	color: var(--iotsharp-ink);
	font-size: 22px;
	font-weight: 680;
	letter-spacing: 0;
	line-height: 1.25;
}

.console-page-shell__copy p {
	margin: 6px 0 0;
	color: var(--iotsharp-text-soft);
	font-size: 13px;
	line-height: 1.65;
}

.console-page-shell__badges,
.console-page-shell__actions {
	display: flex;
	flex-wrap: wrap;
	justify-content: flex-end;
	gap: 7px;
}

.console-page-shell__badge {
	display: inline-flex;
	min-height: 26px;
	align-items: center;
	padding: 0 9px;
	border: 1px solid rgba(var(--iotsharp-accent-rgb), 0.2);
	border-radius: 999px;
	background: rgba(255, 255, 255, 0.66);
	color: var(--iotsharp-accent);
	font-size: 11px;
	font-weight: 650;
	white-space: nowrap;
}

.console-page-shell__context {
	align-items: end;
	padding-top: 12px;
	border-top: 1px solid rgba(var(--iotsharp-accent-rgb), 0.12);
}

.console-page-shell__actions :deep(.el-button) {
	height: 34px;
	padding: 0 13px;
	border-radius: var(--iotsharp-radius-control);
	font-weight: 600;
}

.console-page-shell__metrics {
	display: flex;
	flex: 1;
	justify-content: flex-end;
	margin: 0;
}

.console-metric {
	display: grid;
	grid-template-columns: auto auto;
	gap: 1px 8px;
	min-width: 126px;
	padding: 0 16px;
	border-left: 1px solid rgba(var(--iotsharp-accent-rgb), 0.14);
}

.console-metric dt {
	align-self: center;
	color: var(--iotsharp-text-muted);
	font-size: 11px;
}

.console-metric dd {
	margin: 0;
	color: var(--iotsharp-ink);
	font-size: 18px;
	font-weight: 700;
	font-variant-numeric: tabular-nums;
}

.console-metric small {
	grid-column: 1 / -1;
	color: var(--iotsharp-text-muted);
	font-size: 10px;
	line-height: 1.4;
}

.console-page-shell__body {
	display: flex;
	flex-direction: column;
	gap: 14px;
}

@media (max-width: 1040px) {
	.console-page-shell__headline,
	.console-page-shell__context {
		flex-direction: column;
	}

	.console-page-shell__badges,
	.console-page-shell__metrics {
		justify-content: flex-start;
	}

	.console-page-shell__metrics {
		width: 100%;
	}

	.console-metric:first-child {
		border-left: 0;
		padding-left: 0;
	}
}

@media (max-width: 680px) {
	.console-page-shell__header {
		padding: 15px;
	}

	.console-page-shell__metrics {
		display: grid;
		grid-template-columns: repeat(2, minmax(0, 1fr));
	}

	.console-metric {
		min-width: 0;
		padding: 7px 10px;
		border-left: 0;
	}

	.console-page-shell__actions {
		width: 100%;
		justify-content: flex-start;
	}
}
</style>
