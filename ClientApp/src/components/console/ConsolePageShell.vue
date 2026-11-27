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
	tone?: 'primary' | 'accent' | 'success' | 'warning' | 'danger';
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
	flex: 1 1 auto;
	flex-direction: column;
	gap: 8px;
	min-width: 0;
	min-height: 0;
	color: var(--iotsharp-text);
}

.console-page-shell__header {
	display: grid;
	flex: 0 0 auto;
	gap: 8px;
	padding: 8px 12px;
	border: 1px solid var(--iotsharp-border);
	border-radius: var(--iotsharp-radius-panel);
	background: linear-gradient(115deg, var(--iotsharp-surface), var(--iotsharp-page));
	box-shadow: var(--iotsharp-shadow-panel);
}

.console-page-shell__headline,
.console-page-shell__context {
	display: flex;
	align-items: flex-start;
	justify-content: space-between;
	gap: 8px 12px;
}

.console-page-shell__copy {
	max-width: 74ch;
}

.console-page-shell__copy h1 {
	margin: 0;
	color: var(--iotsharp-ink);
	font-size: 19px;
	font-weight: 680;
	letter-spacing: 0;
	line-height: 1.25;
}

.console-page-shell__copy p {
	margin: 2px 0 0;
	color: var(--iotsharp-text-soft);
	font-size: 12px;
	line-height: 1.45;
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
	min-height: 22px;
	align-items: center;
	padding: 0 9px;
	border: 1px solid var(--iotsharp-border);
	border-radius: var(--iotsharp-radius-small);
	background: var(--iotsharp-surface);
	color: var(--workspace-accent, var(--iotsharp-data-accent, var(--iotsharp-accent)));
	font-size: 11px;
	font-weight: 650;
	white-space: nowrap;
}

.console-page-shell__context {
	align-items: end;
	padding-top: 6px;
	border-top: 1px solid var(--iotsharp-border);
}

.console-page-shell__actions :deep(.el-button) {
	height: 32px;
	padding: 0 13px;
	border-radius: var(--iotsharp-radius-control);
	font-weight: 600;
}

.console-page-shell__actions :deep(.el-button + .el-button) {
	margin-inline-start: 0;
}

.console-page-shell__metrics {
	display: flex;
	flex: 1;
	flex-wrap: wrap;
	justify-content: flex-end;
	row-gap: 4px;
	margin: 0;
}

.console-metric {
	display: grid;
	grid-template-columns: auto auto;
	gap: 1px 8px;
	min-width: 98px;
	padding: 0 10px;
	border-inline-start: 1px solid var(--iotsharp-border);
	--metric-accent: var(--workspace-accent, var(--iotsharp-data-accent, var(--iotsharp-accent)));
}

.console-metric.tone-accent {
	--metric-accent: var(--iotsharp-operation-accent, var(--iotsharp-theme-contrast));
}

.console-metric.tone-success {
	--metric-accent: var(--iotsharp-success);
}

.console-metric.tone-warning {
	--metric-accent: var(--iotsharp-warning);
}

.console-metric.tone-danger {
	--metric-accent: var(--iotsharp-danger);
}

.console-metric dt {
	align-self: center;
	color: var(--iotsharp-text-muted);
	font-size: 11px;
}

.console-metric dd {
	margin: 0;
	color: var(--metric-accent);
	font-size: 16px;
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
	flex: 1 1 auto;
	flex-direction: column;
	gap: 8px;
	min-width: 0;
	min-height: 0;
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
		border-inline-start: 0;
		padding-inline-start: 0;
	}
}

@media (max-width: 680px) {
	.console-page-shell__header {
		padding: 8px 10px;
	}

	.console-page-shell__metrics {
		display: grid;
		grid-template-columns: repeat(2, minmax(0, 1fr));
	}

	.console-metric {
		min-width: 0;
		padding: 4px 8px;
		border: 1px solid var(--iotsharp-border);
		border-radius: var(--iotsharp-radius-small);
		background: var(--iotsharp-surface);
	}

	.console-metric:first-child {
		padding-inline-start: 8px;
		border-inline-start: 1px solid var(--iotsharp-border);
	}

	.console-page-shell__actions {
		width: 100%;
		justify-content: flex-start;
	}
}
</style>
