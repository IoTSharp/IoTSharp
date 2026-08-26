<template>
	<div class="console-drawer-workspace">
		<header class="console-drawer-workspace__header">
			<div class="console-drawer-workspace__headline">
				<div class="console-drawer-workspace__copy">
					<h2>{{ title }}</h2>
					<p>{{ description }}</p>
				</div>
				<div class="console-drawer-workspace__side">
					<div v-if="badges.length" class="console-drawer-workspace__badges">
						<span v-for="badge in badges" :key="badge" class="console-drawer-workspace__badge">{{ badge }}</span>
					</div>
					<div v-if="$slots.actions" class="console-drawer-workspace__actions">
						<slot name="actions" />
					</div>
				</div>
			</div>

			<dl v-if="metrics.length" class="console-drawer-workspace__metrics">
				<div v-for="item in metrics" :key="item.label" class="console-drawer-metric" :class="`tone-${item.tone || 'primary'}`">
					<dt>{{ item.label }}</dt>
					<dd>{{ item.value }}</dd>
					<small>{{ item.hint }}</small>
				</div>
			</dl>
		</header>

		<section class="console-drawer-workspace__body">
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
.console-drawer-workspace {
	display: flex;
	min-height: 100%;
	flex-direction: column;
	gap: 14px;
}

.console-drawer-workspace__header {
	display: grid;
	gap: 14px;
	padding: 16px 18px;
	border: 1px solid rgba(var(--iotsharp-accent-rgb), 0.14);
	border-radius: var(--iotsharp-radius-panel);
	background: var(--iotsharp-quiet-gradient);
}

.console-drawer-workspace__headline {
	display: flex;
	align-items: flex-start;
	justify-content: space-between;
	gap: 16px;
}

.console-drawer-workspace__copy {
	max-width: 74ch;
}

.console-drawer-workspace__copy h2 {
	margin: 0;
	color: var(--iotsharp-ink);
	font-size: 20px;
	font-weight: 680;
	letter-spacing: 0;
}

.console-drawer-workspace__copy p {
	margin: 5px 0 0;
	color: var(--iotsharp-text-soft);
	font-size: 12px;
	line-height: 1.6;
}

.console-drawer-workspace__side,
.console-drawer-workspace__badges,
.console-drawer-workspace__actions {
	display: flex;
	flex-wrap: wrap;
	align-items: center;
	justify-content: flex-end;
	gap: 7px;
}

.console-drawer-workspace__side {
	flex-direction: column;
	align-items: flex-end;
}

.console-drawer-workspace__badge {
	display: inline-flex;
	min-height: 26px;
	align-items: center;
	padding: 0 9px;
	border: 1px solid rgba(var(--iotsharp-accent-rgb), 0.2);
	border-radius: 999px;
	background: rgba(255, 255, 255, 0.68);
	color: var(--iotsharp-accent);
	font-size: 11px;
	font-weight: 650;
}

.console-drawer-workspace__actions :deep(.el-button) {
	height: 34px;
	border-radius: var(--iotsharp-radius-control);
}

.console-drawer-workspace__metrics {
	display: flex;
	margin: 0;
	padding-top: 12px;
	border-top: 1px solid rgba(var(--iotsharp-accent-rgb), 0.12);
}

.console-drawer-metric {
	display: grid;
	grid-template-columns: auto auto;
	gap: 1px 8px;
	min-width: 132px;
	padding: 0 15px;
	border-left: 1px solid rgba(var(--iotsharp-accent-rgb), 0.14);
}

.console-drawer-metric:first-child {
	padding-left: 0;
	border-left: 0;
}

.console-drawer-metric dt {
	align-self: center;
	color: var(--iotsharp-text-muted);
	font-size: 11px;
}

.console-drawer-metric dd {
	margin: 0;
	color: var(--iotsharp-ink);
	font-size: 17px;
	font-weight: 700;
	font-variant-numeric: tabular-nums;
	word-break: break-word;
}

.console-drawer-metric small {
	grid-column: 1 / -1;
	color: var(--iotsharp-text-muted);
	font-size: 10px;
}

.console-drawer-workspace__body {
	display: flex;
	min-height: 0;
	flex: 1;
	flex-direction: column;
	gap: 14px;
}

@media (max-width: 760px) {
	.console-drawer-workspace__headline {
		flex-direction: column;
	}

	.console-drawer-workspace__side,
	.console-drawer-workspace__badges,
	.console-drawer-workspace__actions {
		align-items: flex-start;
		justify-content: flex-start;
	}

	.console-drawer-workspace__metrics {
		display: grid;
		grid-template-columns: repeat(2, minmax(0, 1fr));
	}

	.console-drawer-metric {
		min-width: 0;
		padding: 7px 8px;
		border-left: 0;
	}
}
</style>
