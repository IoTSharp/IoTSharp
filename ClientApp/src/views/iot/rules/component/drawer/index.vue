<template>
	<div>
		<el-drawer :title="`${nodeData.type === 'line' ? '线' : '节点'}操作`" v-model="isOpen" size="320px">
			<el-scrollbar>
				<Line v-if="nodeData.type === 'line'" @change="onLineChange" @close="close" ref="lineRef" />
				<ExecutorPanel v-if="nodeData.type === 'script'" @submit="onexecutorSubmit" @close="close"
					ref="executorRef" />
				<ScriptPanel v-if="nodeData.type === 'executor'" @submit="onscriptSubmit" @close="close"
					ref="scriptRef" />


			</el-scrollbar>
		</el-drawer>
	</div>
</template>

<script lang="ts">
import { defineComponent, reactive, toRefs, ref, nextTick } from 'vue';
import Line from './line.vue';
import ExecutorPanel from './executor.vue';
import ScriptPanel from './script.vue';
// 定义接口来定义对象的类型
interface WorkflowDrawerState {
	isOpen: boolean;
	nodeData: {
		type: string;
	};
	jsplumbConn: any;
}

export default defineComponent({
	name: 'pagesWorkflowDrawer',
	components: { Line, Node },
	setup(props, { emit }) {
		const lineRef = ref();
		const executorRef = ref();
		const scriptRef = ref();
		const state = reactive<WorkflowDrawerState>({
			isOpen: false,
			nodeData: {
				type: 'node',
			},
			jsplumbConn: {},
		});
		// 打开抽屉
		const open = (item: any, conn: any) => {
			state.isOpen = true;
			state.jsplumbConn = conn;
			state.nodeData = item;


			nextTick(() => {
				if (item.type === 'line') {
					lineRef.value.getParentData(item);
				} else {

					switch (item.nodetype) {

						case 'executor':
							{ executorRef.value.getParentData(item); }
							break;

						case 'script':
							{ scriptRef.value.getParentData(item); }
							break;
					}

				}



			});
		};
		// 关闭
		const close = () => {
			state.isOpen = false;
		};
		// 线 label 内容改变时
		const onLineChange = (label: any) => {
			state.jsplumbConn.label = label;
			emit('label', state.jsplumbConn);
		};
		// 节点内容改变时
		const onNodeSubmit = (data: object) => {
			emit('node', data);
		};

		const onexecutorSubmit = (data: object) => {
			emit('node', data);
		};

		const onscriptSubmit = (data: object) => {
			emit('node', data);
		};
		return {
			lineRef,
			executorRef, scriptRef,
			open,
			close,
			onLineChange,
			onNodeSubmit, onexecutorSubmit, onscriptSubmit,
			...toRefs(state),
		};
	},
});
</script>
