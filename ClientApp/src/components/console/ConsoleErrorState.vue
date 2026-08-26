<template>
	<section class="console-error-state">
		<AppLogo hideText class="console-error-state__mark" />
		<div class="console-error-state__code">{{ code }}</div>
		<h1>{{ title }}</h1>
		<p>{{ description }}</p>
		<el-button type="primary" @click="$emit('action')">{{ actionLabel }}</el-button>
	</section>
</template>

<script setup lang="ts">
import AppLogo from '/@/components/AppLogo.vue';

defineProps<{
	code: string;
	title: string;
	description: string;
	actionLabel: string;
}>();

defineEmits<{ action: [] }>();
</script>

<style scoped lang="scss">
.console-error-state {
	display: flex;
	min-height: 100%;
	align-items: center;
	justify-content: center;
	flex-direction: column;
	padding: 44px 20px;
	background: var(--iotsharp-quiet-gradient);
	text-align: center;
}

.console-error-state__mark {
	margin-bottom: 24px;
}

.console-error-state__code {
	color: var(--iotsharp-accent);
	font-size: 54px;
	font-weight: 760;
	font-variant-numeric: tabular-nums;
	line-height: 1;
}

.console-error-state h1 {
	margin: 15px 0 0;
	color: var(--iotsharp-ink);
	font-size: 22px;
	letter-spacing: 0;
}

.console-error-state p {
	max-width: 52ch;
	margin: 8px 0 22px;
	color: var(--iotsharp-text-soft);
	font-size: 12px;
	line-height: 1.7;
}

.console-error-state :deep(.el-button) {
	height: 36px;
	border-radius: var(--iotsharp-radius-control);
}
</style>
