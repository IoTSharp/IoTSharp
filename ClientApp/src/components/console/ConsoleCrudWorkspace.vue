<template>
	<ConsolePageShell
		:eyebrow="eyebrow"
		:title="title"
		:description="description"
		:badges="badges"
		:metrics="metrics"
	>
		<template v-if="$slots.actions" #actions>
			<slot name="actions" />
		</template>

		<div class="console-crud-workspace">
			<div class="console-crud-workspace__head">
				<div>
					<div class="console-crud-workspace__eyebrow">{{ cardEyebrow }}</div>
					<h3>{{ cardTitle }}</h3>
					<p>{{ cardDescription }}</p>
				</div>
				<div v-if="$slots.aside" class="console-crud-workspace__aside">
					<slot name="aside" />
				</div>
			</div>

			<div class="console-crud-workspace__body">
				<slot />
			</div>
		</div>
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
	padding: 20px 22px;
	border-radius: 28px;
	border: 1px solid rgba(226, 232, 240, 0.92);
	background: linear-gradient(180deg, rgba(248, 251, 255, 0.96), rgba(255, 255, 255, 0.98));
	box-shadow: 0 18px 42px rgba(15, 23, 42, 0.05);
}

.console-crud-workspace__head {
	display: flex;
	align-items: flex-start;
	justify-content: space-between;
	gap: 16px;
}

.console-crud-workspace__eyebrow {
	margin-bottom: 10px;
	color: #2563eb;
	font-size: 12px;
	font-weight: 700;
	letter-spacing: 0.16em;
	text-transform: uppercase;
}

.console-crud-workspace__head h3 {
	margin: 0;
	color: #123b6d;
	font-size: 22px;
	letter-spacing: -0.04em;
}

.console-crud-workspace__head p {
	margin: 10px 0 0;
	color: #64748b;
	font-size: 13px;
	line-height: 1.75;
}

.console-crud-workspace__aside {
	flex-shrink: 0;
}

.console-crud-workspace__body {
	margin-top: 18px;
}

:deep(.console-crud-tags) {
	display: flex;
	flex-wrap: wrap;
	justify-content: flex-end;
	gap: 10px;
}

:deep(.console-crud-tag) {
	display: inline-flex;
	align-items: center;
	min-height: 32px;
	padding: 0 12px;
	border-radius: 999px;
	border: 1px solid rgba(226, 232, 240, 0.92);
	background: rgba(248, 250, 252, 0.9);
	color: #475569;
	font-size: 12px;
	font-weight: 600;
}

:deep(.console-crud-tag.is-primary) {
	border-color: rgba(191, 219, 254, 0.92);
	background: rgba(219, 234, 254, 0.8);
	color: #2563eb;
}

@media (max-width: 767px) {
	.console-crud-workspace {
		padding: 18px;
		border-radius: 22px;
	}

	.console-crud-workspace__head {
		flex-direction: column;
	}

	.console-crud-workspace__aside {
		width: 100%;
	}
}
</style>
