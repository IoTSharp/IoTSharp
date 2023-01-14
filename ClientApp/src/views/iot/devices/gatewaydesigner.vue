<template>
  <div class="workflow-container">
    <div class="workflow-mask" v-if="state.isShow"></div>
    <div class="layout-view-bg-white flex" :style="{ height: `calc(100vh - ${setViewHeight}` }">
      <div class="workflow">



        <!-- 左侧导航区 -->
        <div class="workflow-content">
          <div class="workflow-left">
            <el-scrollbar view-style="padding: 10px">
              <div ref="leftNavRefs" v-for="val in state.leftNavList" :key="val.id"
                :style="{ height: val.isOpen ? 'auto' : '50px', overflow: 'hidden' }" class="workflow-left-id">
                <div class="workflow-left-title" @click="onTitleClick(val)">
                  <span>{{ val.title }}</span>
                  <SvgIcon :name="val.isOpen ? 'ele-ArrowDown' : 'ele-ArrowRight'" />
                </div>
                <div class="workflow-left-item" v-for="(v, k) in val.children" :key="k" :data-color="val.color"
                  :data-name="v.name" :data-icon="v.icon" :data-id="v.id">
                  <div class="workflow-left-item-icon" :style="{ backgroundColor: val.color }">
                    <component :is="v.icon" class="workflow-icon-drag"></component>
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


    <menunode :dropdown="state.dropdownNode" ref="contextmenunodeRef" @ongatewayclickcommand="ongatewayclickcommand">
    </menunode>

    <menuconnector :dropdown="state.dropdownNode" ref="contextmenugatewayRef"
      @onconnectoropencommand="onconnectoropencommand">
    </menuconnector>
    <drawercontainer ref="drawerRef" @submit="onsubmit" />
  </div>
</template>
<script lang="ts" setup>
import { GatewayDesignerState } from "./designer/models/gatewaydesignermodel";
import { useTagsViewRoutes } from "/@/stores/tagsViewRoutes";
import { useThemeConfig } from "/@/stores/themeConfig";
import { Graph, Edge, Shape, NodeView } from "@antv/x6";
import { Device } from "./designer/shapes/device";
import menunode from "./designer/contextmenu/menunode.vue";
import menuconnector from "./designer/contextmenu/menuconnector.vue";
import { GateWay } from "./designer/shapes/gateway";
import Sortable from "sortablejs";
import { modbusprofile } from "./designer/pannels/modbus/modbusprofile";
import { datatypes } from "./designer/models/constants";

import drawercontainer from "./designer/pannels/drawercontainer.vue"
import { opcuaprofile } from "./designer/pannels/opcua/opcuaprofile";

const contextmenunodeRef = ref();
const stores = useTagsViewRoutes();
const storesThemeConfig = useThemeConfig();
const { themeConfig } = storeToRefs(storesThemeConfig);
const { isTagsViewCurrenFull } = storeToRefs(stores);
const state = reactive<GatewayDesignerState>({
  dropdownNode: {
    x: '', y: ''

  },
  isShow: false,
  leftNavList: [],
});
const drawerRef = ref();
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

const onsubmit = (param: any) => {
  var { namespace, data } = param;


  switch (namespace) {
    case "modbuslistchanged":
      // sender.node.bizdata.mappings=[...data]
      for (var item of data.node.bizdata.mappings) {
        data.node.addPort({
          group: 'in',
          id: item._id,
          attrs: {
            body: {
              r: 10,
              magnet: 'true',
              stroke: "#31d0c6",
              fill: "#fff",
              strokeWidth: 2,
            },
          }
        });
      }
      break;

    case "opcualistchanged":
      for (var item of data.node.bizdata.mappings) {
        data.node.addPort({
          group: 'in',
          id: item._id,
          attrs: {
            body: {
              r: 10,
              magnet: 'true',
              stroke: "#31d0c6",
              fill: "#fff",
              strokeWidth: 2,
            },
            text: {
              text: item.dataName
            }
          }
        });
      }
      break;


  }

}


const onconnectoropencommand = (args: any) => {
  var { command, sender } = args;

}

const ongatewayclickcommand = (args: any) => {
  var { command, sender } = args;
  switch (command) {
    case "editmodbusmapping":
      drawerRef.value.open(sender)
      break;

    case "editopcuamapping":
      drawerRef.value.open(sender)
      break;

  }

}
onMounted(async () => {
  state.leftNavList = [
    {
      title: "网关",
      name: '',
      // icon: "iconfont icon-shouye",
      color: "#F1F0FF",
      isOpen: true,
      id: "t1",
      children: [],
    },
    {
      title: "设备",
      name: '',
      //  icon: "iconfont icon-shouye",
      color: "#F1F0FF",
      isOpen: true,
      id: "t2",
      children: [],
    },
  ];

  nextTick(() => {
    initgraph();
  });
  await initgateways();
  await initdevices();
  window.addEventListener("resize", setClientWidth);
  initLeftNavbar();

  await initSortable();
});

const initSortable = () => {

  leftNavRefs.value.forEach((v) => {
    Sortable.create(v as HTMLDivElement, {
      group: {
        name: "vue-next-admin-1",
        pull: "clone",
        put: false,
      },
      animation: 0,
      sort: false,
      draggable: ".workflow-left-item",
      forceFallback: true,
      onEnd: (evt: any) => {
        const { name, icon, id, color } = evt.clone.dataset;
        const { nodetype, nodenamespace, mata } = evt.clone.attributes;
        const { layerX, layerY, clientX, clientY } = evt.originalEvent;
        const el = workflowRightRef.value!;
        const { x, y, width, height } = el.getBoundingClientRect();
        if (clientX < x || clientX > width + x || clientY < y || y > y + height) {
          ElMessage.warning("请把节点拖入到画布中");
        } else {
          var target = [];
          findtoolitem(id, state.leftNavList, target);
          var item = target[0]
          if (item) {

            switch (item['profile']['shape']) {

              case 'gateway':
                var gateway = new GateWay({
                  label: item['profile']['name'],
                  bizdata: item['profile'],
                  attrs: {
                    root: {
                      magnet: false,
                    },
                    body: {
                      stroke: '#237804',
                      fill: '#73d13d',
                      rx: 10,
                      ry: 10,
                    },
                  },
                  ports: {
                    items: [

                    ],
                    groups: {
                      in: {
                        label: {
                          position: 'left'
                        },
                        position: {
                          name: 'left'
                        },
                        attrs: {
                          portBody: {
                            magnet: "true",
                            r: 10,
                            stroke: "#ffa940",
                            fill: "#fff",
                            strokeWidth: 2,
                          },
                        },
                        magnet: 'true',
                      },
                      out: {
                        position: {
                          name: "bottom",
                        },
                        attrs: {
                          portBody: {
                            magnet: "true",
                            r: 10,
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
                  .resize(160, 160)
                  .position(layerX - 40, layerY - 15)

                gateway.bizdata = item['profile']
                graph.addNode(
                  gateway
                );

                break;
              case 'device':
                var gateway = new Device({
                  label: item['profile']['name'],
                  bizdata: item['profile'],
                  attrs: {
                    root: {
                      magnet: false,
                    },
                    body: {
                      stroke: '#ffa940',
                      fill: '#ffd591',
                      rx: 10,
                      ry: 10,
                    },
                  },
                  ports: {
                    items: [

                    ],
                    groups: {
                      in: {
                        label: {
                          position: 'left'
                        },
                        position: {
                          name: 'left'
                        },
                        attrs: {
                          portBody: {
                            magnet: "passive",
                            r: 10,
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
                            r: 10,
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
                  .resize(100, 80)
                  .position(layerX - 40, layerY - 15)

                gateway.bizdata = item['profile']
                graph.addNode(
                  gateway
                );
                break;
            }
          }
        }
      },
    });
  });
};




const setClientWidth = () => {
  const clientWidth = document.body.clientWidth;
  clientWidth < 768 ? (state.isShow = true) : (state.isShow = false);
};

const initgateways = () => {
  state.leftNavList[0].children = [
    {
      title: "Modbus",
      name: 'Modbus',
      // icon: "iconfont icon-shouye",
      color: "#F1F0FF",
      isOpen: true,
      id: "1111111",
      profile: {
        devnamespace: 'modbus',
        name: 'ModBus网关',
        shape: 'gateway',
        baseinfoschema: {},
        command: {

          toolbar: [],
          contextmenu: [
            { command: 'deletegateway', txt: "删除", icon: "ele-Delete" },
            { command: 'editmodbusprop', txt: "属性编辑", icon: "ele-Edit" },
            { command: 'editmodbusmapping', txt: "映射编辑", icon: "ele-Edit" },
          ],
        },
        mappings: []
      }

    }, {
      title: "Modbus",
      name: 'Modbus',
      // icon: "iconfont icon-shouye",
      color: "#F1F0FF",
      isOpen: true,
      id: "12222222",
      profile: {
        devnamespace: 'modbus',
        name: 'ModBus网关',
        shape: 'gateway',
        baseinfoschema: {},
        command: {

          toolbar: [],
          contextmenu: [
            { command: 'deletegateway', txt: "删除", icon: "ele-Delete" },
            { command: 'editmodbusprop', txt: "属性编辑", icon: "ele-Edit" },
            { command: 'editmodbusmapping', txt: "映射编辑", icon: "ele-Edit" },
          ],
        },
        mappings: []
      }

    }, {
      title: "Modbus",
      name: 'Modbus',
      // icon: "iconfont icon-shouye",
      color: "#F1F0FF",
      isOpen: true,
      id: "13333333",
      profile: {
        devnamespace: 'modbus',
        name: 'ModBus网关',
        shape: 'gateway',
        baseinfoschema: {},
        command: {

          toolbar: [],
          contextmenu: [
            { command: 'deletegateway', txt: "删除", icon: "ele-Delete" },
            { command: 'editmodbusprop', txt: "属性编辑", icon: "ele-Edit" },
            { command: 'editmodbusmapping', txt: "映射编辑", icon: "ele-Edit" },
          ],
        },
        mappings: []
      }

    }, {
      title: "OPCUA",
      name: 'OPCUA',
      // icon: "iconfont icon-shouye",
      color: "#F1F0FF",
      isOpen: true,
      id: "2222222",
      profile: {
        devnamespace: 'opcua',
        name: 'opcua网关',
        shape: 'gateway',
        baseinfoschema: {},
        command: {

          toolbar: [],
          contextmenu: [
            { command: 'deletegateway', txt: "删除", icon: "ele-Delete" },
            { command: 'editopcuaprop', txt: "编辑属性", icon: "ele-Edit" },
            { command: 'editopcuamapping', txt: "编辑映射", icon: "ele-Edit" },
          ]
        },
        mappings: []
      }
    },
  ];
};
const initdevices = () => {


  state.leftNavList[1].children = [
    {
      icon: "importIcon",
      name: "测试设备",
      id: "33333333",

      profile: {
        devnamespace: 'iot.device.test',
        shape: 'device',
        name: '测试设备',
        baseinfoschema: {},
        command: {

          toolbar: [],
          contextmenu: [
            { command: 'deletegateway', txt: "删除", icon: "ele-Delete" },
            { command: 'editmodbusmapping', txt: "编辑属性", icon: "ele-Edit" },
          ]
        },
        mappings: []
      }

    }, {
      icon: "importIcon",
      name: "测试设备1",

      id: "444444444", profile: {
        devnamespace: 'iot.device.test1',
        name: '测试设备',
        shape: 'device',
        baseinfoschema: {},
        command: {

          toolbar: [],
          contextmenu: [
            { command: 'deletegateway', txt: "删除", icon: "ele-Delete" },
            { command: 'editmodbusmapping', txt: "编辑属性", icon: "ele-Edit" },
          ]
        },
        mappings: []
      }
    }, {
      icon: "importIcon",
      name: "测试设备2",

      id: "55555555555", profile: {
        devnamespace: 'iot.device.test2',
        name: '测试设备',
        shape: 'device',
        baseinfoschema: {},
        command: {
          toolbar: [],
          contextmenu: [
            { command: 'deletegateway', txt: "删除", icon: "ele-Delete" },
            { command: 'editmodbusmapping', txt: "编辑属性", icon: "ele-Edit" },
          ]
        },
        mappings: []
      }
    }, {
      icon: "importIcon",
      name: "测试设备3",

      id: "666666666666", profile: {
        devnamespace: 'iot.device.test3',
        name: '测试设备',
        shape: 'device',
        baseinfoschema: {},
        command: {

          toolbar: [],
          contextmenu: [
            { command: 'deletegateway', txt: "删除", icon: "ele-Delete" },
            { command: 'editmodbusmapping', txt: "编辑属性", icon: "ele-Edit" },
          ]
        },
        mappings: []
      }
    },
  ];
};
const initLeftNavbar = () => {
  leftNavRefs.value.forEach((v) => {

  })
};
const initgraph: () => void = () => {
  graph = new Graph({

    container: workflowRightRef.value as HTMLDivElement,
    width: 800,
    height: 1024,

    grid: {
      size: 10,
      visible: true,
      type: 'doubleMesh',

      args: [
        {
          color: '#eee', // 主网格线颜色
          thickness: 1,     // 主网格线宽度
        },
        {
          color: '#ddd', // 次网格线颜色
          thickness: 1,     // 次网格线宽度
          factor: 4,        // 主次网格线间隔
        },
      ],
    },
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
          bizdata: {
            devnamespace: 'connector',
            shape: 'connector',
            name: '',
            baseinfoschema: {},
            command: {
              toolbar: [],
              contextmenu: [
                { command: 'deleteconnector', txt: "删除", icon: "ele-Delete" },
                { command: 'editconnector', txt: "编辑", icon: "ele-Edit" },
              ]
            },
            incomepoint: '',
            outgoingpoint: '',
            incomeshape: '',
            outgoingshape: '',

          },
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


  //update bizdata right here
  graph.on("edge:connected", ({ isNew, edge }) => {
    //create mapping
    if (isNew) {


    } else {
      //modify mapping


    }
    edge.store.data.bizdata.incomepoint = edge.store.data.target.port
    edge.store.data.bizdata.outgoingpoint = edge.store.data.source.port
    edge.store.data.bizdata.incomeshape = edge.store.data.target.cell
    edge.store.data.bizdata.outgoingshape = edge.store.data.source.cell
  });
  graph.on("edge:contextmenu", (e) => {
    var { edge } = e;
    contextmenunodeRef.value.openContextmenu(edge);
  });

  graph.on("edge:click", (e) => {
    // can't get point info
  });


  graph.on("node:contextmenu", (sender: any) => {
    state.dropdownNode.y = sender.e.pageY;
    state.dropdownNode.x = sender.e.pageX;

    console.log(sender)
    contextmenunodeRef.value.openContextmenu(sender);
  });


  graph.on("edge:connected", ({ previousView, currentView }: any) => {
    if (previousView) {
      update(previousView as NodeView);
    }
    if (currentView) {
      update(currentView as NodeView);
    }

  });

  graph.on("edge:removed", ({ edge, options }: any) => {
    if (!options.ui) {
      return;
    }

    const target = edge.getTargetCell();
    if (target instanceof Device) {
      target.updateInPorts(graph);
    }

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


const findtoolitem = (id: string, leftNavs: any[], target: any[]) => {
  for (var item of leftNavs) {
    if (item.id === id) {
      target.push(item);
    } else {
      if (item.children && item.children.length > 0) {
        findtoolitem(id, item.children, target)
      }
    }
  }
}

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
        background-image: linear-gradient(90deg,
            rgb(156 214 255 / 15%) 10%,
            rgba(0, 0, 0, 0) 10%),
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
