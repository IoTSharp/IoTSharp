<template>
  <div class="workflow-drawer-node">
    <el-tabs type="border-card" v-model="tabsActive">
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

            <div id="codeEditBox" style="height: 300px; padding: 10px"></div>
          </div>
        </el-scrollbar>
      </el-tab-pane>
      <!-- 节点编辑 -->
      <el-tab-pane label="节点编辑" name="2">
        <el-scrollbar>
          <el-form
            :model="node"
            :rules="nodeRules"
            ref="nodeFormRef"
            size="default"
            label-width="80px"
            class="pt15 pr15 pb15 pl15"
          >
            <el-form-item label="数据id" prop="id">
              <el-input
                v-model="node.id"
                placeholder="请输入数据id"
                clearable
                disabled
              ></el-input>
            </el-form-item>
            <el-form-item label="节点id" prop="nodeId">
              <el-input
                v-model="node.nodeId"
                placeholder="请输入节点id"
                clearable
                disabled
              ></el-input>
            </el-form-item>
            <el-form-item label="类型" prop="type">
              <el-input
                v-model="node.type"
                placeholder="请输入类型"
                clearable
                disabled
              ></el-input>
            </el-form-item>
            <el-form-item label="left坐标" prop="left">
              <el-input
                v-model="node.left"
                placeholder="请输入left坐标"
                clearable
                disabled
              ></el-input>
            </el-form-item>
            <el-form-item label="top坐标" prop="top">
              <el-input
                v-model="node.top"
                placeholder="请输入top坐标"
                clearable
                disabled
              ></el-input>
            </el-form-item>
            <el-form-item label="icon图标" prop="icon">
              <el-input
                v-model="node.icon"
                placeholder="请输入icon图标"
                clearable
              ></el-input>
            </el-form-item>
            <el-form-item label="名称" prop="name">
              <el-input v-model="node.name" placeholder="请输入名称" clearable></el-input>
            </el-form-item>
            <el-form-item>
              <el-button class="mb15" @click="onNodeRefresh">
                <SvgIcon name="ele-RefreshRight" />
                重置
              </el-button>
              <el-button type="primary" class="mb15" @click="onexecutorSubmit">
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

<script lang="ts">
import {
  defineComponent,
  reactive,
  toRefs,
  ref,
  nextTick,
  getCurrentInstance,
} from "vue";
import { ElMessage } from "element-plus";
import jsonWorker from "monaco-editor/esm/vs/language/json/json.worker?worker";
import cssWorker from "monaco-editor/esm/vs/language/css/css.worker?worker";
import htmlWorker from "monaco-editor/esm/vs/language/html/html.worker?worker";
import tsWorker from "monaco-editor/esm/vs/language/typescript/ts.worker?worker";
import EditorWorker from "monaco-editor/esm/vs/editor/editor.worker?worker";
import * as monaco from "monaco-editor";
import { stat } from "fs";

// 定义接口来定义对象的类型
interface WorkflowDrawerNodeState {
  node: { [key: string]: any };
  nodeRules: any;
  form: any;
  tabsActive: string;
  loading: {
    extend: boolean;
  };
}

export default defineComponent({
  name: "scriptpanel",
  setup(props, { emit }) {
    self.MonacoEnvironment = {
      getWorker(_: string, label: string) {
        // if (label === "json") {
        //   return new jsonWorker();
        // }
        if (label === "json") {
          return new jsonWorker();
        }
        if (label === "css" || label === "scss" || label === "less") {
          return new cssWorker();
        }
        if (label === "html" || label === "handlebars" || label === "razor") {
          return new htmlWorker();
        }
        if (["typescript", "javascript"].includes(label)) {
          return new tsWorker();
        }
        return new EditorWorker();
      },
    };

    const text = ref("");
    const language = ref("");
    const msg = ref();
    const loading = ref(false);
    let editor: monaco.editor.IStandaloneCodeEditor;
    var IsMax = ref(false);
    const { proxy } = <any>getCurrentInstance();
    const nodeFormRef = ref();
    const extendFormRef = ref();
    const chartsMonitorRef = ref();
    const codeContainer = ref();

    const state = reactive<WorkflowDrawerNodeState>({
      node: {},
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
      editorInit(state.node.mate);
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
    const editorInit = (scripttype: string) => {
      nextTick(() => {
        monaco.languages.typescript.javascriptDefaults.setDiagnosticsOptions({
          noSemanticValidation: true,
          noSyntaxValidation: false,
        });
        monaco.languages.typescript.javascriptDefaults.setCompilerOptions({
          target: monaco.languages.typescript.ScriptTarget.ES2016,
          allowNonTsExtensions: true,
        });

        !editor
          ? (editor = monaco.editor.create(
              document.getElementById("codeEditBox") as HTMLElement,
              {
                value: state.node.content ?? "", // 编辑器初始显示文字
                language: scripttype, // 语言支持自行查阅demo
                automaticLayout: true, // 自适应布局
                theme: "vs-dark", // 官方自带三种主题vs, hc-black, or vs-dark
                foldingStrategy: "indentation",
                renderLineHighlight: "all", // 行亮
                selectOnLineNumbers: true, // 显示行号
                minimap: {
                  enabled: false,
                },
                readOnly: false, // 只读
                fontSize: 16, // 字体大小
                scrollBeyondLastLine: false, // 取消代码后面一大段空白
                overviewRulerBorder: false, // 不要滚动条的边框
              }
            ))
          : editor.setValue(state.node.content ?? "");

        // 监听值的变化
        editor.onDidChangeModelContent((val: any) => {
          state.node.content = editor.getValue();
        });
      });
    };

    const maxEditor = () => {
      document.getElementById("codeEditBox") as HTMLElement;
      editor.layout({
        height: document.body.clientHeight,
        width: document.body.clientWidth,
      });
    };
    const minEditor = () => {
      document.getElementById("codeEditBox") as HTMLElement;
    };
    return {
      codeContainer,
      IsMax,
      maxEditor,
      minEditor,
      nodeFormRef,
      extendFormRef,
      chartsMonitorRef,
      getParentData,
      onNodeRefresh,
      onexecutorSubmit,
      onExtendRefresh,
      onExtendSubmit,
      ...toRefs(state),
    };
  },
});
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
