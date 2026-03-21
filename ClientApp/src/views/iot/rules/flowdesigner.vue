<template>
	<div class="rule-designer">
		<ConsolePageShell
			eyebrow="Rule Designer"
			:title="state.ruleName || '规则设计器'"
			description="继续沿用当前规则编排能力，但把节点库、画布和工具操作重新组织成更清晰的工作台布局，方便持续拖拽建模和回看规则结构。"
			:badges="designerBadges"
			:metrics="designerMetrics"
		>
			<div class="designer-workspace">
				<div class="designer-toolbar">
					<Tool @tool="onToolClick" :ruleName="state.ruleName" />
				</div>

				<div class="designer-body">
					<aside class="designer-panel designer-panel--library">
						<div class="designer-panel__head">
							<div>
								<span>节点库</span>
								<small>按分类拖拽节点到右侧画布</small>
							</div>
							<el-tag effect="plain">{{ libraryNodeCount }} 个节点</el-tag>
						</div>

						<div class="workflow-left">
							<el-scrollbar view-style="padding: 12px 12px 16px;">
								<div
									ref="leftNavRefs"
									v-for="val in state.leftNavList"
									:key="val.id + val.title"
									class="workflow-left-id"
									:style="{ height: val.isOpen ? 'auto' : '52px', overflow: 'hidden' }"
								>
									<div class="workflow-left-title" @click="onTitleClick(val)">
										<div>
											<strong>{{ val.title }}</strong>
											<small>{{ val.children?.length || 0 }} 个节点</small>
										</div>
										<SvgIcon :name="val.isOpen ? 'ele-ArrowDown' : 'ele-ArrowRight'" />
									</div>

									<div
										v-for="(child, childIndex) in val.children"
										:key="`${val.title}-${child.id}-${childIndex}`"
										class="workflow-left-item"
										:data-color="val.color"
										:data-name="child.name"
										:data-icon="child.icon"
										:data-id="child.id"
										:nodetype="child.nodetype"
										:nodenamespace="child.nodenamespace"
										:mata="child.mata"
									>
										<div class="workflow-left-item-icon" :style="{ backgroundColor: val.color }">
											<component :is="{ ...customIcons[child.icon] }" class="workflow-icon-drag" />
											<div class="workflow-left-item-copy">
												<strong>{{ child.name }}</strong>
												<span>{{ child.nodetype }}</span>
											</div>
										</div>
									</div>
								</div>
							</el-scrollbar>
						</div>
					</aside>

					<section class="designer-panel designer-panel--canvas">
						<div class="designer-panel__head">
							<div>
								<span>规则画布</span>
								<small>右键节点或连线继续配置，拖拽节点可调整整体编排结构</small>
							</div>
							<el-tag effect="plain" type="primary">{{ state.jsplumbData.nodeList.length }} 个节点</el-tag>
						</div>

						<div class="workflow-stage">
							<div class="workflow-mask" v-if="state.isShow">
								<div class="workflow-mask__content">移动端暂不支持规则拖拽设计，请在桌面端继续操作。</div>
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
									@contextmenu.prevent="onContextmenu(node, index, $event)"
									v-on-click-outside="
										() => {
											onItemCloneClickOutside(index);
										}
									"
								>
									<div
										class="workflow-right-box"
										:class="{ 'workflow-right-active': state.jsPlumbNodeIndex === index }"
									>
										<div class="workflow-right-box__inner">
											<div class="workflow-right-box__icon" :style="{ backgroundColor: node.color }">
												<component :is="{ ...customIcons[node.icon] }"></component>
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
				</div>
			</div>
		</ConsolePageShell>

		<ContextmenuNode ref="contextmenuNodeRef" :dropdown="state.dropdownNode" @currentnode="onCurrentNodeClick" />
		<ContextmenuLine ref="contextmenuLineRef" :dropdown="state.dropdownLine" @currentline="onCurrentLineClick" />
		<Drawer ref="drawerRef" @executor="onexecutorSubmit" @script="onscriptSubmit" @panelclose="panelClose" />
		<Help ref="helpRef" />
	</div>
</template>

<script lang="ts" setup>
import { computed, nextTick, onMounted, onUnmounted, reactive, ref, watch } from 'vue';
import { vOnClickOutside } from '@vueuse/components';
import { ElMessage, ElMessageBox } from 'element-plus';
import { jsPlumb } from 'jsplumb';
import Sortable from 'sortablejs';
import { storeToRefs } from 'pinia';
import ConsolePageShell from '/@/components/console/ConsolePageShell.vue';
import { useThemeConfig } from '/@/stores/themeConfig';
import { useTagsViewRoutes } from '/@/stores/tagsViewRoutes';
import Tool from './component/tool/index.vue';
import Help from './component/tool/help.vue';
import ContextmenuNode from './component/contextmenu/node.vue';
import ContextmenuLine from './component/contextmenu/line.vue';
import Drawer from './component/drawer/index.vue';
import commonFunction from '/@/utils/commonFunction';
import { customIcons, getGetLeftNavList } from './js/leftNavList';
import { jsplumbConnect, jsplumbDefaults, jsplumbMakeSource, jsplumbMakeTarget } from './js/config';
import { ruleApi } from '/@/api/flows';
import type { FlowState } from '/@/views/iot/rules/models';

const props = defineProps({
	ruleId: {
		type: String,
		default: '',
	},
});

const emit = defineEmits(['close', 'submit']);
const contextmenuNodeRef = ref();
const contextmenuLineRef = ref();
const drawerRef = ref();
const helpRef = ref();
const leftNavRefs = ref<HTMLElement[]>([]);
const workflowRightRef = ref<HTMLDivElement | null>(null);

const stores = useTagsViewRoutes();
const storesThemeConfig = useThemeConfig();
const { themeConfig } = storeToRefs(storesThemeConfig);
const { copyText } = commonFunction();

const state = reactive<FlowState>({
	ruleName: '',
	flowid: props.ruleId,
	leftNavList: [],
	dropdownNode: { x: '', y: '' },
	dropdownLine: { x: '', y: '' },
	isShow: false,
	jsPlumb: null,
	jsPlumbNodeIndex: null,
	jsplumbDefaults,
	jsplumbMakeSource,
	jsplumbMakeTarget,
	jsplumbConnect,
	jsplumbData: {
		nodeList: [],
		lineList: [],
	},
});

const libraryNodeCount = computed(() => {
	return state.leftNavList.reduce((count, item) => count + (item.children?.length || 0), 0);
});

const designerBadges = computed(() => [
	state.ruleName || '未命名规则',
	`${state.leftNavList.length} 个节点分组`,
	state.isShow ? '移动端受限' : '桌面拖拽可用',
]);

const designerMetrics = computed(() => [
	{
		label: '画布节点',
		value: state.jsplumbData.nodeList.length,
		hint: '当前规则画布中的可执行节点数量。',
		tone: 'primary' as const,
	},
	{
		label: '连线数量',
		value: state.jsplumbData.lineList.length,
		hint: '节点之间的连接关系会直接影响规则执行流向。',
		tone: 'accent' as const,
	},
	{
		label: '节点库容量',
		value: libraryNodeCount.value,
		hint: '包含基础节点、执行器节点和脚本节点。',
		tone: 'success' as const,
	},
	{
		label: '当前模式',
		value: state.isShow ? '受限查看' : '可拖拽编辑',
		hint: state.isShow ? '请切换桌面端完成拖拽设计。' : '支持拖拽、右键菜单和抽屉配置。',
		tone: 'warning' as const,
	},
]);

const setClientWidth = () => {
	state.isShow = document.body.clientWidth < 768;
};

const initLeftNavList = async () => {
	const nav = await getGetLeftNavList();
	state.leftNavList = nav ?? [];
	state.jsplumbData = {
		nodeList: [],
		lineList: [],
	};

	const [diagramResponse, ruleResponse] = await Promise.all([
		ruleApi().getDiagram(state.flowid),
		ruleApi().getrule(state.flowid),
	]);

	state.jsplumbData = {
		nodeList: diagramResponse.data.nodes,
		lineList: diagramResponse.data.lines,
	};
	state.ruleName = ruleResponse.data.name;
};

const initSortable = () => {
	leftNavRefs.value.forEach((item) => {
		Sortable.create(item, {
			group: {
				name: 'iotsharp-workflow-node-library',
				pull: 'clone',
				put: false,
			},
			animation: 0,
			sort: false,
			draggable: '.workflow-left-item',
			forceFallback: true,
			onEnd: (evt: any) => {
				const { name, icon, id, color } = evt.clone.dataset;
				const { nodetype, nodenamespace, mata } = evt.clone.attributes;
				const { layerX, layerY, clientX, clientY } = evt.originalEvent;
				const canvasElement = workflowRightRef.value;
				if (!canvasElement) return;

				const { x, y, width, height } = canvasElement.getBoundingClientRect();
				if (clientX < x || clientX > x + width || clientY < y || clientY > y + height) {
					ElMessage.warning('请把节点拖拽到右侧画布内');
					return;
				}

				const nodeId = Math.random().toString(36).substring(2, 14);
				const node = {
					nodeId,
					color,
					left: `${layerX - 40}px`,
					top: `${layerY - 15}px`,
					nodeclass: 'workflow-right-clone',
					nodetype: nodetype.value,
					nodenamespace: nodenamespace?.value ?? '',
					mata: mata?.value ?? '',
					name,
					icon,
					id,
				};

				state.jsplumbData.nodeList.push(node);
				nextTick(() => {
					state.jsPlumb.makeSource(nodeId, state.jsplumbMakeSource);
					state.jsPlumb.makeTarget(nodeId, state.jsplumbMakeTarget, jsplumbConnect);
					state.jsPlumb.draggable(nodeId, {
						containment: 'parent',
						stop: (event: any) => {
							state.jsplumbData.nodeList.forEach((currentNode) => {
								if (currentNode.nodeId === event.el.id) {
									currentNode.left = `${event.pos[0]}px`;
									currentNode.top = `${event.pos[1]}px`;
								}
							});
						},
					});
				});
			},
		});
	});
};

const initJsPlumbConnection = () => {
	state.jsplumbData.nodeList.forEach((item) => {
		state.jsPlumb.makeSource(item.nodeId, state.jsplumbMakeSource);
		state.jsPlumb.makeTarget(item.nodeId, state.jsplumbMakeTarget, jsplumbConnect);
		state.jsPlumb.draggable(item.nodeId, {
			containment: 'parent',
			stop: (event: any) => {
				state.jsplumbData.nodeList.forEach((currentNode) => {
					if (currentNode.nodeId === event.el.id) {
						currentNode.left = `${event.pos[0]}px`;
						currentNode.top = `${event.pos[1]}px`;
					}
				});
			},
		});
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

		state.jsPlumb.bind('contextmenu', (connection: any, originalEvent: MouseEvent) => {
			originalEvent.preventDefault();
			const { sourceId, targetId } = connection;
			state.dropdownLine.x = originalEvent.clientX;
			state.dropdownLine.y = originalEvent.clientY;
			const currentNode: any = state.jsplumbData.nodeList.find((item) => item.nodeId === targetId);
			const currentLine: any = state.jsplumbData.lineList.find((item) => item.sourceId === sourceId && item.targetId === targetId);
			currentNode.type = 'line';
			currentNode.label = currentLine.label;
			connection.linename = currentLine.linename;
			connection.condition = currentLine.condition;
			contextmenuLineRef.value.openContextmenu(currentNode, connection);
		});

		state.jsPlumb.bind('beforeDrop', (connection: any) => {
			const { sourceId, targetId } = connection;
			if (sourceId === targetId) {
				ElMessage.warning('连接目标不能是节点本身');
				return false;
			}

			const duplicated = state.jsplumbData.lineList.find((item) => item.sourceId === sourceId && item.targetId === targetId);
			if (duplicated) {
				ElMessage.warning('该连线已存在，不能重复连接');
				return false;
			}

			return true;
		});

		state.jsPlumb.bind('connection', (connection: any) => {
			const lineId = Math.random().toString(36).substring(2, 14);
			state.jsplumbData.lineList.push({
				lineId,
				sourceId: connection.sourceId,
				targetId: connection.targetId,
				label: '',
				linename: '',
				linenamespace: 'bpmn:SequenceFlow',
			});
		});

		state.jsPlumb.bind('connectionDetached', (connection: any) => {
			state.jsplumbData.lineList = state.jsplumbData.lineList.filter(
				(item) => !(item.sourceId === connection.sourceId && item.targetId === connection.targetId)
			);
		});
	});
};

const onTitleClick = (item: any) => {
	item.isOpen = !item.isOpen;
};

const onexecutorSubmit = () => {
	return undefined;
};

const onscriptSubmit = () => {
	return undefined;
};

const changeLineState = (nodeId: string, isActive: boolean) => {
	const lines = state.jsPlumb.getAllConnections();
	lines.forEach((line: any) => {
		if (line.targetId === nodeId || line.sourceId === nodeId) {
			line.canvas.classList.toggle('active', isActive);
		}
	});
};

const onItemCloneClick = (index: number) => {
	state.jsPlumbNodeIndex = index;
	const nodeId = state.jsplumbData.nodeList[index]?.nodeId;
	if (nodeId) changeLineState(nodeId, true);
};

const onItemCloneClickOutside = (index: number) => {
	state.jsPlumbNodeIndex = null;
	const nodeId = state.jsplumbData.nodeList[index]?.nodeId;
	if (nodeId) changeLineState(nodeId, false);
};

const onContextmenu = (item: any, index: number, event: MouseEvent) => {
	state.jsPlumbNodeIndex = index;
	state.dropdownNode.x = event.clientX;
	state.dropdownNode.y = event.clientY;
	item.type = 'node';
	item.label = '';

	let currentDefinition: any = {};
	state.leftNavList.forEach((group) => {
		if (group.children?.find((child: any) => child.id === item.id)) {
			currentDefinition = group.children.find((child: any) => child.id === item.id);
		}
	});

	item.from = currentDefinition.form;
	contextmenuNodeRef.value.openContextmenu(item);
};

const onCurrentNodeClick = (item: any) => {
	const { contextMenuClickId, nodeId } = item;
	if (contextMenuClickId === 0) {
		const nodeIndex = state.jsplumbData.nodeList.findIndex((currentNode) => currentNode.nodeId === nodeId);
		state.jsplumbData.nodeList.splice(nodeIndex, 1);
		state.jsPlumb.removeAllEndpoints(nodeId);
		state.jsPlumbNodeIndex = null;
		return;
	}

	if (contextMenuClickId === 1) {
		drawerRef.value.open(item);
	}
};

const onCurrentLineClick = (item: any, connection: any) => {
	const { contextMenuClickId } = item;
	const contactNodes: any[] = [];
	connection.endpoints.forEach((endpoint: any) => {
		contactNodes.push({
			id: endpoint.element.id,
			innerText: endpoint.element.innerText,
		});
	});
	item.contact = `${contactNodes[0].innerText}(${contactNodes[0].id}) => ${contactNodes[1].innerText}(${contactNodes[1].id})`;

	if (contextMenuClickId === 0) {
		state.jsPlumb.deleteConnection(connection);
	}
	else if (contextMenuClickId === 1) {
		drawerRef.value.open(item, connection);
	}
};

const setLineLabel = (payload: any) => {
	const { sourceId, targetId, label, linename, condition } = payload;
	const connection = state.jsPlumb.getConnections({ source: sourceId, target: targetId })[0];
	connection.setLabel(linename);
	if (!linename) {
		connection.addClass('workflow-right-empty-label');
	}
	else {
		connection.removeClass('workflow-right-empty-label');
		connection.addClass('workflow-right-label');
	}

	state.jsplumbData.lineList.forEach((item) => {
		if (item.sourceId === sourceId && item.targetId === targetId) {
			item.label = label;
			item.linename = linename;
			item.condition = condition;
		}
	});
};

const setNodeContent = (payload: any) => {
	const { nodeId, name, icon, nodetype, nodenamespace, mata, content } = payload;
	state.jsplumbData.nodeList.forEach((item) => {
		if (item.nodeId === nodeId) {
			item.name = name;
			item.icon = icon;
			item.nodetype = nodetype;
			item.nodenamespace = nodenamespace;
			item.mata = mata;
			item.content = content;
		}
	});

	nextTick(() => {
		state.jsPlumb.setSuspendDrawing(false, true);
	});
};

const onToolHelp = () => {
	nextTick(() => {
		helpRef.value.open();
	});
};

const onToolDownload = () => {
	const { globalTitle } = themeConfig.value;
	const href = `data:text/json;charset=utf-8,${encodeURIComponent(JSON.stringify(state.jsplumbData, null, '\t'))}`;
	const link = document.createElement('a');
	link.setAttribute('href', href);
	link.setAttribute('download', `${globalTitle}规则设计.json`);
	link.click();
	link.remove();
	ElMessage.success('规则设计文件已下载');
};

const onReturnToList = () => {
	emit('close', state.jsplumbData);
};

const onToolSubmit = async () => {
	const result = await ruleApi().saveDiagramV({
		RuleId: state.flowid,
		nodes: state.jsplumbData.nodeList,
		lines: state.jsplumbData.lineList,
	});

	if (result.data) {
		ElMessage.success('规则保存成功');
		onReturnToList();
	}
	else {
		ElMessage.warning(`规则保存失败: ${result.msg}`);
	}
};

const onToolCopy = () => {
	copyText(JSON.stringify(state.jsplumbData));
};

const onToolDel = () => {
	ElMessageBox.confirm('此操作将清空当前规则画布，是否继续？', '提示', {
		confirmButtonText: '清空画布',
		cancelButtonText: '取消',
	})
		.then(() => {
			state.jsplumbData.nodeList.forEach((item) => {
				state.jsPlumb.removeAllEndpoints(item.nodeId);
			});
			nextTick(() => {
				state.jsplumbData = {
					nodeList: [],
					lineList: [],
				};
				ElMessage.success('规则画布已清空');
			});
		})
		.catch(() => undefined);
};

const onToolFullscreen = () => {
	stores.setCurrenFullscreen(true);
};

const onToolClick = (fnName: string) => {
	switch (fnName) {
		case 'help':
			onToolHelp();
			break;
		case 'download':
			onToolDownload();
			break;
		case 'submit':
			void onToolSubmit();
			break;
		case 'copy':
			onToolCopy();
			break;
		case 'del':
			onToolDel();
			break;
		case 'fullscreen':
			onToolFullscreen();
			break;
		case 'return':
			onReturnToList();
			break;
	}
};

const panelClose = (payload: any) => {
	if (!payload) return;

	switch (payload.nodetype) {
		case 'script':
		case 'executor':
			setNodeContent(payload);
			break;
		case 'basic':
			break;
		default:
			setLineLabel(payload);
			break;
	}
};

const initData = async () => {
	if (!props.ruleId) return;

	state.flowid = props.ruleId;
	state.ruleName = '';
	state.jsPlumb?.reset?.();
	await initLeftNavList();
	await nextTick();
	initSortable();
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
	state.jsPlumb?.reset?.();
});
</script>

<style scoped lang="scss">
.rule-designer {
	display: flex;
	flex-direction: column;
	gap: 18px;
}

.designer-workspace {
	padding: 20px 22px;
	border-radius: 28px;
	border: 1px solid rgba(226, 232, 240, 0.92);
	background: linear-gradient(180deg, rgba(248, 251, 255, 0.96), rgba(255, 255, 255, 0.98));
	box-shadow: 0 18px 42px rgba(15, 23, 42, 0.05);
}

.designer-toolbar {
	padding: 4px 0 16px;
}

.designer-body {
	display: grid;
	grid-template-columns: 300px minmax(0, 1fr);
	gap: 18px;
}

.designer-panel {
	min-width: 0;
	padding: 18px;
	border-radius: 24px;
	border: 1px solid rgba(226, 232, 240, 0.92);
	background:
		radial-gradient(circle at top right, rgba(59, 130, 246, 0.08), transparent 32%),
		linear-gradient(180deg, rgba(255, 255, 255, 0.98), rgba(248, 250, 252, 0.96));
	box-shadow: 0 18px 42px rgba(15, 23, 42, 0.04);
}

.designer-panel__head {
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

.workflow-left {
	height: 720px;
	border-radius: 20px;
	background: rgba(255, 255, 255, 0.72);
}

.workflow-left-id + .workflow-left-id {
	margin-top: 12px;
}

.workflow-left-title {
	display: flex;
	align-items: center;
	justify-content: space-between;
	gap: 12px;
	padding: 14px 16px;
	border-radius: 18px;
	background: rgba(241, 245, 249, 0.82);
	color: #123b6d;
	cursor: pointer;

	strong {
		display: block;
		font-size: 14px;
	}

	small {
		display: block;
		margin-top: 4px;
		color: #7c8da1;
		font-size: 11px;
	}
}

.workflow-left-item {
	margin-top: 10px;
	cursor: move;
}

.workflow-left-item-icon {
	display: flex;
	align-items: center;
	gap: 12px;
	padding: 12px 14px;
	border-radius: 18px;
	border: 1px solid rgba(191, 219, 254, 0.72);
	background: rgba(255, 255, 255, 0.88);
	transition: transform 0.18s ease, box-shadow 0.18s ease, border-color 0.18s ease;
}

.workflow-left-item:hover .workflow-left-item-icon {
	transform: translateY(-1px);
	border-color: rgba(96, 165, 250, 0.85);
	box-shadow: 0 12px 24px rgba(59, 130, 246, 0.12);
}

.workflow-left-item-copy {
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
	cursor: move;
	transition: transform 0.18s ease, box-shadow 0.18s ease, border-color 0.18s ease;
}

.workflow-right-box:hover {
	transform: translateY(-1px);
	border-color: rgba(59, 130, 246, 0.95);
	box-shadow: 0 16px 34px rgba(59, 130, 246, 0.14);
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
	position: relative;
	flex-shrink: 0;
}

.workflow-icon-drag::after {
	content: ' ';
	position: absolute;
	inset: 0;
	z-index: 1;
	cursor: default;
	background: transparent;
}

@media (max-width: 1080px) {
	.designer-body {
		grid-template-columns: 1fr;
	}

	.workflow-left,
	.workflow-right {
		height: 560px;
	}

	.workflow-stage {
		min-height: 560px;
	}
}

@media (max-width: 767px) {
	.designer-workspace {
		padding: 18px;
		border-radius: 22px;
	}

	.designer-panel {
		padding: 16px;
		border-radius: 20px;
	}
}
</style>

<style lang="scss">
.jtk-connector.active {
	z-index: 9999;

	path {
		stroke: #2563eb;
		stroke-width: 1.5;
		animation: ring 3s linear infinite;
		stroke-dasharray: 5;
	}
}

@keyframes ring {
	from {
		stroke-dashoffset: 50;
	}

	to {
		stroke-dashoffset: 0;
	}
}
</style>
