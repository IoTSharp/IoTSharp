import { ChangeDetectorRef, Component, ElementRef, Input, OnInit, Type, ViewChild } from '@angular/core';
import { Graph, Edge, Shape, NodeView, Cell, Color } from '@antv/x6';
import { STColumn, STColumnTag, STComponent, STData } from '@delon/abc/st';
import { SettingsService } from '@delon/theme';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-devicegraph',
  templateUrl: './devicegraph.component.html',
  styleUrls: ['./devicegraph.component.less'],
})
export class DevicegraphComponent implements OnInit {
  selectedstyle = {
    body: {
      stroke: '#00ff00',
      strokeWidth: 5,
    },
  };

  selcetedDevice: any = {
    ports: {
      in: [],
    },
  };

  ports: port[] = [];
  TAG: STColumnTag = {
    1: { text: '以太网', color: 'green' },
    2: { text: 'RS232', color: 'blue' },
    3: { text: 'RS485', color: 'orange' },
  };
  columns: STColumn[] = [
    { title: '名称', index: 'portname', render: 'portNameTpl' },
    { title: 'IO类型', index: 'iotype', render: 'portTypeTpl' },
    {
      title: '类型',
      index: 'type',
      render: 'portPhyTypeTpl',
      type: 'enum',
      enum: { 1: '壹', 2: '贰', 3: '叁' },
    },
    {
      title: '操作',
      buttons: [
        {
          text: `修改`,
          iif: (i) => !i.edit,
          click: (i) => this.updateEdit(i, true),
        },
        {
          text: `保存`,
          iif: (i) => i.edit,
          click: (i) => {
            this.submit(i);
          },
        },
        {
          text: `取消`,
          iif: (i) => i.edit,
          click: (i) => this.updateEdit(i, false),
        },
      ],
    },
  ];
  subscription: Subscription;
  droppedData: string;
  @Input()
  id: Number = -1;
  @ViewChild('container', { static: true })
  private container!: ElementRef;
  @ViewChild('st')
  private st: STComponent;
  //左侧未在设计上的设备和网关的列表数据，自己有时间搞Shape,可以用官方的dnd(https://x6.antv.vision/zh/docs/tutorial/basic/dnd)，不然就手动吧

  data: Array<DeviceItem> = [
    {
      devicename: '设备1',
      id: '11',
      type: 'device',
      logo: 'control',
      image: './assets/logo.png',
      remark: '这是一个设备，拖动它放到设计器上',
      prop: {
        GraphStroke: '#d9d9d9',
        GraphStrokeWidth: 1,
        GraphTextFill: '',
        GraphTextFontSize: '',
        GraphPostionX: '',
        GraphPostionY: '',
        GraphFill: '',
        GraphTextRefX: '',
        GraphHeight: '',
        GraphTextRefY: '',
        GraphTextAnchor: '',
        GraphTextVerticalAnchor: '',
        GraphTextFontFamily: '',
        GraphWidth: '',
        GraphShape: '',
      },
      ports: {
        in: [{ portname: 'port1', id: 1, type: 1, iotype: 1 }],
      },
    },
    {
      devicename: '设备2',
      id: '22',
      type: 'device',
      logo: 'control',
      image: './assets/logo.png',
      remark: '这是一个设备，拖动它放到设计器上',
      prop: {
        GraphStroke: '#d9d9d9',
        GraphStrokeWidth: 1,
        GraphTextFill: '',
        GraphTextFontSize: '',
        GraphPostionX: '',
        GraphPostionY: '',
        GraphFill: '',
        GraphTextRefX: '',
        GraphHeight: '',
        GraphTextRefY: '',
        GraphTextAnchor: '',
        GraphTextVerticalAnchor: '',
        GraphTextFontFamily: '',
        GraphWidth: '',
        GraphShape: '',
      },
      ports: {
        in: [{ portname: 'port1', id: 2, type: 1, iotype: 1 }],
      },
    },
    {
      devicename: '设备3',
      id: '33',
      type: 'device',
      logo: 'control',
      image: './assets/logo.png',
      remark: '这是一个设备，拖动它放到设计器上',
      prop: {
        GraphStroke: '#d9d9d9',
        GraphStrokeWidth: 1,
        GraphTextFill: '',
        GraphTextFontSize: '',
        GraphPostionX: '',
        GraphPostionY: '',
        GraphFill: '',
        GraphTextRefX: '',
        GraphHeight: '',
        GraphTextRefY: '',
        GraphTextAnchor: '',
        GraphTextVerticalAnchor: '',
        GraphTextFontFamily: '',
        GraphWidth: '',
        GraphShape: '',
      },
      ports: {
        in: [{ portname: 'port1', id: 3, type: 1, iotype: 1 }],
      },
    },
    {
      devicename: '网关1',
      id: '44',
      type: 'gateway',
      logo: 'ungroup',
      image: './assets/logo.png',
      remark: '这是一个网关，拖动它放到设计器上',
      prop: {
        GraphStroke: '#d9d9d9',
        GraphStrokeWidth: 1,
        GraphTextFill: '',
        GraphTextFontSize: '',
        GraphPostionX: '',
        GraphPostionY: '',
        GraphFill: '',
        GraphTextRefX: '',
        GraphHeight: '',
        GraphTextRefY: '',
        GraphTextAnchor: '',
        GraphTextVerticalAnchor: '',
        GraphTextFontFamily: '',
        GraphWidth: '',
        GraphShape: '',
      },
      ports: {
        in: [
          { portname: 'port11', id: '4', type: 1, iotype: 1 },
          { portname: 'port2', id: '5', type: 1, iotype: 1 },
          { portname: 'port3', id: '6', type: 1, iotype: 1 },
          { portname: 'port4', id: '7', type: 1, iotype: 1 },
          { portname: 'port5', id: '8', type: 1, iotype: 1 },
          { portname: 'port6', id: '9', type: 1, iotype: 1 },
          { portname: 'port7', id: '10', type: 1, iotype: 1 },
        ],
        out: [
          { portname: 'port11', id: '11', type: 1, iotype: 1 },
          { portname: 'port2', id: '12', type: 1, iotype: 1 },
          { portname: 'port3', id: '13', type: 1, iotype: 1 },
          { portname: 'port4', id: '14', type: 1, iotype: 1 },
          { portname: 'port5', id: '15', type: 1, iotype: 1 },
          { portname: 'port6', id: '16', type: 1, iotype: 1 },
          { portname: 'port7', id: '17', type: 1, iotype: 1 },
        ],
      },
    },
    {
      devicename: '网关2',
      id: '55',
      type: 'gateway',
      logo: 'ungroup',
      image: './assets/logo.png',
      remark: '这是一个网关，拖动它放到设计器上',
      prop: {
        GraphStroke: '#d9d9d9',
        GraphStrokeWidth: 1,
        GraphTextFill: '',
        GraphTextFontSize: '',
        GraphPostionX: '',
        GraphPostionY: '',
        GraphFill: '',
        GraphTextRefX: '',
        GraphHeight: '',
        GraphTextRefY: '',
        GraphTextAnchor: '',
        GraphTextVerticalAnchor: '',
        GraphTextFontFamily: '',
        GraphWidth: '',
        GraphShape: '',
      },
      ports: {
        in: [
          { portname: 'port1', id: 18, type: 1, iotype: 1 },
          { portname: 'port2', id: 19, type: 1, iotype: 1 },
          { portname: 'port3', id: 20, type: 1, iotype: 1 },
          { portname: 'port4', id: 21, type: 1, iotype: 1 },
          { portname: 'port5', id: 22, type: 1, iotype: 1 },
          { portname: 'port6', id: 23, type: 1, iotype: 1 },
          { portname: 'port7', id: 24, type: 1, iotype: 1 },
        ],
        out: [
          { portname: 'port1', id: 25, type: 1, iotype: 1 },
          { portname: 'port2', id: 26, type: 1, iotype: 1 },
          { portname: 'port3', id: 27, type: 1, iotype: 1 },
          { portname: 'port4', id: 28, type: 1, iotype: 1 },
          { portname: 'port5', id: 29, type: 1, iotype: 1 },
          { portname: 'port6', id: 30, type: 1, iotype: 1 },
          { portname: 'port7', id: 31, type: 1, iotype: 1 },
        ],
      },
    },
    {
      devicename: '设备4',
      id: '66',
      type: 'device',
      logo: 'control',
      image: './assets/logo.png',
      remark: '这是一个设备，拖动它放到设计器上',
      prop: {
        GraphStroke: '#d9d9d9',
        GraphStrokeWidth: 1,
        GraphTextFill: '',
        GraphTextFontSize: '',
        GraphPostionX: '',
        GraphPostionY: '',
        GraphFill: '',
        GraphTextRefX: '',
        GraphHeight: '',
        GraphTextRefY: '',
        GraphTextAnchor: '',
        GraphTextVerticalAnchor: '',
        GraphTextFontFamily: '',
        GraphWidth: '',
        GraphShape: '',
      },
      ports: { in: [{ portname: 'port1', id: 32, type: 1, iotype: 1 }] },
    },
    {
      devicename: '设备5',
      id: '77',
      type: 'device',
      logo: 'control',
      image: './assets/logo.png',
      remark: '这是一个设备，拖动它放到设计器上',
      prop: {
        GraphStroke: '#d9d9d9',
        GraphStrokeWidth: 1,
        GraphTextFill: '',
        GraphTextFontSize: '',
        GraphPostionX: '',
        GraphPostionY: '',
        GraphFill: '',
        GraphTextRefX: '',
        GraphHeight: '',
        GraphTextRefY: '',
        GraphTextAnchor: '',
        GraphTextVerticalAnchor: '',
        GraphTextFontFamily: '',
        GraphWidth: '',
        GraphShape: '',
      },
      ports: { in: [{ portname: 'port1', id: 33, type: 1, iotype: 1 }] },
    },
    {
      devicename: '设备6',
      id: '88',
      type: 'device',
      logo: 'control',
      image: './assets/logo.png',
      remark: '这是一个设备，拖动它放到设计器上',
      prop: {
        GraphStroke: '#d9d9d9',
        GraphStrokeWidth: 1,
        GraphTextFill: '',
        GraphTextFontSize: '',
        GraphPostionX: '',
        GraphPostionY: '',
        GraphFill: '',
        GraphTextRefX: '',
        GraphHeight: '',
        GraphTextRefY: '',
        GraphTextAnchor: '',
        GraphTextVerticalAnchor: '',
        GraphTextFontFamily: '',
        GraphWidth: '',
        GraphShape: '',
      },
      ports: { in: [{ portname: 'port1', id: 34, type: 1, iotype: 1 }] },
    },
    {
      devicename: '设备7',
      id: '99',
      type: 'device',
      logo: 'control',
      image: './assets/logo.png',
      remark: '这是一个设备，拖动它放到设计器上',
      prop: {
        GraphStroke: '#d9d9d9',
        GraphStrokeWidth: 1,
        GraphTextFill: '',
        GraphTextFontSize: '',
        GraphPostionX: '',
        GraphPostionY: '',
        GraphFill: '',
        GraphTextRefX: '',
        GraphHeight: '',
        GraphTextRefY: '',
        GraphTextAnchor: '',
        GraphTextVerticalAnchor: '',
        GraphTextFontFamily: '',
        GraphWidth: '',
        GraphShape: '',
      },
      ports: { in: [{ portname: 'port1', id: 35, type: 1, iotype: 1 }] },
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
  toolbtnclick = ({ cell }) => {
    this.data = [...this.data, cell.getProp('Biz')]; //设计器删除的设备返回设备列表
    this.graph.removeCell(cell);
  };
  tools: any = [
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
        onClick: this.toolbtnclick, //闭包了哟
      },
    },
  ];
  constructor(cdr: ChangeDetectorRef, private settingService: SettingsService) { }

  newport(id) {
    this.selcetedDevice.ports.in = [...this.selcetedDevice.ports.in, { id: '0', portname: '新端口', type: 1, iotype: 1 }];
    console.log(this.selcetedDevice.ports.in);
  }

  private submit(i: STData): void {
    this.updateEdit(i, false);
  }

  private updateEdit(i: STData, edit: boolean): void {
    this.st.setRow(i, { edit }, { refreshSchema: true });
  }
  ngOnInit(): void {
    this.graph = new Graph({
      background: { color: '' },
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
      onPortRendered: (args) => { },
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
      var matadata = e.node.getProp('Biz');
      this.graph.getNodes;

      for (var item of this.graph.getNodes()) {
        var _matadata = e.node.getProp('Biz');

        item.attr({
          root: {
            magnet: false,
          },
          body: {
            fill: '#eeffee',
            stroke: _matadata.prop.GraphStroke,
            strokeWidth: _matadata.prop.GraphStrokeWidth,
          },
        });
      }
      e.cell.attr(this.selectedstyle);
      this.selcetedDevice = matadata;
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

  dragend($event) { }
  onDrop($event) {
    switch ($event.dropData.type) {
      case 'device':
        var ports = [];
        if ($event.dropData.ports.in) {
          for (var item of $event.dropData.ports.in) {
            var port = {
              id: item.id,
              group: 'in',

              zIndex: 'auto',
              attrs: {
                text: {
                  // 标签选择器
                  text: item.portname, // 标签文本
                },
              },
            };
            ports = [...ports, port];
          }
        }
        if ($event.dropData.ports.out) {
          for (var item of $event.dropData.ports.out) {
            var port = {
              id: item.id,
              group: 'out',
              zIndex: 'auto',
              attrs: {
                text: {
                  // 标签选择器
                  text: item.portname, // 标签文本
                },
              },
            };
            ports = [...ports, port];
          }
        }

        var data = {
          label: $event.dropData.devicename,
          height: 80,
          width: 160,
          tools: this.tools,
          offsetX: this.dragEndlocation.offsetX,
          offsetY: this.dragEndlocation.offsetY,
          Biz: $event.dropData,
          portdata: {
            group: {
              in: {
                label: {
                  position: 'left',
                },
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
            ports: ports,
          },
        };
        this.createshape(data);

        break;

      case 'gateway':
        // var node = this.graph.addNode(
        //   new GateWay({
        //     label: $event.dropData.devicename,
        //     tools: this.tools,
        //   })
        //     .setProp('Biz', $event.dropData)
        //     .resize(160, 200)
        //     .position(this.dragEndlocation.offsetX, this.dragEndlocation.offsetY)
        //     .updateInPorts(this.graph),
        // );
        // node.setPortLabelMarkup;

        // this.data.splice(this.data.indexOf($event.dropData), 1);

        var ports = [];

        for (var item of $event.dropData.ports.in) {
          var port = {
            id: item.id,
            group: 'in',

            zIndex: 'auto',
            attrs: {
              text: {
                // 标签选择器
                text: item.portname, // 标签文本
              },
            },
          };
          ports = [...ports, port];
        }

        for (var item of $event.dropData.ports.out) {
          var port = {
            id: item.id,
            group: 'out',

            zIndex: 'auto',
            attrs: {
              text: {
                // 标签选择器
                text: item.portname, // 标签文本
              },
            },
          };
          ports = [...ports, port];
        }
        console.log(ports);
        var data = {
          label: $event.dropData.devicename,
          height: 200,
          width: 160,
          tools: this.tools,
          offsetX: this.dragEndlocation.offsetX,
          offsetY: this.dragEndlocation.offsetY,
          Biz: $event.dropData,
          portdata: {
            group: {
              in: {
                label: {
                  position: 'left',
                },
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
            ports: ports,
          },
        };
        this.createshape(data);
        break;
    }
  }

  createshape(data: any) {
    var node = this.graph.addNode(
      new GateWay({
        label: data.label,
        tools: data.tools,
      })
        .setProp('Biz', data.Biz)
        .resize(data.width, data.height)
        .position(this.dragEndlocation.offsetX, this.dragEndlocation.offsetY)
        .initports(data.portdata)
        .setAttrs({
          root: {
            magnet: false,
          },
          body: {
            fill: '#eeffee',
            stroke: data.Biz.prop.GraphStroke,
            strokeWidth: data.Biz.prop.GraphStrokeWidth,
          },
        }),
      //   .updateInPorts(this.graph),
    );
    node.setPortLabelMarkup;
    this.data.splice(this.data.indexOf(data.Biz), 1);
  }

  dragEnd(event) { }

  onmove($event) {
    this.dragEndlocation = $event;
  }

  dragEndlocation: any;

  load() { }

  savediagram() {
    var edges = this.graph.getEdges();
    var nodes = this.graph.getNodes();
    var shapes = [];
    var mappings = [];
    for (var item of nodes) {
      var port = item.ports.items;
      var incomes = port.filter((x) => x.group === 'in').map((x) => x.id);
      var outgoings = port.filter((x) => x.group === 'out').map((x) => x.id);
      var data = item.getProp('Biz');
      var dev = {
        incomes,
        outgoings,

        prop: {
          position: item.getPosition(),
          size: item.getSize(),
          body: item.getAttrs().body,
          text: item.getAttrs().text,
        },
        id: data.id,
        type: data.type,
      };
      shapes = [...shapes, dev];
    }

    for (var _item of edges) {
      var edge = {
        id: _item.id,
        source: _item.source,
        target: _item.target,
      };
      mappings = [...mappings, edge];
    }

    var graph = {
      shapes,
      mappings,
    };

    console.log(graph);
  }
}
export interface DeviceItem {
  devicename: string;
  id: string;
  type: string;
  logo: string;
  image: string;
  remark: string;
  prop: any;
  ports: any;
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

class Device extends Shape.Rect {
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
  initports(data: any) {
    const ports = this.getInPorts();
    console.log(data);
    console.log(this.ports);
    for (var item of data.ports) {
      this.addPort(item);
    }
    return this;
  }

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

  // updateInPorts(graph: Graph) {
  //   const minNumberOfPorts = 8;
  //   const ports = this.getInPorts();
  //   const usedPorts = this.getUsedInPorts(graph);
  //   const newPorts = this.getNewInPorts(Math.max(minNumberOfPorts - usedPorts.length, 1));

  //   if (ports.length === minNumberOfPorts && ports.length - usedPorts.length > 0) {
  //     // noop
  //   } else if (ports.length === usedPorts.length) {
  //     this.addPorts(newPorts);
  //   } else if (ports.length + 1 > usedPorts.length) {
  //     this.prop(['ports', 'items'], this.getOutPorts().concat(usedPorts).concat(newPorts), {
  //       rewrite: true,
  //     });
  //   }

  //   return this;
  // }
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
    groups: {
      in: {
        label: {
          position: 'left',
        },
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

export interface port {
  portid: number;
  portName: string;
  portType: number;
  portPhyType: number;
}
