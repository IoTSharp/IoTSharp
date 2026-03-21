<template>
	<div class="workflow-tool">
		<div class="workflow-tool__summary">
			<div class="workflow-tool__title">
				<span>规则模拟工具栏</span>
				<small>{{ setToolTitle }}</small>
			</div>

			<div class="workflow-tool__badges">
				<span>测试数据</span>
				<span>轨迹回放</span>
				<span>结果复盘</span>
			</div>
		</div>

		<div class="workflow-tool__actions">
			<button
				v-for="item in toolList"
				:key="item.fnName"
				type="button"
				class="workflow-tool__action"
				:class="`tone-${item.tone}`"
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
		{ icon: 'iconfont icon-AIshiyanshi', title: '开始模拟', fnName: 'submit', tone: 'primary' },
		{ icon: 'ele-FullScreen', title: '全屏', fnName: 'fullscreen', tone: 'default' },
		{ icon: 'ele-Back', title: '返回', fnName: 'return', tone: 'ghost' },
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
	gap: 18px;
	padding: 16px 18px;
	border-radius: 24px;
	border: 1px solid rgba(191, 219, 254, 0.92);
	background:
		radial-gradient(circle at top right, rgba(14, 165, 233, 0.12), transparent 34%),
		linear-gradient(180deg, rgba(255, 255, 255, 0.94), rgba(248, 250, 252, 0.92));
	box-shadow: 0 16px 32px rgba(15, 23, 42, 0.05);
}

.workflow-tool__summary {
	display: flex;
	flex-direction: column;
	gap: 10px;
	min-width: 0;
}

.workflow-tool__title {
	min-width: 0;

	span {
		display: block;
		color: #123b6d;
		font-size: 16px;
		font-weight: 700;
	}

	small {
		display: block;
		margin-top: 6px;
		color: #64748b;
		font-size: 12px;
		line-height: 1.6;
		word-break: break-word;
	}
}

.workflow-tool__badges {
	display: flex;
	flex-wrap: wrap;
	gap: 8px;

	span {
		display: inline-flex;
		align-items: center;
		min-height: 30px;
		padding: 0 12px;
		border-radius: 999px;
		border: 1px solid rgba(165, 243, 252, 0.92);
		background: rgba(236, 254, 255, 0.88);
		color: #0891b2;
		font-size: 11px;
		font-weight: 700;
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
	gap: 8px;
	height: 42px;
	padding: 0 16px;
	border: 1px solid rgba(191, 219, 254, 0.88);
	border-radius: 14px;
	background: rgba(255, 255, 255, 0.82);
	color: #0891b2;
	cursor: pointer;
	transition: transform 0.18s ease, box-shadow 0.18s ease, border-color 0.18s ease, background-color 0.18s ease;

	span {
		font-size: 13px;
		font-weight: 600;
	}
}

.workflow-tool__action:hover {
	transform: translateY(-1px);
	box-shadow: 0 14px 26px rgba(14, 165, 233, 0.14);
}

.workflow-tool__action.tone-primary {
	border-color: rgba(8, 145, 178, 0.94);
	background: linear-gradient(135deg, rgba(8, 145, 178, 0.96), rgba(6, 182, 212, 0.88));
	color: #fff;
	box-shadow: 0 14px 28px rgba(8, 145, 178, 0.22);
}

.workflow-tool__action.tone-default:hover {
	border-color: rgba(34, 211, 238, 0.92);
	background: rgba(236, 254, 255, 0.96);
}

.workflow-tool__action.tone-ghost {
	color: #475569;
	background: rgba(248, 250, 252, 0.92);
}

.workflow-tool__action.tone-ghost:hover {
	border-color: rgba(148, 163, 184, 0.86);
	background: rgba(241, 245, 249, 0.96);
	box-shadow: 0 14px 26px rgba(100, 116, 139, 0.12);
}

@media (max-width: 1080px) {
	.workflow-tool {
		flex-direction: column;
		align-items: flex-start;
	}

	.workflow-tool__actions {
		justify-content: flex-start;
	}
}

@media (max-width: 767px) {
	.workflow-tool {
		padding: 16px;
		border-radius: 20px;
	}

	.workflow-tool__actions {
		width: 100%;
	}
}
</style>
