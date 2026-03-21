<template>
	<div>
		<el-drawer
			v-model="state.isOpen"
			size="760px"
			class="workflow-config-drawer"
			append-to-body
			destroy-on-close
		>
			<div class="workflow-config-shell">
				<section class="workflow-config-shell__hero">
					<div>
						<div class="workflow-config-shell__eyebrow">{{ panelMeta.eyebrow }}</div>
						<h2>{{ panelMeta.title }}</h2>
						<p>{{ panelMeta.description }}</p>
					</div>
					<div class="workflow-config-shell__badges">
						<span v-for="badge in panelBadges" :key="badge" class="workflow-config-shell__badge">{{ badge }}</span>
					</div>
				</section>

				<section class="workflow-config-shell__body">
					<Line
						v-if="state.panelKind === 'line'"
						:model-value="state.lineData"
						@submit="handlePanelSubmit"
						@close="close"
					/>
					<BasicPanel
						v-else-if="state.panelKind === 'basic'"
						:model-value="state.nodeData"
						@submit="handlePanelSubmit"
						@close="close"
					/>
					<ExecutorPanel
						v-else-if="state.panelKind === 'executor'"
						:model-value="state.nodeData"
						@submit="handlePanelSubmit"
						@close="close"
					/>
					<ScriptPanel
						v-else-if="state.panelKind === 'script'"
						:model-value="state.nodeData"
						@submit="handlePanelSubmit"
						@close="close"
					/>
				</section>
			</div>
		</el-drawer>
	</div>
</template>

<script lang="ts" setup>
import { computed, reactive } from 'vue';
import BasicPanel from './basic.vue';
import ExecutorPanel from './executor.vue';
import Line from './line.vue';
import ScriptPanel from './script.vue';

interface WorkflowDrawerState {
	isOpen: boolean;
	panelKind: 'line' | 'basic' | 'executor' | 'script';
	nodeData: Record<string, any>;
	lineData: Record<string, any>;
}

const emit = defineEmits(['panelclose', 'executor', 'script']);

const state = reactive<WorkflowDrawerState>({
	isOpen: false,
	panelKind: 'executor',
	nodeData: {},
	lineData: {},
});

const panelMeta = computed(() => {
	switch (state.panelKind) {
		case 'line':
			return {
				eyebrow: 'Rule Link',
				title: '连线条件配置',
				description: '设置当前连线的显示名称与流转条件，帮助规则在复杂分支中保持可读和可维护。',
			};
		case 'basic':
			return {
				eyebrow: 'Base Node',
				title: '基础节点配置',
				description: '维护开始或结束节点的名称与基础标识，保证规则画布上的语义足够清晰。',
			};
		case 'script':
			return {
				eyebrow: 'Script Node',
				title: '脚本节点配置',
				description: '继续完善脚本内容和节点名称，让数据清洗、转换与分支判断更集中、更易回看。',
			};
		default:
			return {
				eyebrow: 'Executor Node',
				title: '执行器节点配置',
				description: '在这里编辑执行器 JSON 配置，并结合右侧说明快速确认输入输出约定。',
			};
	}
});

const panelBadges = computed(() => {
	if (state.panelKind === 'line') {
		return [
			state.lineData.linename || '未命名连线',
			state.lineData.condition ? '已配置条件' : '无条件流转',
			state.lineData.sourceId && state.lineData.targetId ? `${state.lineData.sourceId} -> ${state.lineData.targetId}` : '待确认连接',
		];
	}

	return [
		state.nodeData.nodetype || '--',
		state.nodeData.name || '未命名节点',
		state.nodeData.nodeId || '待生成标识',
	];
});

const open = (item: any, conn?: any) => {
	state.isOpen = true;

	if (item.type === 'line') {
		state.panelKind = 'line';
		state.lineData = {
			sourceId: conn?.sourceId,
			targetId: conn?.targetId,
			linename: conn?.linename ?? item.linename ?? '',
			label: item.label ?? '',
			condition: conn?.condition ?? item.condition ?? '',
			type: item.type,
			contact: item.contact,
		};
		return;
	}

	state.panelKind = item.nodetype === 'script' ? 'script' : item.nodetype === 'basic' ? 'basic' : 'executor';
	state.nodeData = {
		...(item ?? {}),
		content: item?.content ?? '',
		name: item?.name ?? '',
	};
};

const close = () => {
	state.isOpen = false;
};

const handlePanelSubmit = (payload: Record<string, any>) => {
	if (state.panelKind === 'script') emit('script', payload);
	if (state.panelKind === 'executor') emit('executor', payload);
	emit('panelclose', payload);
	close();
};

defineExpose({
	open,
});
</script>

<style scoped lang="scss">
:deep(.workflow-config-drawer .el-drawer__header) {
	margin-bottom: 0;
	padding-bottom: 0;
}

:deep(.workflow-config-drawer .el-drawer__body) {
	padding: 18px;
	background: linear-gradient(180deg, #f8fbff 0%, #f3f7fc 100%);
}

.workflow-config-shell {
	display: flex;
	flex-direction: column;
	gap: 18px;
	min-height: 100%;
}

.workflow-config-shell__hero {
	display: flex;
	align-items: flex-start;
	justify-content: space-between;
	gap: 16px;
	padding: 22px 24px;
	border-radius: 26px;
	border: 1px solid rgba(191, 219, 254, 0.78);
	background:
		radial-gradient(circle at top right, rgba(96, 165, 250, 0.18), transparent 28%),
		linear-gradient(180deg, rgba(248, 251, 255, 0.98), rgba(240, 247, 255, 0.96));
	box-shadow: 0 18px 42px rgba(15, 23, 42, 0.06);
}

.workflow-config-shell__eyebrow {
	margin-bottom: 10px;
	color: #2563eb;
	font-size: 12px;
	font-weight: 700;
	letter-spacing: 0.18em;
	text-transform: uppercase;
}

.workflow-config-shell__hero h2 {
	margin: 0;
	color: #123b6d;
	font-size: 28px;
	letter-spacing: -0.05em;
}

.workflow-config-shell__hero p {
	margin: 10px 0 0;
	color: #64748b;
	font-size: 13px;
	line-height: 1.8;
}

.workflow-config-shell__badges {
	display: flex;
	flex-wrap: wrap;
	justify-content: flex-end;
	gap: 10px;
}

.workflow-config-shell__badge {
	display: inline-flex;
	align-items: center;
	min-height: 34px;
	padding: 0 12px;
	border-radius: 999px;
	border: 1px solid rgba(191, 219, 254, 0.9);
	background: rgba(255, 255, 255, 0.74);
	color: #2563eb;
	font-size: 12px;
	font-weight: 700;
	white-space: nowrap;
}

.workflow-config-shell__body {
	flex: 1;
	min-height: 0;
}

@media (max-width: 767px) {
	:deep(.workflow-config-drawer) {
		width: 100% !important;
	}

	:deep(.workflow-config-drawer .el-drawer__body) {
		padding: 12px;
	}

	.workflow-config-shell__hero {
		flex-direction: column;
		padding: 18px;
		border-radius: 20px;
	}

	.workflow-config-shell__badges {
		justify-content: flex-start;
	}
}
</style>
