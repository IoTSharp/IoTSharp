<template>
  <div class="workflow-drawer-node">
    <el-tabs type="border-card" v-model="state.tabsActive">
      <!-- 扩展表单 -->
      <el-tab-pane label="脚本" name="1">
        <el-scrollbar>
          <div :id="codeContainer" class="the-code-editor-container">
            <!-- <div>
              <i
                v-if="IsMax"
                class="el-icon-rank"
                title="点击缩小"
                @click="minEditor"
              ></i>
              <i
                v-else
                class="el-icon-full-screen"
                title="点击放大"
                @click="maxEditor"
              ></i>
            </div> -->

            <monaco height="100%" width="100%" theme="vs-dark" v-model="state.node.content" :language="state.language"
              selectOnLineNumbers="true"></monaco>
          </div>
        </el-scrollbar>
      </el-tab-pane>
      <!-- 节点编辑 -->
      <el-tab-pane label="节点编辑" name="2">
        <el-scrollbar>
          <el-form :model="state.node" :rules="state.nodeRules" ref="nodeFormRef" size="default" label-width="80px"
            class="pt15 pr15 pb15 pl15">
            <!-- <el-form-item label="数据id" prop="id">
              <el-input v-model="state.node.id" placeholder="请输入数据id" clearable disabled></el-input>
            </el-form-item>
            <el-form-item label="节点id" prop="nodeId">
              <el-input v-model="state.node.nodeId" placeholder="请输入节点id" clearable disabled></el-input>
            </el-form-item>
            <el-form-item label="类型" prop="type">
              <el-input v-model="state.node.type" placeholder="请输入类型" clearable disabled></el-input>
            </el-form-item>
            <el-form-item label="left坐标" prop="left">
              <el-input v-model="state.node.left" placeholder="请输入left坐标" clearable disabled></el-input>
            </el-form-item>
            <el-form-item label="top坐标" prop="top">
              <el-input v-model="state.node.top" placeholder="请输入top坐标" clearable disabled></el-input>
            </el-form-item>
            <el-form-item label="icon图标" prop="icon">
              <el-input v-model="state.node.icon" placeholder="请输入icon图标" clearable></el-input>
            </el-form-item> -->
            <el-form-item label="名称" prop="name">
              <el-input v-model="state.node.name" placeholder="请输入名称" clearable></el-input>
            </el-form-item>
            <!-- <el-form-item>
              <el-button class="mb15" @click="onNodeRefresh">
                <SvgIcon name="ele-RefreshRight" />
                重置
              </el-button>
              <el-button type="primary" class="mb15" @click="onexecutorSubmit">
                <SvgIcon name="ele-Check" />
                保存
              </el-button>
            </el-form-item> -->
          </el-form>
        </el-scrollbar>
      </el-tab-pane>
    </el-tabs>
  </div>
</template>

<script lang="ts" setup>
import {
  defineComponent,
  reactive,
  toRefs,
  ref,
  getCurrentInstance,
} from "vue";
import { ElMessage } from "element-plus";

// 定义接口来定义对象的类型
interface WorkflowDrawerNodeState {
  node: { [key: string]: any };
  language: string;
  nodeRules: any;
  form: any;
  tabsActive: string;
  loading: {
    extend: boolean;
  };
}
const props = defineProps({
  modelValue: {
    type: Object,
    default: {},
  },
});

const emit = defineEmits(["close", "submit"]);
const { proxy } = <any>getCurrentInstance();
const nodeFormRef = ref();
const extendFormRef = ref();
const chartsMonitorRef = ref();
const codeContainer = ref();

const state = reactive<WorkflowDrawerNodeState>({
  node: props.modelValue,
  language: "json",
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
  state.language = state.node.mate;
};
// 节点编辑-重置
const onNodeRefresh = () => {
  state.node.icon = "";
  state.node.name = "";
};
// 节点编辑-保存
const onexecutorSubmit = () => {
  nodeFormRef.value.validate((valid: boolean) => {
    if (valid) {
      emit("submit", state.node);
      emit("close");
    } else {
      return false;
    }
  });
};


watch(
  () => props.modelValue,
  () => {
    state.node = props.modelValue
  }
);

// 扩展表单-重置
const onExtendRefresh = () => {
  extendFormRef.value.resetFields();
};
// 扩展表单-保存
const onExtendSubmit = () => {
  extendFormRef.value.validate((valid: boolean) => {
    if (valid) {
      state.loading.extend = true;
      setTimeout(() => {
        state.loading.extend = false;
        ElMessage.success("保存成功");
        emit("close");
      }, 1000);
    } else {
      return false;
    }
  });
};
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

.the-code-editor-container {
  width: 100%;
  height: 100%;
  position: relative;

  [class^="el-icon"] {
    font-size: 35px;
    cursor: pointer;
    position: absolute;
    right: 10px;
    top: 0;
    z-index: 9999;
    color: #fff;
  }

  .my-editor {
    width: 100%;
    height: 100%;
    overflow: auto;
  }

  .monaco-editor .scroll-decoration {
    box-shadow: none;
  }
}
</style>
