<template>
  <div class="x6-graph-wrap">
    <div ref="container" class="x6-graph"></div>
  </div>
</template>

<script lang="ts">
  //安装完X6但是报缺少MenuItem的错，是没有安装 @antv/x6-react-components，手动执行 yarn add @antv/x6-react-components，在angular这个又不是必要的，很有趣
  import { Graph, Edge, Shape, NodeView } from '@antv/x6';
  import { defineComponent, ref } from 'vue'; // reactive
  const magnetAvailabilityHighlighter = {
    name: 'stroke',
    args: {
      attrs: {
        fill: '#fff',
        stroke: '#47C769',
      },
    },
  };
  export default defineComponent({
    name: 'Index',
    setup() {
      const canUndo = ref(true);
      const canRedo = ref(false);

      const graph = ref(Graph);
      const init: (any) => void = (that) => {
        that.graph = new Graph({
          grid: true,
          container: that.$refs.container as HTMLDivElement,
          width: 800,
          height: 600,
          highlighting: {
            magnetAvailable: magnetAvailabilityHighlighter,
            magnetAdsorbed: {
              name: 'stroke',
              args: {
                attrs: {
                  fill: '#fff',
                  stroke: '#31d0c6',
                },
              },
            },
          },
          connecting: {
            snap: true,
            allowBlank: false,
            allowLoop: false,
            highlight: true,
            connector: 'rounded',
            connectionPoint: 'boundary',
            router: {
              name: 'er',
              args: {
                direction: 'V',
              },
            },
            createEdge() {
              return new Shape.Edge({
                attrs: {
                  line: {
                    stroke: '#a0a0a0',
                    strokeWidth: 1,
                    targetMarker: {
                      name: 'classic',
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

              if (targetMagnet.getAttribute('port-group') !== 'in') {
                return false;
              }

              if (targetView) {
                const node = targetView.cell;
                if (node instanceof Device) {
                  const portId = targetMagnet.getAttribute('port');
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

        that.graph.addNode(
          new Device({
            attrs: {
              root: {
                magnet: false,
              },
              body: {
                fill: '#f5f5f5',
                stroke: '#d9d9d9',
                strokeWidth: 1,
              },
            },
            ports: {
              items: [
                {
                  group: 'out',
                },
              ],
              groups: {
                in: {
                  position: {
                    name: 'top',
                  },
                  attrs: {
                    portBody: {
                      magnet: 'passive',
                      r: 6,
                      stroke: '#ffa940',
                      fill: '#fff',
                      strokeWidth: 2,
                    },
                  },
                },
                out: {
                  position: {
                    name: 'bottom',
                  },
                  attrs: {
                    portBody: {
                      magnet: true,
                      r: 6,
                      fill: '#fff',
                      stroke: '#3199FF',
                      strokeWidth: 2,
                    },
                  },
                },
              },
            },
            portMarkup: [
              {
                tagName: 'circle',
                selector: 'portBody',
              },
            ],
          })
            .resize(120, 40)
            .position(200, 50)
            .updateInPorts(that.graph)
        );

        that.graph.addNode(new Device().resize(120, 40).position(0, 0).updateInPorts(that.graph));

        that.graph.addNode(
          new GateWay().resize(120, 40).position(300, 250).updateInPorts(that.graph)
        );
        that.graph.addNode(
          new Device().resize(120, 40).position(300, 50).updateInPorts(that.graph)
        );
        that.graph.addNode(
          new Device().resize(120, 40).position(400, 50).updateInPorts(that.graph)
        );

        //  this.graph.fromJSON(this.data);

        that.graph.on('edge:connected', ({ previousView, currentView }) => {
          if (previousView) {
            update(previousView as NodeView);
          }
          if (currentView) {
            update(currentView as NodeView);
          }
          console.log(previousView);
        });

        that.graph.on('edge:removed', ({ edge, options }) => {
          if (!options.ui) {
            return;
          }

          const target = edge.getTargetCell();
          if (target instanceof Device) {
            target.updateInPorts(that.graph);
          }
          console.log(target);
        });

        that.graph.on('edge:mouseenter', ({ edge }) => {
          edge.addTools([
            'source-arrowhead',
            'target-arrowhead',
            {
              name: 'button-remove',
              args: {
                distance: -30,
              },
            },
          ]);
          console.log(edge);
        });

        that.graph.on('edge:mouseleave', ({ edge }) => {
          edge.removeTools();
        });
      };

      const update: (view: NodeView) => void = (view: NodeView) => {
        const cell = view.cell;
        if (cell instanceof Device) {
          cell.getInPorts().forEach((port) => {
            const portNode = view.findPortElem(port.id!, 'portBody');
            view.unhighlight(portNode, {
              highlighter: magnetAvailabilityHighlighter,
            });
          });

          cell.updateInPorts(graph);
        }
      };

      return {
        canUndo,
        canRedo,
        history,
        init,
        update,
        graph,
      };
    },
    mounted() {
      /* eslint-disable */
      const that = this as any;
      this.$nextTick(function () {
        that.init(that);
      });
    },
  });

  class Device extends Shape.Circle {
    getInPorts() {
      return this.getPortsByGroup('in');
    }

    getOutPorts() {
      return this.getPortsByGroup('out');
    }

    getUsedInPorts(graph: Graph) {
      const incomingEdges = graph.getIncomingEdges(this) || [];
      return incomingEdges.map((edge: Edge) => {
        const portId = edge.getTargetPortId();
        return this.getPort(portId!);
      });
    }

    getNewInPorts(length: number) {
      return Array.from(
        {
          length,
        },
        () => {
          return {
            group: 'in',
          };
        }
      );
    }

    updateInPorts(graph: Graph) {
      const minNumberOfPorts = 1;
      const ports = this.getInPorts();
      const usedPorts = this.getUsedInPorts(graph);
      const newPorts = this.getNewInPorts(Math.max(minNumberOfPorts - usedPorts.length, 1));

      if (ports.length === minNumberOfPorts && ports.length - usedPorts.length > 0) {
        // noop
      } else if (ports.length === usedPorts.length) {
        this.addPorts(newPorts);
      } else if (ports.length + 1 > usedPorts.length) {
        this.prop(['ports', 'items'], this.getOutPorts().concat(usedPorts).concat(newPorts), {
          rewrite: true,
        });
      }

      return this;
    }
  }

  //只是demo，实际上是这个JSON结构应该从后端传过来，然后在Device的构造方法中传入，参看左上角第一个设备
  Device.config({
    attrs: {
      root: {
        magnet: false,
      },
      body: {
        fill: '#f5f5f5',
        stroke: '#d9d9d9',
        strokeWidth: 1,
      },
    },
    ports: {
      items: [
        {
          group: 'out',
        },
      ],
      groups: {
        in: {
          position: {
            name: 'top',
          },
          attrs: {
            portBody: {
              magnet: 'passive',
              r: 6,
              stroke: '#ffa940',
              fill: '#fff',
              strokeWidth: 2,
            },
          },
        },
        out: {
          position: {
            name: 'bottom',
          },
          attrs: {
            portBody: {
              magnet: true,
              r: 6,
              fill: '#fff',
              stroke: '#3199FF',
              strokeWidth: 2,
            },
          },
        },
      },
    },
    portMarkup: [
      {
        tagName: 'circle',
        selector: 'portBody',
      },
    ],
  });

  class GateWay extends Shape.Rect {
    getInPorts() {
      return this.getPortsByGroup('in');
    }

    getOutPorts() {
      return this.getPortsByGroup('out');
    }

    getUsedInPorts(graph: Graph) {
      const incomingEdges = graph.getIncomingEdges(this) || [];
      return incomingEdges.map((edge: Edge) => {
        const portId = edge.getTargetPortId();
        return this.getPort(portId!);
      });
    }

    getNewInPorts(length: number) {
      return Array.from(
        {
          length,
        },
        () => {
          return {
            group: 'in',
          };
        }
      );
    }

    updateInPorts(graph: Graph) {
      const minNumberOfPorts = 8;
      const ports = this.getInPorts();
      const usedPorts = this.getUsedInPorts(graph);
      const newPorts = this.getNewInPorts(Math.max(minNumberOfPorts - usedPorts.length, 1));

      if (ports.length === minNumberOfPorts && ports.length - usedPorts.length > 0) {
        // noop
      } else if (ports.length === usedPorts.length) {
        this.addPorts(newPorts);
      } else if (ports.length + 1 > usedPorts.length) {
        this.prop(['ports', 'items'], this.getOutPorts().concat(usedPorts).concat(newPorts), {
          rewrite: true,
        });
      }

      return this;
    }
  }
  GateWay.config({
    attrs: {
      root: {
        magnet: false,
      },
      body: {
        fill: '#ffa940',
        stroke: '#d9d9d9',
        strokeWidth: 1,
      },
    },
    ports: {
      items: [
        {
          group: 'out',
        },
      ],
      groups: {
        in: {
          position: {
            name: 'top',
          },
          attrs: {
            portBody: {
              magnet: 'passive',
              r: 6,
              stroke: '#ffa940',
              fill: '#fff',
              strokeWidth: 2,
            },
          },
        },
        out: {
          position: {
            name: 'bottom',
          },
          attrs: {
            portBody: {
              magnet: true,
              r: 6,
              fill: '#fff',
              stroke: '#3199FF',
              strokeWidth: 2,
            },
          },
        },
      },
    },
    portMarkup: [
      {
        tagName: 'circle',
        selector: 'portBody',
      },
    ],
  });
</script>

<style scoped></style>
