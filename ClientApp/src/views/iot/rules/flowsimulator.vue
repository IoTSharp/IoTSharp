<template>
	<div class="rule-simulator">
		<ConsolePageShell
			eyebrow="Rule Simulator"
			title="规则模拟器"
			description="在同一工作台中回看规则画布、提交测试数据并沿时间线查看节点执行轨迹，适合调试规则逻辑和验证执行链路。"
			:badges="simulatorBadges"
			:metrics="simulatorMetrics"
		>
			<div class="simulator-workspace">
				<div class="simulator-toolbar">
					<Tool @tool="onToolClick" />
				</div>

				<div class="simulator-layout">
					<section class="simulator-panel simulator-panel--canvas">
						<div class="simulator-panel__head">
							<div>
								<span>规则画布回放</span>
								<small>提交测试数据后，命中的节点会在画布上按执行顺序高亮显示。</small>
							</div>
							<el-tag effect="plain" type="primary">{{ state.jsplumbData.nodeList.length }} 个节点</el-tag>
						</div>

						<div class="workflow-stage">
							<div class="workflow-mask" v-if="state.isShow">
								<div class="workflow-mask__content">移动端暂不支持规则模拟画布交互，请在桌面端继续操作。</div>
							</div>

							<div class="workflow-right" ref="workflowRightRef">
								<div
									v-for="(node, index) in state.jsplumbData.nodeList"
									:key="node.nodeId"
									:id="node.nodeId"
									:data-node-id="node.nodeId"
									:class="node.nodeclass"
									:style="{ left: node.left, top: node.top }"
									@click="onItemCloneClick(index)"
								>
									<div class="workflow-right-box" :class="{ 'workflow-right-active': state.jsPlumbNodeIndex === index }">
										<div class="workflow-right-box__inner">
											<div class="workflow-right-box__icon" :style="{ backgroundColor: node.color }">
												<component :is="{ ...customIcons[node.icon] }" class="workflow-icon-drag" />
											</div>
											<div class="workflow-right-box__copy">
												<strong>{{ node.name }}</strong>
												<span>{{ node.nodetype }}</span>
											</div>
										</div>
									</div>
								</div>
							</div>
						</div>
					</section>

					<aside class="simulator-panel simulator-panel--timeline">
						<div class="simulator-panel__head">
							<div>
								<span>执行时间线</span>
								<small>按执行先后展示规则节点的事件记录和返回数据。</small>
							</div>
							<el-tag effect="plain">{{ state.activities.length }} 条事件</el-tag>
						</div>

						<el-tabs v-model="activeName" type="card" class="timeline-tabs">
							<el-tab-pane label="执行轨迹" name="timeline">
								<div class="timeline-panel">
									<el-empty v-if="!state.activities.length" description="尚未执行模拟，请先提交测试数据。" />
									<el-timeline v-else>
										<el-timeline-item
											v-for="(activity, index) in state.activities"
											:key="index"
											:type="activity.type"
											:timestamp="activity.timestamp"
										>
											<el-card class="timeline-card">
												<h4>{{ activity.content }}</h4>
												<p>{{ activity.data }}</p>
											</el-card>
										</el-timeline-item>
									</el-timeline>
								</div>
							</el-tab-pane>
						</el-tabs>
					</aside>
				</div>
			</div>
		</ConsolePageShell>

		<el-dialog v-model="state.dataFormVisible" title="测试数据" width="860px" class="simulator-dialog">
			<div class="simulator-dialog__body">
				<monaco
					height="420px"
					width="100%"
					theme="vs-dark"
					v-model="state.content"
					language="json"
					selectOnLineNumbers="true"
				/>
			</div>

			<template #footer>
				<span class="dialog-footer">
					<el-button @click="state.dataFormVisible = false">取消</el-button>
					<el-button type="primary" @click="submitData">开始模拟</el-button>
				</span>
			</template>
		</el-dialog>
	</div>
</template>

<script lang="ts" setup>
import { computed, onMounted, onUnmounted, reactive, ref, watch } from 'vue';
import { ElMessage } from 'element-plus';
import { jsPlumb } from 'jsplumb';
import { useTagsViewRoutes } from '/@/stores/tagsViewRoutes';
import ConsolePageShell from '/@/components/console/ConsolePageShell.vue';
import Tool from './component/tool/simulator.vue';
import monaco from '/@/components/monaco/monaco.vue';
import { jsplumbConnect, jsplumbDefaults, jsplumbMakeSource, jsplumbMakeTarget } from './js/config';
import { customIcons } from './js/leftNavList';
import { ruleApi } from '/@/api/flows';
import type { LineListState, NodeListState } from './models';

interface XyState {
	x: string | number;
	y: string | number;
}

interface FlowState {
	activities: any[];
	flowid: string;
	content: string;
	dropdownNode: XyState;
	dropdownLine: XyState;
	isShow: boolean;
	jsPlumb: any;
	jsPlumbNodeIndex: null | number;
	jsplumbDefaults: any;
	jsplumbMakeSource: any;
	jsplumbMakeTarget: any;
	jsplumbConnect: any;
	dataFormVisible: boolean;
	jsplumbData: {
		nodeList: Array<NodeListState>;
		lineList: Array<LineListState>;
	};
}

const props = defineProps({
	ruleId: {
		type: String,
		default: '',
	},
});

const emit = defineEmits(['close', 'submit']);
const stores = useTagsViewRoutes();
const activeName = ref('timeline');
const workflowRightRef = ref<HTMLDivElement | null>(null);
const playbackTimers: number[] = [];

const state = reactive<FlowState>({
	activities: [],
	flowid: props.ruleId,
	content: '',
	dropdownNode: { x: '', y: '' },
	dropdownLine: { x: '', y: '' },
	isShow: false,
	jsPlumb: null,
	jsPlumbNodeIndex: null,
	jsplumbDefaults,
	jsplumbMakeSource,
	jsplumbMakeTarget,
	jsplumbConnect,
	dataFormVisible: false,
	jsplumbData: {
		nodeList: [],
		lineList: [],
	},
});

const simulatorBadges = computed(() => [
	props.ruleId ? `规则 ${props.ruleId.slice(0, 8)}` : '未选择规则',
	state.activities.length ? '已生成执行轨迹' : '等待首次模拟',
	state.isShow ? '移动端受限' : '桌面交互正常',
]);

const simulatorMetrics = computed(() => [
	{
		label: '画布节点',
		value: state.jsplumbData.nodeList.length,
		hint: '当前参与模拟的规则节点数量。',
		tone: 'primary' as const,
	},
	{
		label: '连线数量',
		value: state.jsplumbData.lineList.length,
		hint: '可帮助判断当前规则流的复杂度。',
		tone: 'accent' as const,
	},
	{
		label: '执行事件',
		value: state.activities.length,
		hint: '模拟后会按时间顺序生成执行事件记录。',
		tone: 'success' as const,
	},
	{
		label: '当前状态',
		value: state.activities.length ? '回放完成' : '待运行',
		hint: state.activities.length ? '可以继续修改输入数据再次回放。' : '点击顶部工具栏即可发起测试。',
		tone: 'warning' as const,
	},
]);

const clearPlaybackTimers = () => {
	while (playbackTimers.length) {
		const timer = playbackTimers.pop();
		if (timer) window.clearTimeout(timer);
	}
};

const setClientWidth = () => {
	state.isShow = document.body.clientWidth < 768;
};

const initRightNodeList = async () => {
	const response = await ruleApi().getDiagram(state.flowid);
	state.jsplumbData = {
		nodeList: response.data.nodes,
		lineList: response.data.lines,
	};
};

const initJsPlumbConnection = () => {
	state.jsplumbData.nodeList.forEach((item) => {
		state.jsPlumb.makeSource(item.nodeId, state.jsplumbMakeSource);
		state.jsPlumb.makeTarget(item.nodeId, state.jsplumbMakeTarget, jsplumbConnect);
	});

	state.jsplumbData.lineList.forEach((item) => {
		state.jsPlumb.connect(
			{
				source: item.sourceId,
				target: item.targetId,
				label: item.linename,
				linename: item.linename,
				condition: item.condition,
			},
			state.jsplumbConnect
		);
	});
};

const initJsPlumb = () => {
	(jsPlumb as any).ready(() => {
		state.jsPlumb = (jsPlumb as any).getInstance({
			detachable: false,
			Container: 'workflow-right',
		});
		state.jsPlumb.fire('jsPlumbDemoLoaded', state.jsPlumb);
		state.jsPlumb.importDefaults(state.jsplumbDefaults);
		state.jsPlumb.setSuspendDrawing(false, true);
		initJsPlumbConnection();
	});
};

const onItemCloneClick = (index: number) => {
	state.jsPlumbNodeIndex = index;
};

const onReturnToList = () => {
	emit('close', state.jsplumbData);
};

const openRunDialog = () => {
	state.dataFormVisible = true;
	state.content = state.content || '{\n  \n}';
};

const onToolSubmit = () => {
	openRunDialog();
};

const onToolFullscreen = () => {
	stores.setCurrenFullscreen(true);
};

const onToolClick = (fnName: string) => {
	switch (fnName) {
		case 'submit':
			onToolSubmit();
			break;
		case 'fullscreen':
			onToolFullscreen();
			break;
		case 'return':
			onReturnToList();
			break;
	}
};

const setHighlightBatch = (item: any) => {
	for (const nodeInfo of item.nodes) {
		const currentNode = state.jsplumbData.nodeList.find((node) => node.nodeId === nodeInfo.bpmnid);
		if (currentNode) {
			state.activities = [
				...state.activities,
				{
					timestamp: nodeInfo.addDate,
					content: nodeInfo.operationDesc,
					data: nodeInfo.data,
				},
			];
			currentNode.nodeclass = 'workflow-right-highlight';
		}
	}
};

const clearHighlightClass = () => {
	for (const node of state.jsplumbData.nodeList) {
		node.nodeclass = 'workflow-right-clone';
	}
};

const submitData = async () => {
	state.dataFormVisible = false;

	let formData: any;
	try {
		formData = JSON.parse(state.content);
	}
	catch {
		ElMessage.warning('请输入合法的 JSON 测试数据');
		return;
	}

	clearPlaybackTimers();
	clearHighlightClass();
	state.activities = [];

	const response = await ruleApi().active({
		form: formData,
		extradata: { ruleflowid: state.flowid },
	});

	for (let index = 0; index < response.data.length; index += 1) {
		const timer = window.setTimeout(() => {
			setHighlightBatch(response.data[index]);
		}, index * 500);
		playbackTimers.push(timer);
	}

	const clearTimer = window.setTimeout(() => {
		clearHighlightClass();
	}, response.data.length * 1000);
	playbackTimers.push(clearTimer);

	ElMessage.success('测试数据已提交，正在回放执行轨迹');
};

const initData = async () => {
	if (!props.ruleId) return;

	state.flowid = props.ruleId;
	state.activities = [];
	clearPlaybackTimers();
	state.jsPlumb?.reset?.();
	await initRightNodeList();
	initJsPlumb();
	setClientWidth();
};

watch(
	() => props.ruleId,
	async () => {
		await initData();
	}
);

onMounted(async () => {
	await initData();
	window.addEventListener('resize', setClientWidth);
});

onUnmounted(() => {
	window.removeEventListener('resize', setClientWidth);
	clearPlaybackTimers();
	state.jsPlumb?.reset?.();
});
</script>

<style lang="scss" scoped>
.rule-simulator {
	display: flex;
	flex-direction: column;
	gap: 18px;
}

.simulator-workspace {
	padding: 20px 22px;
	border-radius: 28px;
	border: 1px solid rgba(226, 232, 240, 0.92);
	background: linear-gradient(180deg, rgba(248, 251, 255, 0.96), rgba(255, 255, 255, 0.98));
	box-shadow: 0 18px 42px rgba(15, 23, 42, 0.05);
}

.simulator-toolbar {
	padding: 4px 0 16px;
}

.simulator-layout {
	display: grid;
	grid-template-columns: minmax(0, 1.7fr) 360px;
	gap: 18px;
}

.simulator-panel {
	padding: 18px;
	border-radius: 24px;
	border: 1px solid rgba(226, 232, 240, 0.92);
	background:
		radial-gradient(circle at top right, rgba(59, 130, 246, 0.08), transparent 32%),
		linear-gradient(180deg, rgba(255, 255, 255, 0.98), rgba(248, 250, 252, 0.96));
	box-shadow: 0 18px 42px rgba(15, 23, 42, 0.04);
	min-width: 0;
}

.simulator-panel__head {
	display: flex;
	align-items: center;
	justify-content: space-between;
	gap: 12px;
	margin-bottom: 14px;

	span {
		display: block;
		color: #123b6d;
		font-size: 18px;
		font-weight: 700;
		letter-spacing: -0.03em;
	}

	small {
		display: block;
		margin-top: 4px;
		color: #64748b;
		font-size: 12px;
		line-height: 1.6;
	}
}

.workflow-stage {
	position: relative;
	min-height: 720px;
	border-radius: 22px;
	border: 1px solid rgba(226, 232, 240, 0.92);
	background: rgba(255, 255, 255, 0.76);
	overflow: hidden;
}

.workflow-right {
	position: relative;
	height: 720px;
	overflow: hidden;
	background:
		linear-gradient(90deg, rgba(191, 219, 254, 0.28) 1px, transparent 1px),
		linear-gradient(rgba(191, 219, 254, 0.28) 1px, transparent 1px),
		linear-gradient(180deg, rgba(248, 251, 255, 0.96), rgba(240, 247, 255, 0.92));
	background-size: 24px 24px, 24px 24px, cover;
}

.workflow-right-clone,
.workflow-right-highlight {
	position: absolute;
}

.workflow-right-box {
	min-width: 148px;
	padding: 10px;
	border-radius: 18px;
	border: 1px solid rgba(191, 219, 254, 0.88);
	background: rgba(255, 255, 255, 0.95);
	box-shadow: 0 12px 28px rgba(15, 23, 42, 0.08);
	cursor: pointer;
}

.workflow-right-box__inner {
	display: flex;
	align-items: center;
	gap: 12px;
	min-height: 52px;
}

.workflow-right-box__icon {
	display: inline-flex;
	align-items: center;
	justify-content: center;
	width: 44px;
	height: 44px;
	border-radius: 14px;
	color: #123b6d;
	flex-shrink: 0;
}

.workflow-right-box__copy {
	display: flex;
	flex-direction: column;
	gap: 4px;
	min-width: 0;

	strong {
		color: #0f172a;
		font-size: 14px;
		overflow: hidden;
		text-overflow: ellipsis;
		white-space: nowrap;
	}

	span {
		color: #64748b;
		font-size: 12px;
		text-transform: uppercase;
	}
}

.workflow-right-active {
	border-color: rgba(37, 99, 235, 0.98);
	background: linear-gradient(180deg, rgba(239, 246, 255, 0.96), rgba(219, 234, 254, 0.8));
	box-shadow: 0 18px 36px rgba(37, 99, 235, 0.18);
}

.workflow-right-highlight .workflow-right-box {
	border-color: rgba(14, 165, 233, 0.92);
	background: linear-gradient(180deg, rgba(224, 242, 254, 0.96), rgba(191, 219, 254, 0.88));
}

:deep(.jtk-overlay):not(.aLabel) {
	padding: 6px 10px;
	border: 1px solid rgba(191, 219, 254, 0.72) !important;
	color: #475569 !important;
	background: rgba(255, 255, 255, 0.95) !important;
	border-radius: 999px;
	font-size: 11px;
	box-shadow: 0 10px 20px rgba(15, 23, 42, 0.08);
}

:deep(.jtk-overlay.workflow-right-empty-label) {
	display: none;
}

.timeline-tabs {
	min-height: 720px;
}

.timeline-panel {
	max-height: 650px;
	padding-right: 8px;
	overflow: auto;
}

.timeline-card {
	border-radius: 18px;
	box-shadow: none;

	h4 {
		margin: 0 0 8px;
		color: #123b6d;
		font-size: 14px;
	}

	p {
		margin: 0;
		color: #64748b;
		font-size: 12px;
		line-height: 1.75;
		white-space: pre-wrap;
		word-break: break-word;
	}
}

.workflow-mask {
	position: absolute;
	inset: 0;
	z-index: 2;
	display: flex;
	align-items: center;
	justify-content: center;
	background: rgba(255, 255, 255, 0.86);
	backdrop-filter: blur(4px);
}

.workflow-mask__content {
	max-width: 320px;
	padding: 18px 20px;
	border-radius: 20px;
	border: 1px solid rgba(191, 219, 254, 0.88);
	background: rgba(255, 255, 255, 0.94);
	color: #475569;
	font-size: 14px;
	line-height: 1.8;
	text-align: center;
}

.workflow-icon-drag {
	flex-shrink: 0;
}

.simulator-dialog :deep(.el-dialog) {
	border-radius: 24px;
}

.simulator-dialog__body {
	border-radius: 20px;
	overflow: hidden;
}

@media (max-width: 1080px) {
	.simulator-layout {
		grid-template-columns: 1fr;
	}

	.workflow-stage,
	.workflow-right {
		min-height: 560px;
		height: 560px;
	}

	.timeline-tabs {
		min-height: auto;
	}

	.timeline-panel {
		max-height: 360px;
	}
}

@media (max-width: 767px) {
	.simulator-workspace {
		padding: 18px;
		border-radius: 22px;
	}

	.simulator-panel {
		padding: 16px;
		border-radius: 20px;
	}
}
</style>
