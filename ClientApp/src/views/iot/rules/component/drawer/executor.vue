<template>
  <div class="workflow-drawer-node">
    <el-tabs type="border-card" v-model="state.tabsActive">
      <!-- 扩展表单 -->
      <el-tab-pane label="配置" name="1">
        <el-scrollbar>
          <monaco
            :height="state.configheight"
            :width="state.configwidth"
            theme="vs-dark"
            v-model="state.node.content"
            language="json"
            selectOnLineNumbers="true"
          ></monaco>
        </el-scrollbar>
      </el-tab-pane>
      <!-- 节点编辑 -->
      <el-tab-pane label="杂项" name="2">
        <el-scrollbar>
          <el-form
            :model="node"
            :rules="state.nodeRules"
            ref="nodeFormRef"
            size="default"
            label-width="80px"
            class="pt15 pr15 pb15 pl15"
          >
            <el-form-item label="数据id" prop="id">
              <el-input
                v-model="state.node.id"
                placeholder="请输入数据id"
                clearable
                disabled
              ></el-input>
            </el-form-item>
            <el-form-item label="节点id" prop="nodeId">
              <el-input
                v-model="state.node.nodeId"
                placeholder="请输入节点id"
                clearable
                disabled
              ></el-input>
            </el-form-item>
            <el-form-item label="类型" prop="type">
              <el-input
                v-model="state.node.type"
                placeholder="请输入类型"
                clearable
                disabled
              ></el-input>
            </el-form-item>
            <el-form-item label="left坐标" prop="left">
              <el-input
                v-model="state.node.left"
                placeholder="请输入left坐标"
                clearable
                disabled
              ></el-input>
            </el-form-item>
            <el-form-item label="top坐标" prop="top">
              <el-input
                v-model="state.node.top"
                placeholder="请输入top坐标"
                clearable
                disabled
              ></el-input>
            </el-form-item>
            <el-form-item label="icon图标" prop="icon">
              <el-input
                v-model="state.node.icon"
                placeholder="请输入icon图标"
                clearable
              ></el-input>
            </el-form-item>
            <el-form-item label="名称" prop="name">
              <el-input
                v-model="state.node.name"
                placeholder="请输入名称"
                clearable
              ></el-input>
            </el-form-item>
            <el-form-item>
              <el-button class="mb15" @click="onNodeRefresh">
                <SvgIcon name="ele-RefreshRight" />
                重置
              </el-button>
              <el-button type="primary" class="mb15" @click="onNodeSubmit">
                <SvgIcon name="ele-Check" />
                保存
              </el-button>
            </el-form-item>
          </el-form>
        </el-scrollbar>
      </el-tab-pane>
    </el-tabs>
  </div>
</template>

<script lang="ts" setup>
import { reactive, toRefs, ref, nextTick, getCurrentInstance, Ref } from "vue";
import {
  ElButton,
  ElForm,
  ElFormItem,
  ElInput,
  ElMessage,
  ElScrollbar,
  ElTabPane,
  ElTabs,
} from "element-plus";
import monaco from "/@/components/monaco/monaco.vue";
import node from "element-plus/es/components/cascader-panel/src/node";
// 定义接口来定义对象的类型
interface WorkflowDrawerNodeState {
  configwidth: string;
  configheight: string;
  node: nodedata;
  nodeRules: any;
  form: any;
  tabsActive: string;
  loading: {
    extend: boolean;
  };
}

interface nodedata {
  id?: string;
  contextMenuClickId?: number;
  content?: string;
  from?: string;
  icon?: string;
  label?: string;
  left?: string;
  mata?: string;
  name?: string;
  nodeId?: string;
  nodeclass?: string;
  nodenamespace?: string;
  nodetype?: string;
  result?: string;
  top?: string;
  type?: string;
}

const props = defineProps({
  modelValue: {
    type: String,
    default: "",
  },
});

const emit = defineEmits(["close", "submit"]);




const { proxy } = <any>getCurrentInstance();
const nodeFormRef = ref();
const extendFormRef = ref();
const chartsMonitorRef = ref();
const state = reactive<WorkflowDrawerNodeState>({
  configwidth: "100%",
  configheight: "300px",
  node: {
    content: props.modelValue,
    contextMenuClickId: 0,
    from: "",
    icon: "",
    label: "",
    left: "",
    mata: "",
    nodeId: "",
    nodeclass: "",
    nodenamespace: "",
    nodetype: "",
    result: "",
    top: "",
    type: "",
  },
  nodeRules: {
    id: [{ required: true, message: "请输入数据id", trigger: "blur" }],
    nodeId: [{ required: true, message: "请输入节点id", trigger: "blur" }],
    type: [{ required: true, message: "请输入类型", trigger: "blur" }],
    left: [{ required: true, message: "请输入left坐标", trigger: "blur" }],
    top: [{ required: true, message: "请输入top坐标", trigger: "blur" }],
    icon: [{ required: true, message: "请输入icon图标", trigger: "blur" }],
    name: [{ required: true, message: "请输入名称", trigger: "blur" }],
  },
  form: {
    module: [],
  },
  tabsActive: "1",
  loading: {
    extend: false,
  },
});
// 获取父组件数据
const getParentData = (data: object) => {
  state.tabsActive = "1";
  state.node = data;
};

onMounted(() => {});
// 节点编辑-重置
const onNodeRefresh = () => {
  state.node.icon = "";
  state.node.name = "";
};
// 节点编辑-保存
const onNodeSubmit = () => {
  nodeFormRef.value.validate((valid: boolean) => {
    if (valid) {
      emit("submit", state.node);
      emit("close");
    } else {
      return false;
    }
  });
};


defineExpose({
  getParentData,
});
// 图表可视化-初始化
</script>

<style scoped lang="scss">
.workflow-drawer-node {
  :deep {
    .el-tabs {
      box-shadow: unset;
      border: unset;

      .el-tabs__nav {
        display: flex;
        width: 100%;

        .el-tabs__item {
          flex: 1;
          padding: unset;
          text-align: center;

          &:first-of-type.is-active {
            border-left-color: transparent;
          }

          &:last-of-type.is-active {
            border-right-color: transparent;
          }
        }
      }

      .el-tabs__content {
        padding: 0;
        height: calc(100vh - 90px);

        .el-tab-pane {
          height: 100%;
        }
      }
    }
  }
}
</style>
