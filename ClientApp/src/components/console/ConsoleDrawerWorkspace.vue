<template>
	<div class="console-drawer-workspace">
		<section class="console-drawer-workspace__hero">
			<div class="console-drawer-workspace__head">
				<div class="console-drawer-workspace__copy">
					<div class="console-drawer-workspace__eyebrow">{{ eyebrow }}</div>
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

			<div v-if="metrics.length" class="console-drawer-workspace__metrics">
				<article
					v-for="item in metrics"
					:key="item.label"
					class="console-drawer-metric"
					:class="`tone-${item.tone || 'primary'}`"
				>
					<span>{{ item.label }}</span>
					<strong>{{ item.value }}</strong>
					<small>{{ item.hint }}</small>
				</article>
			</div>
		</section>

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
.console-drawer-workspace {
	display: flex;
	flex-direction: column;
	gap: 18px;
	min-height: 100%;
}

.console-drawer-workspace__hero {
	padding: 24px 26px;
	border-radius: 28px;
	border: 1px solid rgba(191, 219, 254, 0.78);
	background:
		radial-gradient(circle at top right, rgba(96, 165, 250, 0.18), transparent 30%),
		linear-gradient(180deg, rgba(248, 251, 255, 0.98), rgba(240, 247, 255, 0.96));
	box-shadow: 0 18px 42px rgba(15, 23, 42, 0.06);
}

.console-drawer-workspace__head {
	display: flex;
	align-items: flex-start;
	justify-content: space-between;
	gap: 18px;
}

.console-drawer-workspace__copy {
	max-width: 860px;
}

.console-drawer-workspace__eyebrow {
	margin-bottom: 12px;
	color: #2563eb;
	font-size: 12px;
	font-weight: 700;
	letter-spacing: 0.18em;
	text-transform: uppercase;
}

.console-drawer-workspace__copy h2 {
	margin: 0;
	color: #123b6d;
	font-size: clamp(26px, 3vw, 34px);
	letter-spacing: -0.05em;
}

.console-drawer-workspace__copy p {
	margin: 12px 0 0;
	color: #5f7289;
	font-size: 14px;
	line-height: 1.8;
}

.console-drawer-workspace__side {
	display: flex;
	flex-direction: column;
	align-items: flex-end;
	gap: 12px;
	flex-shrink: 0;
}

.console-drawer-workspace__badges {
	display: flex;
	flex-wrap: wrap;
	justify-content: flex-end;
	gap: 10px;
}

.console-drawer-workspace__badge {
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

.console-drawer-workspace__actions {
	display: flex;
	flex-wrap: wrap;
	justify-content: flex-end;
	gap: 10px;
}

.console-drawer-workspace__actions :deep(.el-button) {
	height: 44px;
	padding: 0 18px;
	border-radius: 14px;
	font-weight: 600;
}

.console-drawer-workspace__metrics {
	display: grid;
	grid-template-columns: repeat(4, minmax(0, 1fr));
	gap: 12px;
	margin-top: 20px;
}

.console-drawer-metric {
	padding: 16px 18px;
	border-radius: 22px;
	border: 1px solid rgba(226, 232, 240, 0.92);
	background: rgba(255, 255, 255, 0.88);
	min-width: 0;
}

.console-drawer-metric span {
	display: block;
	color: #64748b;
	font-size: 12px;
}

.console-drawer-metric strong {
	display: block;
	margin-top: 10px;
	color: #123b6d;
	font-size: 22px;
	font-weight: 700;
	letter-spacing: -0.04em;
	word-break: break-word;
}

.console-drawer-metric small {
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

.console-drawer-workspace__body {
	display: flex;
	flex-direction: column;
	gap: 18px;
	flex: 1;
	min-height: 0;
}

@media (max-width: 1080px) {
	.console-drawer-workspace__head {
		flex-direction: column;
	}

	.console-drawer-workspace__side,
	.console-drawer-workspace__badges,
	.console-drawer-workspace__actions {
		align-items: flex-start;
		justify-content: flex-start;
	}

	.console-drawer-workspace__metrics {
		grid-template-columns: repeat(2, minmax(0, 1fr));
	}
}

@media (max-width: 767px) {
	.console-drawer-workspace__hero {
		padding: 20px 18px;
		border-radius: 22px;
	}

	.console-drawer-workspace__actions {
		width: 100%;
	}

	.console-drawer-workspace__actions :deep(.el-button) {
		flex: 1;
		min-width: 0;
	}

	.console-drawer-workspace__metrics {
		grid-template-columns: 1fr;
	}
}
</style>
