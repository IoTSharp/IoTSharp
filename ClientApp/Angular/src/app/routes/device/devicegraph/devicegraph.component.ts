import { Component, ElementRef, Input, OnInit, Type, ViewChild } from '@angular/core';
import { Graph, Edge, Shape, NodeView, Cell, Color } from '@antv/x6';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-devicegraph',
  templateUrl: './devicegraph.component.html',
  styleUrls: ['./devicegraph.component.less'],
})
export class DevicegraphComponent implements OnInit {
  subscription: Subscription;
  droppedData: string;
  @Input()
  id: Number = -1;
  @ViewChild('container', { static: true })
  private container!: ElementRef;
  //左侧未在设计上的设备和网关的列表数据，自己有时间搞Shape,可以用官方的dnd(https://x6.antv.vision/zh/docs/tutorial/basic/dnd)，不然就手动吧
  data = [
    {
      devicename: '设备1',
      id: '11',
      type: 'device',
      logo: 'control',
      image: './assets/logo.png',
      remark: '这是一个设备，拖动它放到设计器上',
    },
    {
      devicename: '设备2',
      id: '22',
      type: 'device',
      logo: 'control',
      image: './assets/logo.png',
      remark: '这是一个设备，拖动它放到设计器上',
    },
    {
      devicename: '设备3',
      id: '33',
      type: 'device',
      logo: 'control',
      image: './assets/logo.png',
      remark: '这是一个设备，拖动它放到设计器上',
    },
    {
      devicename: '网关1',
      id: '44',
      type: 'gateway',
      logo: 'ungroup',
      image: './assets/logo.png',
      remark: '这是一个网关，拖动它放到设计器上',
    },
    {
      devicename: '网关2',
      id: '55',
      type: 'gateway',
      logo: 'ungroup',
      image: './assets/logo.png',
      remark: '这是一个网关，拖动它放到设计器上',
    },
    {
      devicename: '设备4',
      id: '66',
      type: 'device',
      logo: 'control',
      image: './assets/logo.png',
      remark: '这是一个设备，拖动它放到设计器上',
    },
    {
      devicename: '设备5',
      id: '77',
      type: 'device',
      logo: 'control',
      image: './assets/logo.png',
      remark: '这是一个设备，拖动它放到设计器上',
    },
    {
      devicename: '设备6',
      id: '88',
      type: 'device',
      logo: 'control',
      image: './assets/logo.png',
      remark: '这是一个设备，拖动它放到设计器上',
    },
    {
      devicename: '设备7',
      id: '99',
      type: 'device',
      logo: 'control',
      image: './assets/logo.png',
      remark: '这是一个设备，拖动它放到设计器上',
    },
  ];
  graph!: Graph;
  magnetAvailabilityHighlighter = {
    name: 'stroke',
    args: {
      attrs: {
        fill: '#fff',
        stroke: '#47C769',
      },
    },
  };
  constructor() {}

  ngOnInit(): void {
    this.graph = new Graph({
      autoResize: true,
      grid: true,
      container: this.container.nativeElement,
      height: 800,
      highlighting: {
        magnetAvailable: this.magnetAvailabilityHighlighter,
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
      onPortRendered: (args) => {},
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
            direction: 'H',
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

    this.graph.on('edge:connected', ({ previousView, currentView }) => {
      if (previousView) {
        this.update(previousView as NodeView);
      }
      if (currentView) {
        this.update(currentView as NodeView);
      }
    });
    this.graph.on('blank:mousemove', ({ e }) => {
      this.dragEndlocation = e;
    });
    this.graph.on('edge:removed', ({ edge, options }) => {
      if (!options.ui) {
        return;
      }

      const target = edge.getTargetCell();
      if (target instanceof Device) {
        target.updateInPorts(this.graph);
      }
      console.log(target);
    });

    this.graph.on('edge:mouseenter', ({ edge }) => {
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
    });

    this.graph.on('cell:mousedown', ({ cell }) => {
      //  cell.removeTools(); //只读状态下移除Node中的操作按钮
    });

    this.graph.on('node:click', (e) => {
      // e.node.getProp('Biz'); 获取Node中的原始数据
    });
  }
  update(view: NodeView) {
    const cell = view.cell;
    if (cell instanceof Device) {
      cell.getInPorts().forEach((port) => {
        const portNode = view.findPortElem(port.id!, 'portBody');
        view.unhighlight(portNode, {
          highlighter: this.magnetAvailabilityHighlighter,
        });
      });
      cell.updateInPorts(this.graph);
    }
  }

  dragend($event) {}
  onDrop($event) {
    switch ($event.dropData.type) {
      case 'device':
        var node = this.graph.addNode(
          new Device({ label: $event.dropData.devicename })
            .setProp('Biz', $event.dropData)
            .resize(80, 80)
            .position(this.dragEndlocation.offsetX, this.dragEndlocation.offsetY)
            .updateInPorts(this.graph),
        );
        node.setPortLabelMarkup;

        this.data.splice(this.data.indexOf($event.dropData), 1);
        break;

      case 'gateway':
        var node = this.graph.addNode(
          new GateWay({
            label: $event.dropData.devicename,
            tools: [
              {
                name: 'button',
                args: {
                  markup: [
                    {
                      tagName: 'circle',
                      selector: 'button',
                      attrs: {
                        r: 14,
                        stroke: '#fe854f',
                        strokeWidth: 2,
                        fill: 'white',
                        cursor: 'pointer',
                      },
                    },
                    {
                      tagName: 'text',
                      textContent: '-',
                      selector: 'icon',
                      attrs: {
                        fill: '#fe854f',
                        fontSize: 24,
                        textAnchor: 'middle',
                        pointerEvents: 'none',
                        y: '0.3em',
                      },
                    },
                  ],
                  x: '50%',
                  y: '10%',
                  offset: { x: -0, y: -0 },
                  onClick({ cell }: { cell: Cell }) {
                    this.graph.removeCell(cell);
                    const fill = Color.randomHex();
                    cell.attr({
                      body: {
                        fill,
                      },
                      label: {
                        fill: Color.invert(fill, true),
                      },
                    });
                  },
                },
              },
            ],
          })
            .setProp('Biz', $event.dropData)
            .resize(160, 200)
            .position(this.dragEndlocation.offsetX, this.dragEndlocation.offsetY)
            .updateInPorts(this.graph),
        );
        node.setPortLabelMarkup;

        this.data.splice(this.data.indexOf($event.dropData), 1);
        break;
    }

    var edges = this.graph.getEdges();
    var nodes = this.graph.getNodes();
    var result = [];
    for (var item of nodes) {
      console.log(item.toJSON());
      var port = item.ports.items;
      var incomes = port.filter((x) => x.group === 'in').map((x) => x.id);
      var outgoings = port.filter((x) => x.group === 'out').map((x) => x.id);
      var data = item.getProp('Biz');

      var dev = {
        incomes,
        outgoings,
        id: data.id,
        type: data.type,
      };
      result = [...result, dev];
    }
    console.log(nodes);

    for (var _item of edges) {
      var edge = {
        id: _item.id,
        source: _item.source,
        target: _item.target,
      };
    }
    console.log(edges);
  }
  dragEnd(event) {}

  onmove($event) {
    this.dragEndlocation = $event;
  }

  dragEndlocation: any;

  load() {}

  save() {
    var edges = this.graph.getEdges(); // 所有Edge对象
    var nodes = this.graph.getNodes(); // 所有Shape对象,

    for (var item of nodes) {
      var port = item.ports.items;
      var incomes = port.filter((x) => x.group === 'in'); //Shape对象的输入端口，网关或设备的输入端口

      var outgoings = port.filter((x) => x.group === 'out'); //Shape对象的输出端口，网关或设备的输出端口
    }
  }
}

export interface DeviceInfo {
  Income: string[];
  OutGoing: string[];
  Label: string;
  LocationX: number;
  LocationY: number;
  Width: number;
  Height: number;
  Type: string;
}

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
      },
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
      //  this.addPorts(newPorts);
    } else if (ports.length + 1 > usedPorts.length) {
      this.prop(['ports', 'items'], this.getOutPorts().concat(usedPorts).concat(newPorts), {
        rewrite: true,
      });
    }

    return this;
  }
}
Device.config({
  label: '',
  attrs: {
    root: {
      magnet: false,
    },
    body: {
      fill: '#eeffee',
      stroke: '#d9d9d9',
      strokeWidth: 1,
    },
  },
  ports: {
    items: [
      {
        group: 'out',
        attrs: {
          text: {
            // 标签选择器
            text: 'port1', // 标签文本
          },
        },
      },
    ],
    groups: {
      in: {
        position: {
          name: 'right',
        },
        attrs: {
          portBody: {
            magnet: 'passive',
            r: 6,
            stroke: '#ff0000',
            fill: '#fff',
            strokeWidth: 2,
          },
        },
      },
      out: {
        position: {
          name: 'left',
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
      },
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
        attrs: {
          text: {
            // 标签选择器
            text: 'port1', // 标签文本
          },
        },
      },
    ],
    groups: {
      in: {
        label: {
          position: 'left',
        },
        position: {
          name: 'right',
        },
        attrs: {
          text: {
            text: 'port1',
          },
          portBody: {
            magnet: 'passive',
            r: 6,
            stroke: '#ff0000',
            fill: '#fff',
            strokeWidth: 2,
          },
        },
      },
      out: {
        position: {
          name: 'left',
        },
        label: {
          position: 'right',
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
