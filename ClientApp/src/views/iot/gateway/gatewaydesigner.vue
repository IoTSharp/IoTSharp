<template>
  <div class="workflow-container">
    <div class="workflow-mask" v-if="state.isShow"></div>
    <div
      class="layout-view-bg-white flex"
      :style="{ height: `calc(100vh - ${setViewHeight}` }"
    >
      <div class="workflow">



        <!-- 左侧导航区 -->
        <div class="workflow-content">
          <div class="workflow-left">
            <el-scrollbar view-style="padding: 10px">
              <div
                ref="leftNavRefs"
                v-for="val in state.leftNavList"
                :key="val.id"
                :style="{ height: val.isOpen ? 'auto' : '50px', overflow: 'hidden' }"
                class="workflow-left-id"
              >
                <div class="workflow-left-title" @click="onTitleClick(val)">
                  <span>{{ val.title }}</span>
                  <SvgIcon :name="val.isOpen ? 'ele-ArrowDown' : 'ele-ArrowRight'" />
                </div>
                <div
                  class="workflow-left-item"
                  v-for="(v, k) in val.children"
                  :key="k"
                  :data-color="val.color"
                  :data-name="v.name"
                  :data-icon="v.icon"
                  :data-id="v.id"
                >
                <div
                    class="workflow-left-item-icon"
                    :style="{ backgroundColor: val.color }"
                  >
                    <component
                      :is="v.icon"
                      class="workflow-icon-drag"
                    ></component>
                    <div class="text-sm pl5 name">{{ v.name }}</div>
                  </div>
                </div>
              </div>
            </el-scrollbar>
          </div>

          <!-- 右侧绘画区 -->
          <div class="workflow-right" ref="workflowRightRef"></div>
        </div>
      </div>
    </div>
  </div>
</template>
<script lang="ts" setup>
import { GatewayDesignerState } from "./gatewaydesignermodel";
import { useTagsViewRoutes } from "/@/stores/tagsViewRoutes";
import { useThemeConfig } from "/@/stores/themeConfig";
import { Graph, Edge, Shape, NodeView } from "@antv/x6";
import { Device } from "./shapes/device";
import { GateWay } from "./shapes/gateway";

import Sortable from "sortablejs";

const stores = useTagsViewRoutes();
const storesThemeConfig = useThemeConfig();
const { themeConfig } = storeToRefs(storesThemeConfig);
const { isTagsViewCurrenFull } = storeToRefs(stores);
const state = reactive<GatewayDesignerState>({
  isShow: false,
  leftNavList: [],
});

const workflowRightRef = ref<HTMLDivElement | null>(null);
const leftNavRefs = ref([]);
var graph: any;
const magnetAvailabilityHighlighter = {
  name: "stroke",
  args: {
    attrs: {
      fill: "#fff",
      stroke: "#47C769",
    },
  },
};
onMounted(async () => {
  state.leftNavList = [
    {
      title: "网关",
      name:'',
      icon: "iconfont icon-shouye",
      color: "#F1F0FF",
      isOpen: true,
      id: "1",
      children: [],
    },
    {
      title: "设备",     
       name:'',
      icon: "iconfont icon-shouye",
      color: "#F1F0FF",
      isOpen: true,
      id: "1",
      children: [],
    },
  ];

  nextTick(() => {
    initgraph();
  });
  initgateways();
  initdevices();
  window.addEventListener("resize", setClientWidth);
  initLeftNavbar();
});

const setClientWidth = () => {
  const clientWidth = document.body.clientWidth;
  clientWidth < 768 ? (state.isShow = true) : (state.isShow = false);
};

const initgateways = () => {
  state.leftNavList[0].children = [
    {
      title: "网关1",
      name:'网关1',
      icon: "iconfont icon-shouye",
      color: "#F1F0FF",
      isOpen: true,
      id: "1111111",
    },  {
      title: "网关2",
      name:'网关2',
      icon: "iconfont icon-shouye",
      color: "#F1F0FF",
      isOpen: true,
      id: "2222222",
    },
  ];
};
const initdevices = () => {


  state.leftNavList[1].children = [
    {
      icon: "importIcon",
      name: "网关1",
      id: "111",
    },{
      icon: "importIcon",
      name: "网关2",
      id: "222",
    },{
      icon: "importIcon",
      name: "网关3",
      id: "333",
    },{
      icon: "importIcon",
      name: "网关4",
      id: "4444",
    },
  ];
};
const initLeftNavbar = () => {




  leftNavRefs.value.forEach((v)=>{

    console.log(v)
  })





  console.log(leftNavRefs.value)




  // leftNavRefs.value.forEach((v.value) => {
  //   console.log(v)
  //   Sortable.create(v as HTMLDivElement, {
  //     group: {
  //       name: "vue-next-admin-1",
  //       pull: "clone",
  //       put: false,
  //     },
  //     animation: 0,
  //     sort: false,
  //     draggable: ".workflow-left-item",
  //     forceFallback: true,
  //     onEnd: function (evt: any) {

  //       console.log(evt)
  //       const { name, icon, id, color } = evt.clone.dataset;
  //       const { layerX, layerY, clientX, clientY } = evt.originalEvent;
  //       const el = workflowRightRef.value!;
  //       console.log(
  //         `%conEnd@flowdesigner:217`,
  //         "color:black;font-size:16px;background:yellow;font-weight: bold;",
  //         el
  //       );
  //       const { x, y, width, height } = el.getBoundingClientRect();

  //       if (clientX < x || clientX > width + x || clientY < y || y > y + height) {
  //         ElMessage.warning("请把节点拖入到画布中");
  //       } else {
  //         // 节点id（唯一）
  //         const nodeId = Math.random().toString(36).substr(2, 12);
  //         // 处理节点数据
  //         const node = {
  //           nodeId,
  //           color,
  //           left: `${layerX - 40}px`,
  //           top: `${layerY - 15}px`,
  //           nodeclass: "workflow-right-clone",
  //           name,
  //           icon,
  //           id,
  //         };
      
  //         nextTick(() => {

          
  //         });
  //       }
  //     },
  //   });
  // });

};
const initgraph: () => void = () => {
  graph = new Graph({
    grid: true,
    container: workflowRightRef.value as HTMLDivElement,
    width: 800,
    height: 600,
    highlighting: {
      magnetAvailable: magnetAvailabilityHighlighter,
      magnetAdsorbed: {
        name: "stroke",
        args: {
          attrs: {
            fill: "#fff",
            stroke: "#31d0c6",
          },
        },
      },
    },
    connecting: {
      snap: true,
      allowBlank: false,
      allowLoop: false,
      highlight: true,
      connector: "rounded",
      connectionPoint: "boundary",
      router: {
        name: "er",
        args: {
          direction: "V",
        },
      },
      createEdge() {
        return new Shape.Edge({
          attrs: {
            line: {
              stroke: "#a0a0a0",
              strokeWidth: 1,
              targetMarker: {
                name: "classic",
                size: 7,
              },
            },
          },
        });
      },
      validateConnection({ sourceView, targetView, targetMagnet }) {
        if (!targetMagnet) {
          return false;
        }

        if (targetMagnet.getAttribute("port-group") !== "in") {
          return false;
        }

        if (targetView) {
          const node = targetView.cell;
          if (node instanceof Device) {
            const portId = targetMagnet.getAttribute("port");
            const usedInPorts = node.getUsedInPorts(this);
            if (usedInPorts.find((port) => port && port.id === portId)) {
              return false;
            }
          }
        }

        return true;
      },
    },
  });

  graph.addNode(
    new Device({
      attrs: {
        root: {
          magnet: false,
        },
        body: {
          fill: "#f5f5f5",
          stroke: "#d9d9d9",
          strokeWidth: 1,
        },
      },
      ports: {
        items: [
          {
            group: "out",
          },
        ],
        groups: {
          in: {
            position: {
              name: "top",
            },
            attrs: {
              portBody: {
                magnet: "passive",
                r: 6,
                stroke: "#ffa940",
                fill: "#fff",
                strokeWidth: 2,
              },
            },
          },
          out: {
            position: {
              name: "bottom",
            },
            attrs: {
              portBody: {
                magnet: true,
                r: 6,
                fill: "#fff",
                stroke: "#3199FF",
                strokeWidth: 2,
              },
            },
          },
        },
      },
      portMarkup: [
        {
          tagName: "circle",
          selector: "portBody",
        },
      ],
    })
      .resize(40, 123)
      .position(300, 50)
      .updateInPorts(graph)
  );
  graph.addNode(
    new Device({
      attrs: {
        root: {
          magnet: false,
        },
        body: {
          fill: "#f5f5f5",
          stroke: "#d9d9d9",
          strokeWidth: 1,
        },
      },
      ports: {
        items: [
          {
            group: "out",
          },
        ],
        groups: {
          in: {
            position: {
              name: "top",
            },
            attrs: {
              portBody: {
                magnet: "passive",
                r: 6,
                stroke: "#ffa940",
                fill: "#fff",
                strokeWidth: 2,
              },
            },
          },
          out: {
            position: {
              name: "bottom",
            },
            attrs: {
              portBody: {
                magnet: true,
                r: 6,
                fill: "#fff",
                stroke: "#3199FF",
                strokeWidth: 2,
              },
            },
          },
        },
      },
      portMarkup: [
        {
          tagName: "circle",
          selector: "portBody",
        },
      ],
    })
      .resize(120, 40)
      .position(200, 50)
      .updateInPorts(graph)
  );

  graph.on("edge:connected", ({ previousView, currentView }: any) => {
    if (previousView) {
      update(previousView as NodeView);
    }
    if (currentView) {
      update(currentView as NodeView);
    }
    console.log(previousView);
  });

  graph.on("edge:removed", ({ edge, options }: any) => {
    if (!options.ui) {
      return;
    }

    const target = edge.getTargetCell();
    if (target instanceof Device) {
      target.updateInPorts(graph);
    }
    console.log(target);
  });

  graph.on("edge:mouseenter", ({ edge }: any) => {
    edge.addTools([
      "source-arrowhead",
      "target-arrowhead",
      {
        name: "button-remove",
        args: {
          distance: -30,
        },
      },
    ]);
    console.log(edge);
  });

  graph.on("edge:mouseleave", ({ edge }: any) => {
    edge.removeTools();
  });
};

const update: (view: NodeView) => void = (view: NodeView) => {
  const cell = view.cell;
  if (cell instanceof Device) {
    cell.getInPorts().forEach((port) => {
      const portNode = view.findPortElem(port.id!, "portBody");
      view.unhighlight(portNode, {
        highlighter: magnetAvailabilityHighlighter,
      });
    });

    //   cell.updateInPorts(  graph);
  }
};
// 左侧导航-菜单标题点击
const onTitleClick = (val: any) => {
  val.isOpen = !val.isOpen;
};
const setViewHeight = computed(() => {
  let { isTagsview } = themeConfig.value;
  if (isTagsViewCurrenFull.value) {
    return `30px`;
  } else {
    if (isTagsview) return `114px`;
    else return `80px`;
  }
});
onUnmounted(() => {
  window.removeEventListener("resize", setClientWidth);
});
</script>

<style scoped lang="scss">
.workflow-container {
  position: relative;

  .workflow {
    display: flex;
    height: 100%;
    width: 100%;
    flex-direction: column;

    .workflow-content {
      display: flex;
      height: calc(100% - 35px);

      .workflow-left {
        box-sizing: border-box;
        width: 220px;
        height: 100%;
        border-right: 1px solid var(--el-border-color-light, #ebeef5);

        :deep(.el-collapse-item__content) {
          padding-bottom: 0;
        }

        .workflow-left-title {
          height: 50px;
          display: flex;
          align-items: center;
          padding: 0 10px;
          border-top: 1px solid var(--el-border-color-light, #ebeef5);
          color: var(--el-text-color-primary);
          cursor: default;

          span {
            flex: 1;
          }
        }

        .workflow-left-item {
          box-sizing: border-box;
          display: inline-block;
          width: 100%;
          position: relative;
          cursor: move;
          //margin: 0 0 10px 10px;
          //padding-right: 10px;
          //border: 2px solid #9E9DAD;
          border: 2px solid transparent;
          border-radius: 6px;
          margin-bottom: 10px;
          outline: 1px solid #9e9dad;
          outline-offset: -2px;

          &:hover {
            outline: none;
            //transition: all 0.3s ease;
            border: 2px dashed var(--el-color-primary);
            background: var(--el-color-primary-light-9);
            border-radius: 6px;

            i,
            .name {
              transition: all 0.3s ease;
              color: var(--el-color-primary);
            }
          }

          .workflow-left-item-icon {
            height: 35px;
            display: flex;
            align-items: center;
            transition: all 0.3s ease;
            padding: 5px 10px;
            border: 1px dashed transparent;
            background: var(--next-bg-color);
            border-radius: 6px;

            i,
            .name {
              color: black;
              transition: all 0.3s ease;
              white-space: nowrap;
              text-overflow: ellipsis;
              overflow: hidden;
            }
          }
        }

        & .workflow-left-id:first-of-type {
          .workflow-left-title {
            border-top: none;
          }
        }
      }

      .workflow-right {
        flex: 1;
        position: relative;
        overflow: hidden;
        padding: 5px;
        height: 100%;
        background-image: linear-gradient(
            90deg,
            rgb(156 214 255 / 15%) 10%,
            rgba(0, 0, 0, 0) 10%
          ),
          linear-gradient(rgb(156 214 255 / 15%) 10%, rgba(0, 0, 0, 0) 10%);
        background-size: 10px 10px;
      }
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
</style>
