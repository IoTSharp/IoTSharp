<template>
	<ConsolePageShell :eyebrow="eyebrow" :title="title" :description="description" :badges="badges" :metrics="metrics">
		<template v-if="$slots.actions" #actions>
			<slot name="actions" />
		</template>

		<section class="console-crud-workspace">
			<header class="console-crud-workspace__head">
				<div class="console-crud-workspace__copy">
					<h2>{{ cardTitle }}</h2>
					<p>{{ cardDescription }}</p>
				</div>
				<div v-if="$slots.aside" class="console-crud-workspace__aside">
					<slot name="aside" />
				</div>
			</header>

			<div class="console-crud-workspace__body">
				<slot />
			</div>
		</section>
	</ConsolePageShell>
</template>

<script setup lang="ts">
import ConsolePageShell from './ConsolePageShell.vue';

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
		cardEyebrow: string;
		cardTitle: string;
		cardDescription: string;
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
.console-crud-workspace {
	border: 1px solid var(--iotsharp-border);
	border-radius: var(--iotsharp-radius-panel);
	background: var(--iotsharp-surface);
	box-shadow: var(--iotsharp-shadow-panel);
}

.console-crud-workspace__head {
	display: flex;
	align-items: center;
	justify-content: space-between;
	gap: 16px;
	min-height: 58px;
	padding: 12px 16px;
	border-bottom: 1px solid var(--iotsharp-border);
}

.console-crud-workspace__copy {
	min-width: 0;
}

.console-crud-workspace__head h2 {
	margin: 0;
	color: var(--iotsharp-ink);
	font-size: 15px;
	font-weight: 680;
	letter-spacing: 0;
}

.console-crud-workspace__head p {
	margin: 4px 0 0;
	color: var(--iotsharp-text-muted);
	font-size: 11px;
	line-height: 1.5;
}

.console-crud-workspace__aside {
	flex: 0 0 auto;
}

.console-crud-workspace__body {
	padding: 14px 16px 16px;
}

:deep(.console-crud-tags) {
	display: flex;
	flex-wrap: wrap;
	justify-content: flex-end;
	gap: 6px;
}

:deep(.console-crud-tag) {
	display: inline-flex;
	min-height: 26px;
	align-items: center;
	padding: 0 9px;
	border: 1px solid var(--iotsharp-border);
	border-radius: 999px;
	background: var(--iotsharp-surface-muted);
	color: var(--iotsharp-text-soft);
	font-size: 11px;
	font-weight: 600;
}

:deep(.console-crud-tag.is-primary) {
	border-color: rgba(var(--iotsharp-accent-rgb), 0.2);
	background: var(--iotsharp-selection);
	color: var(--iotsharp-accent);
}

@media (max-width: 680px) {
	.console-crud-workspace__head {
		align-items: flex-start;
		flex-direction: column;
	}

	.console-crud-workspace__aside {
		width: 100%;
	}

	.console-crud-workspace__body {
		padding: 10px;
	}
}
</style>
