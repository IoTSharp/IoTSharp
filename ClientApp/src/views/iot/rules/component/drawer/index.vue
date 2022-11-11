<template>
  <div>
    <el-drawer
      :title="`${nodeData.type === 'line' ? '线' : nodeData.name}操作`"
      v-model="isOpen"
      size="640px"
      @closed="drawerclose"
    >
      <el-scrollbar>
        <Line
          v-if="nodeData.type === 'line'"
          @linechange="onLineChange"
          @close="close"
          ref="lineRef"
        />
        <ExecutorPanel
          v-if="nodeData.nodetype === 'executor' && nodeData.type === 'node'"
          @submit="onexecutorSubmit"
          @close="close"
          ref="executorRef"
        />
        <ScriptPanel
          v-if="nodeData.nodetype === 'script' && nodeData.type === 'node'"
          @submit="onscriptSubmit"
          @close="close"
          ref="scriptRef"
        />
      </el-scrollbar>
    </el-drawer>
  </div>
</template>

<script lang="ts">
import { defineComponent, reactive, toRefs, ref, nextTick } from "vue";
import Line from "./line.vue";
import ExecutorPanel from "./executor.vue";
import ScriptPanel from "./script.vue";
// 定义接口来定义对象的类型
interface WorkflowDrawerState {
  isOpen: boolean;
  nodeData: {
    name: string;
    type: string;
    nodetype: string;
  };
  jsplumbConn: any;
}

export default defineComponent({
  name: "pagesWorkflowDrawer",
  components: { Line, ExecutorPanel, ScriptPanel },
  setup(props, { emit }) {
    const lineRef = ref();
    const executorRef = ref();
    const scriptRef = ref();
    const state = reactive<WorkflowDrawerState>({
      isOpen: false,
      nodeData: {
        type: "node",
        nodetype: "executor",
        name: "",
      },
      jsplumbConn: {},
    });
    // 打开抽屉
    const open = (item: any, conn: any) => {
      state.isOpen = true;
      state.jsplumbConn = conn;
      state.nodeData = item;
      nextTick(() => {
        if (item.type === "line") {
         // 当前line的数据不存储在node当中，而是在conn副本当中

          lineRef.value.getParentData(conn);
        } else {
          switch (item.nodetype) {
            case "executor":
              {
                executorRef.value.getParentData(item);
              }
              break;

            case "script":
              {
                scriptRef.value.getParentData(item);
              }
              break;
          }
        }
      });
    };

    // 关闭
    const drawerclose = () => {
      //分别触发保存,node的label和line的label同时触发change事件会有名称空间冲突导致label值被覆盖，
      if (state.nodeData.type === "line") {
        lineRef.value.onLineTextChange();
      }
      if (state.nodeData.type === "node") {
        emit("panelclose", state.nodeData);
      }

    };

    // 关闭
    const close = () => {
      state.isOpen = false;
    };
    // 线 label 内容改变时
    const onLineChange = (label: any) => {
      state.jsplumbConn.label = label.label;
      state.jsplumbConn.linename =  label.linename;
      state.jsplumbConn.condition =  label.condition;
      emit("label", state.jsplumbConn);
    };
    // 节点内容改变时
    const onNodeSubmit = (data: object) => {
      emit("node", data);
    };

    const onexecutorSubmit = (data: object) => {
      emit("executor", data);
    };

    const onscriptSubmit = (data: object) => {
      emit("script", data);
    };
    return {
      lineRef,
      executorRef,
      scriptRef,
      open,
      close,
      onLineChange,
      drawerclose,
      onNodeSubmit,
      onexecutorSubmit,
      onscriptSubmit,
      ...toRefs(state),
    };
  },
});
</script>
