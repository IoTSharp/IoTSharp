<template>
  <div class="workflow-container">
    <div class="layout-view-bg-white flex" :style="{ height: `calc(100vh - 400px` }">
      <div class="workflow">
        <!-- 顶部工具栏 -->
        <Tool @tool="onToolClick" />
        <!-- 左侧导航区 -->
        <div class="workflow-content">
          <!-- 右侧绘画区 -->
          <div class="workflow-right" ref="workflowRightRef">
            <div v-for="(v, k) in state.jsplumbData.nodeList" :key="v.nodeId" :id="v.nodeId" :data-node-id="v.nodeId"
              :class="v.nodeclass" :style="{ left: v.left, top: v.top }" @click="onItemCloneClick(k)">
              <div :style="{ backgroundColor: v.color }" class="workflow-right-box"
                :class="{ 'workflow-right-active': state.jsPlumbNodeIndex === k }">
                  <div class="workflow-left-item-icon">
                      <SvgIcon :name="v.icon" class="workflow-icon-drag" />
                      <div class="font10 pl5 name">{{ v.name }}</div>
                  </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <div class="workflow-bottom">
      <el-tabs type="card" class="tabs" v-model="activeName">
        <el-tab-pane label="timeline" name="timeline"> <el-timeline>
            <el-timeline-item v-for="(activity, index) in state.activities" :key="index" :type="activity.type"
              :timestamp="activity.timestamp">
              <el-card>
                <h4> {{ activity.content }}</h4>
                <p> {{ activity.data }}</p>
              </el-card>
            </el-timeline-item>
          </el-timeline></el-tab-pane>

      </el-tabs>
    </div>

    <el-dialog v-model="state.dataFormVisible" title="测试数据">
      <div>
        <monaco height="100%" width="100%" theme="vs-dark" v-model="state.content" language="json"
          selectOnLineNumbers="true"></monaco>
      </div>

      <template #footer>
        <span class="dialog-footer">
          <el-button @click="state.dataFormVisible = false">取消</el-button>
          <el-button type="primary" @click="submitData"> 确认 </el-button>
        </span>
      </template>
    </el-dialog>
  </div>
</template>

<script lang="ts" setup>
import {
  reactive,
  computed,
  onMounted,
  onUnmounted,
  nextTick,
  ref,
  getCurrentInstance,
} from "vue";
import { ElMessage, ElMessageBox } from "element-plus";
import { jsPlumb } from "jsplumb";
import Sortable from "sortablejs";
import { storeToRefs } from "pinia";
import { useThemeConfig } from "/@/stores/themeConfig";
import { useTagsViewRoutes } from "/@/stores/tagsViewRoutes";
import Tool from "./component/tool/simulator.vue";
import monaco from "/@/components/monaco/monaco.vue";
import commonFunction from "/@/utils/commonFunction";

import {
  jsplumbDefaults,
  jsplumbMakeSource,
  jsplumbMakeTarget,
  jsplumbConnect,
} from "./js/config";
import { useRouter, useRoute } from "vue-router";
import { ruleApi } from "/@/api/flows";

import { LineListState, NodeListState } from "./models";

// 定义接口来定义对象的类型

interface XyState {
  x: string | number;
  y: string | number;
}
interface FlowState {
  activities: any[];
  index: number;
  flowid?: string | any;
  content?: string | any;
  workflowRightRef: HTMLDivElement | null;
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
const emit = defineEmits(["close", "submit"]);
const props = defineProps({
  ruleId: {
    type: String,
    default: ''
  }
})

const activeName = ref('timeline')
const route = useRoute();
const router = useRouter();
const language = ref("json");

const contextmenuNodeRef = ref();
const contextmenuLineRef = ref();
const drawerRef = ref();
const helpRef = ref();
const stores = useTagsViewRoutes();
const storesThemeConfig = useThemeConfig();
const { themeConfig } = storeToRefs(storesThemeConfig);
const { isTagsViewCurrenFull } = storeToRefs(stores);
const { copyText } = commonFunction();

const state = reactive<FlowState>({
  activities: [],
  flowid: props.ruleId,
  workflowRightRef: null as HTMLDivElement | null,
  dropdownNode: { x: "", y: "" },
  dropdownLine: { x: "", y: "" },
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

  index: 0,
});
// 设置 view 的高度
const setViewHeight = computed(() => {
  let { isTagsview } = themeConfig.value;
  if (isTagsViewCurrenFull.value) {
    return `30px`;
  } else {
    if (isTagsview) return `114px`;
    else return `80px`;
  }
});
// 设置 宽度小于 768
const setClientWidth = () => {
  const clientWidth = document.body.clientWidth;
  clientWidth < 768 ? (state.isShow = true) : (state.isShow = false);
};
// 右侧导航-数据初始化
const initRightNodeList = async () => {
  await ruleApi()
    .getDiagram(state.flowid)
    .then((res) => {
      state.jsplumbData = {
        nodeList: res.data.nodes,
        lineList: res.data.lines,
      };
    });
    window.setInterval(() => {
     var index = state.index % state.jsplumbData.nodeList.length;
     state.jsplumbData.nodeList[index].class = "workflow-right-highlight";
     state.index++;
   }, 1000);
};
// 初始化 jsPlumb
const initJsPlumb = () => {
  (<any>jsPlumb).ready(() => {
    state.jsPlumb = (<any>jsPlumb).getInstance({
      detachable: false,
      Container: "workflow-right",
    });
    state.jsPlumb.fire("jsPlumbDemoLoaded", state.jsPlumb);
    // 导入默认配置
    state.jsPlumb.importDefaults(state.jsplumbDefaults);
    // 会使整个jsPlumb立即重绘。
    state.jsPlumb.setSuspendDrawing(false, true);
    // 初始化节点、线的链接
    initJsPlumbConnection();
  });
};
// 初始化节点、线的链接
const initJsPlumbConnection = () => {
  state.jsplumbData.nodeList.forEach((v) => {
    // 整个节点作为source或者target
    state.jsPlumb.makeSource(v.nodeId, state.jsplumbMakeSource);
    // 整个节点作为source或者target
    state.jsPlumb.makeTarget(v.nodeId, state.jsplumbMakeTarget, jsplumbConnect);
  });

  // 线
  state.jsplumbData.lineList.forEach((v) => {
    state.jsPlumb.connect(
      {
        source: v.sourceId,
        target: v.targetId,
        label: v.linename,
        linename: v.linename,
        condition: v.condition,
      },
      state.jsplumbConnect
    );

  });
  // 节点
};
const onexecutorSubmit = (data: object) => { };

const onscriptSubmit = (data: any) => { };

// 右侧内容区-当前项点击
const onItemCloneClick = (k: number) => {
  state.jsPlumbNodeIndex = k;
};

// 右侧内容区-当前项右键菜单点击回调(线)
const onCurrentLineClick = (item: any, conn: any) => {
  const { contextMenuClickId } = item;
  const { endpoints } = conn;
  const intercourse: any = [];
  endpoints.forEach((v: any) => {
    intercourse.push({
      id: v.element.id,
      innerText: v.element.innerText,
    });
  });
  item.contact = `${intercourse[0].innerText}(${intercourse[0].id}) => ${intercourse[1].innerText}(${intercourse[1].id})`;
  if (contextMenuClickId === 0) state.jsPlumb.deleteConnection(conn);
  else if (contextMenuClickId === 1) {
    drawerRef.value.open(item, conn);
  }
};
// 设置线的 label
const setLineLabel = (obj: any) => {
  const { sourceId, targetId, label, linename, condition, namespace } = obj;
  const conn = state.jsPlumb.getConnections({
    source: sourceId,
    target: targetId,
  })[0];
  conn.setLabel(linename);
  if (!linename || linename === "") {
    conn.addClass("workflow-right-empty-label");
  } else {
    conn.removeClass("workflow-right-empty-label");
    conn.addClass("workflow-right-label");
  }
  state.jsplumbData.lineList.forEach((v) => {
    if (v.sourceId === sourceId && v.targetId === targetId) {
      v.label = label;
      v.linename = linename;
      v.condition = condition;
    }
  });
};
// 设置节点内容
const setNodeContent = (obj: any) => {
  const { nodeId, name, icon } = obj;

  // 设置节点 name 与 icon
  state.jsplumbData.nodeList.forEach((v) => {
    if (v.nodeId === nodeId) {
      v.name = name;
      v.icon = icon;
    }
  });
  // 重绘
  nextTick(() => {
    state.jsPlumb.setSuspendDrawing(false, true);
  });
};
// 顶部工具栏-当前项点击
const onToolClick = (fnName: String) => {
  switch (fnName) {
    case "help":
      onToolHelp();
      break;
    case "download":
      onToolDownload();
      break;
    case "submit":
      onToolSubmit();
      break;
    case "fullscreen":
      onToolFullscreen();
      break;
    case "return":
      onReturnToList();
      break;
  }
};

const onReturnToList = () => {
  emit("close", state.jsplumbData);
  // router.push({
  //   path: "/iot/rules/flowlist",
  // });
};

// 顶部工具栏-帮助
const onToolHelp = () => {
  nextTick(() => {
    helpRef.value.open();
  });
};
// 顶部工具栏-下载
const onToolDownload = () => {
  const { globalTitle } = themeConfig.value;
  const href =
    "data:text/json;charset=utf-8," +
    encodeURIComponent(JSON.stringify(state.jsplumbData, null, "\t"));
  const aLink = document.createElement("a");
  aLink.setAttribute("href", href);
  aLink.setAttribute("download", `${globalTitle}设计.json`);
  aLink.click();
  aLink.remove();
  ElMessage.success("下载成功");
};

const panelclose = (data: any) => {
  switch (data.nodetype) {
    case "script":
      state.jsplumbData.nodeList.forEach((v) => {
        if (v.nodeId === data.nodeId) {
          v.name = data.name;
          v.icon = data.icon;
          v.nodetype = data.nodetype;
          v.nodenamespace = data.namespace;
          v.mata = data.mata;
          v.content = data.content;
        }
      });

      break;
    case "executor":
      state.jsplumbData.nodeList.forEach((v) => {
        if (v.nodeId === data.nodeId) {
          v.name = data.name;
          v.icon = data.icon;
          v.nodetype = data.nodetype;
          v.nodenamespace = data.namespace;
          v.mata = data.mata;
          v.content = data.content;
        }
      });
      break;
    case "basic":
      break;
  }
};

// 顶部工具栏-提交
const onToolSubmit = () => {
  openRunDialog();
};

const setclass = (item: any) => {
  for (var _item of item.nodes) {
    var node = state.jsplumbData.nodeList.find((c) => c.nodeId == _item.bpmnid);
    if (node) {
      state.activities = [...state.activities, { timestamp: _item.addDate, content: _item.operationDesc, data: _item.data }]
      node.nodeclass = "workflow-right-highlight";
    }
  }
};

const clearclass = () => {
  for (var node of state.jsplumbData.nodeList) {
    //  state.activities = []
      node.nodeclass = "workflow-right-clone";
  }
}

const submitData = (node: any) => {
  state.dataFormVisible = false;
  var data = JSON.parse(state.content);
  if (data) {
    ruleApi()
      .active({
        form: data,
        extradata: { ruleflowid: state.flowid },
      })
      .then((res) => {
        state.activities=[];
        for (let index = 0; index < res.data.length; index++) {
          var item = res.data[index];
          setTimeout(setclass, index * 500, item);
        }

       setTimeout(clearclass, res.data.length * 1000);

      });
    ElMessage.success("数据提交成功");
  }
};

const openRunDialog = () => {
  state.dataFormVisible = true;
  state.content = "";
};

// 顶部工具栏-全屏
const onToolFullscreen = () => {
  stores.setCurrenFullscreen(true);
};
watch(() => props.ruleId, async () => {

  if (props.ruleId && props.ruleId !== "") {

    await initRightNodeList();
    initJsPlumb();
    setClientWidth();
  }

})
// 页面加载时
onMounted(async () => {
  if (props.ruleId && props.ruleId !== "") {
      await initRightNodeList();
    initJsPlumb();
    setClientWidth();
  }
  window.addEventListener("resize", setClientWidth);
});
// 页面卸载时
onUnmounted(() => {
  window.removeEventListener("resize", setClientWidth);
});
</script>

<style scoped lang="scss">
  

.workflow-container {
  position: relative;
    
    .workflow-bottom {
        padding-top:5px;
        min-height:286px;
        background-color:#f7fbff;
        height: 100%;
        border-right: 1px solid var(--el-border-color-light, #ebeef5);
          ::v-deep .el-timeline-item__node {
        background-color: #9dbdfd !important;
        left:0px;
    }
    ::v-deep .el-timeline-item__tail {
        border-left: 2px solid #e3ebf9 !important;
        left: 5px;
    }
    ::v-deep .el-timeline-item__wrapper{
        padding-right:15px;
    }
    ::v-deep .el-tab-pane .el-timeline li:first{}
    }

  .workflow {
    display: flex;
    height: 100%;
    width: 100%;
    flex-direction: column;

    .workflow-content {
      display: flex;
      height: calc(100% - 35px);

      .workflow-right {
        flex: 1;
        position: relative;
        overflow: hidden;
        height: 100%;
        background-image: linear-gradient(90deg,
            rgb(156 214 255 / 15%) 10%,
            rgba(0, 0, 0, 0) 10%),
          linear-gradient(rgb(156 214 255 / 15%) 10%, rgba(0, 0, 0, 0) 10%);
        background-size: 10px 10px;

        .workflow-right-clone {
          position: absolute;

          .workflow-right-box {
            height: 50px;
            align-items: center;
            color: black;
            padding: 0 10px;
            border-radius: 3px;
            cursor: move;
            transition: all 0.1s ease;
            min-width: 94.5px;
            background: var(--el-color-white);
            border: 1px solid var(--el-border-color-light, #ebeef5);

            .workflow-left-item-icon {
              display: flex;
              align-items: center;
              height: 50px;
            }

            &:hover {
              outline: 2px dashed var(--el-color-primary);
              background: var(--el-color-primary-light-9);
              //transition: all 0.3s ease;
              color: var(--el-color-primary);

              i {
                cursor: Crosshair;
              }
            }
          }

          .workflow-right-active {
            //border: 1px dashed var(--el-color-primary);
            outline: 2px solid var(--el-color-primary);
            background: var(--el-color-primary-light-9);
            color: var(--el-color-primary);
          }
        }

        .workflow-right-highlight {
          position: absolute;

          .workflow-right-box {
            height: 50px;
            align-items: center;
            color: var(--el-color-white);
            padding: 0 10px;
            border-radius: 3px;
            cursor: move;
            transition: all 0.3s ease;
            min-width: 94.5px;
            background: #755eea;
            border: 1px solid var(--el-border-color-light, #c95eea);

            .workflow-left-item-icon {
              display: flex;
              align-items: center;
              height: 50px;
            }

            &:hover {
              border: 1px dashed var(--el-color-primary);
              background: var(--el-color-primary-light-9);
              transition: all 0.3s ease;
              color: var(--el-color-primary);

              i {
                cursor: Crosshair;
              }
            }
          }

          .workflow-right-active {
            border: 1px dashed var(--el-color-primary);
            background: var(--el-color-primary-light-9);
            color: var(--el-color-primary);
          }
        }

        :deep(.jtk-overlay):not(.aLabel) {
          padding: 4px 10px;
          border: 1px solid var(--el-border-color-light, #ebeef5) !important;
          color: var(--el-text-color-secondary) !important;
          background: var(--el-color-white) !important;
          border-radius: 3px;
          font-size: 10px;
        }

        :deep(.jtk-overlay.workflow-right-empty-label) {
          display: none;
        }
      }
    }
  }

  .workflow-mask {
    position: absolute;
    top: 0;
    right: 0;
    bottom: 0;
    left: 0;

    &::after {
      content: "手机版不支持 jsPlumb 操作";
      position: absolute;
      top: 0;
      right: 0;
      bottom: 0;
      left: 0;
      z-index: 1;
      background: rgba(255, 255, 255, 0.9);
      color: #666666;
      display: flex;
      align-items: center;
      justify-content: center;
    }
  }
}

.workflow-icon-drag {
  position: relative;

  &:after {
    content: " ";
    width: 32px;
    height: 32px;
    left: 0;
    top: 0;
    z-index: 1000;
    position: absolute;
    cursor: default;
    background: transparent;
  }
}

.jtk-connector.active {
  z-index: 9999;

  path {
    stroke: #150042;
    stroke-width: 1.5;
    animation: ring;
    animation-duration: 3s;
    animation-timing-function: linear;
    animation-iteration-count: infinite;
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
