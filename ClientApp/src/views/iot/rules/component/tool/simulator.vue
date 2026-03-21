<template>
	<div class="workflow-tool">
		<div class="workflow-tool__title">
			<span>规则模拟工具栏</span>
			<small>{{ setToolTitle }}</small>
		</div>
		<div class="workflow-tool__actions">
			<button
				v-for="item in toolList"
				:key="item.fnName"
				type="button"
				class="workflow-tool__action"
				:title="item.title"
				@click="onToolClick(item.fnName)"
			>
				<SvgIcon :name="item.icon" />
				<span>{{ item.title }}</span>
			</button>
		</div>
	</div>
</template>

<script lang="ts" setup>
import { computed, reactive, toRefs } from 'vue';
import { storeToRefs } from 'pinia';
import { useThemeConfig } from '/@/stores/themeConfig';

const emit = defineEmits(['tool']);

const storesThemeConfig = useThemeConfig();
const { themeConfig } = storeToRefs(storesThemeConfig);

const state = reactive({
	toolList: [
		{ icon: 'iconfont icon-AIshiyanshi', title: '开始模拟', fnName: 'submit' },
		{ icon: 'ele-FullScreen', title: '全屏', fnName: 'fullscreen' },
		{ icon: 'ele-Back', title: '返回', fnName: 'return' },
	],
});

const setToolTitle = computed(() => {
	const { globalTitle } = themeConfig.value;
	return `${globalTitle} / 规则模拟`;
});

const onToolClick = (fnName: string) => {
	emit('tool', fnName);
};

const { toolList } = toRefs(state);
</script>

<style scoped lang="scss">
.workflow-tool {
	display: flex;
	align-items: center;
	justify-content: space-between;
	gap: 16px;
	padding: 14px 16px;
	border-radius: 20px;
	border: 1px solid rgba(191, 219, 254, 0.9);
	background: rgba(255, 255, 255, 0.8);
}

.workflow-tool__title {
	min-width: 0;

	span {
		display: block;
		color: #123b6d;
		font-size: 15px;
		font-weight: 700;
	}

	small {
		display: block;
		margin-top: 4px;
		color: #64748b;
		font-size: 12px;
		line-height: 1.5;
	}
}

.workflow-tool__actions {
	display: flex;
	flex-wrap: wrap;
	justify-content: flex-end;
	gap: 10px;
}

.workflow-tool__action {
	display: inline-flex;
	align-items: center;
	gap: 6px;
	height: 40px;
	padding: 0 14px;
	border: 1px solid rgba(191, 219, 254, 0.9);
	border-radius: 14px;
	background: rgba(239, 246, 255, 0.78);
	color: #2563eb;
	cursor: pointer;
	transition: transform 0.18s ease, box-shadow 0.18s ease, background-color 0.18s ease;

	span {
		font-size: 13px;
		font-weight: 600;
	}
}

.workflow-tool__action:hover {
	transform: translateY(-1px);
	box-shadow: 0 12px 24px rgba(59, 130, 246, 0.16);
	background: rgba(219, 234, 254, 0.96);
}

@media (max-width: 767px) {
	.workflow-tool {
		flex-direction: column;
		align-items: flex-start;
	}

	.workflow-tool__actions {
		width: 100%;
		justify-content: flex-start;
	}
}
</style>
