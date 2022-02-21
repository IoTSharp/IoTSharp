import { ChangeDetectorRef, Component, ComponentFactoryResolver, ElementRef, Input, OnInit, Type, ViewChild } from '@angular/core';
import { Cell, Graph, NodeView, Shape } from '@antv/x6';

import { STColumn, STColumnTag, STComponent, STData } from '@delon/abc/st';
import { SettingsService } from '@delon/theme';
import { Observable, Subscription, timer } from 'rxjs';
import { DeviceItem, port } from './models/data';
import { Device, GateWay } from './models/shape';
import { ConnectionedgeComponent } from './panels/connectionedge/connectionedge.component';
import { DevivceshapeComponent } from './panels/devivceshape/devivceshape.component';
import { GatewayshapeComponent } from './panels/gatewayshape/gatewayshape.component';
import { PortshapeComponent } from './panels/portshape/portshape.component';
import { toolpaneldirective } from './panels/toolpaneldirective';
import { IToolsPanel, PanelItem } from './panels/toolspanel';

@Component({
  selector: 'app-devicegraph',
  templateUrl: './devicegraph.component.html',
  styleUrls: ['./devicegraph.component.less'],
})
export class DevicegraphComponent implements OnInit {
  @ViewChild(toolpaneldirective, { static: true })
  toolpanelcontainer!: toolpaneldirective;
  portiseselected = false;
  //注册工具面板
  toolpanels = [
    new PanelItem<IToolsPanel>('device', DevivceshapeComponent, {

    }),
    new PanelItem<IToolsPanel>('gateway', GatewayshapeComponent, {
      //  someneedtransferdata: "yourdata,don't forget declara a @Input someneedtransferdata Property ",
    }),
    new PanelItem<IToolsPanel>('edge', ConnectionedgeComponent, {
      //  someneedtransferdata: "yourdata,don't forget declara a @Input someneedtransferdata Property ",
    }),

    new PanelItem<IToolsPanel>('port', PortshapeComponent, {
      //  someneedtransferdata: "yourdata,don't forget declara a @Input someneedtransferdata Property ",
    })
  ];

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

  selectedport =
    {
      port: {},
      owner: {},
      orginattr: {}

    };
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
        in: [{ portname: '温度', id: '1', type: 1, iotype: 1 }],
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
          { portname: 'port1', id: '18', type: 1, iotype: 1 },
          { portname: 'port2', id: '19', type: 1, iotype: 1 },
          { portname: 'port3', id: '20', type: 1, iotype: 1 },
          { portname: 'port4', id: '21', type: 1, iotype: 1 },
          { portname: 'port5', id: '22', type: 1, iotype: 1 },
          { portname: 'port6', id: '23', type: 1, iotype: 1 },
          { portname: 'port7', id: '24', type: 1, iotype: 1 },
        ],
        out: [
          { portname: 'port1', id: '25', type: 1, iotype: 1 },
          { portname: 'port2', id: '26', type: 1, iotype: 1 },
          { portname: 'port3', id: '27', type: 1, iotype: 1 },
          { portname: 'port4', id: '28', type: 1, iotype: 1 },
          { portname: 'port5', id: '29', type: 1, iotype: 1 },
          { portname: 'port6', id: '30', type: 1, iotype: 1 },
          { portname: 'port7', id: '31', type: 1, iotype: 1 },
        ],
      },
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
      ports: { in: [{ portname: '湿度', id: '35', type: 1, iotype: 1 }] },
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
        onClick: this.toolbtnclick,
      },
    },
  ];
  constructor(cdr: ChangeDetectorRef, private settingService: SettingsService, private componentFactoryResolver: ComponentFactoryResolver) { }

  newport(id) {
    this.selcetedDevice.ports.in = [...this.selcetedDevice.ports.in, { id: '0', portname: '新端口', type: 1, iotype: 1 }];
    console.log(this.selcetedDevice.ports.in);
  }

  private createpanel(
    panel: string, BizData: any
  ) {
    const componentFactory = this.componentFactoryResolver.resolveComponentFactory(this.toolpanels.find(c => c.name === panel)?.component);
    const viewContainerRef = this.toolpanelcontainer.viewContainerRef;
    viewContainerRef.clear();
    const componentRef = viewContainerRef.createComponent<DevivceshapeComponent>(componentFactory);
    componentRef.instance.BizData = BizData;
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

    this.graph.on('edge:connected', ({ previousView, currentView, previousCell, currentCell, isNew, edge }) => {
      if (previousView) {
        this.update(previousView as NodeView);
      }
      if (currentView) {
        this.update(currentView as NodeView);
      }

      var targetcell = edge.getTargetCell()
      var sourcecell = edge.getSourceCell() as GateWay
      var targetport = edge.getTargetPortId();
      var sourceport = edge.getSourcePortId();
      var td = targetcell.getProp('Biz') as DeviceItem;
      var sd = sourcecell.getProp('Biz') as DeviceItem;
      var g = sourcecell as GateWay
      var d = targetcell as Device;
      var ip = d.getPort(targetport);
      sourcecell.setPortProp(sourceport, 'attrs/text', {
        text: ip.attrs.text.text
      })


      var sp = g.getPort(sourceport);
      sp.attrs = { text: { text: 'dasd' } }

      console.log(sp);


    });



    this.graph.on('edge:added', ({ edge, index, options }) => {









    })


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

    this.graph.on('cell:click', ({ e, x, y, cell, view }) => {

      console.log(this.selectedport)

      if (this.selectedport.port['id'] && this.selectedport.owner['id']) {
        var shape = this.graph.getCellById(this.selectedport.owner['id'])
        var md = shape.getProp('Biz') as DeviceItem;
        switch (md.type) {
          case 'device':
            {
              var d = shape as Device;
              switch (this.selectedport.port['group']) {
                case 'in':
                  d.setPortProp(this.selectedport.port['id'], 'attrs/circle', {
                    stroke: '#ff0000',
                    strokeWidth: 2,
                  })
                  break;
                case 'out':
                  d.setPortProp(this.selectedport.port['id'], 'attrs/circle', {
                    stroke: '#3199FF',
                    strokeWidth: 2,
                  })
                  break;
              }
            }

            break;

          case 'gateway':
            {
              var g = shape as GateWay;
              switch (this.selectedport.port['group']) {
                case 'in':
                  g.setPortProp(this.selectedport.port['id'], 'attrs/circle', {
                    stroke: '#ff0000',
                    strokeWidth: 2,
                  })
                  break;
                case 'out':
                  g.setPortProp(this.selectedport.port['id'], 'attrs/circle', {
                    stroke: '#3199FF',
                    strokeWidth: 2,
                  })
                  break;
              }
            }

            break;

        }

      }


      this.portiseselected = true;
      var matadata = cell.getProp('Biz') as DeviceItem;
      if (matadata) {

        switch (matadata.type) {
          case 'device':
            {


              if (e.target.attributes?.port?.value) {
                var device = cell as Device
                var port = device.getPort(e.target.attributes.port.value);
                this.selectedport = {
                  port: port,
                  owner: device,
                  orginattr: port.attrs
                }
                device.setPortProp(port.id, 'attrs/circle', this.selectedstyle.body)



                timer(50).subscribe(next => {

                  this.createpanel('port', port)
                });
              }
            }
            break;
          case 'gateway':
            {
              if (e.target.attributes?.port?.value) {
                var gateway = cell as GateWay
                var port = gateway.getPort(e.target.attributes.port.value);



                this.selectedport = {
                  port: port,
                  owner: gateway,
                  orginattr: port.attrs
                }
                  ;


                gateway.setPortProp(port.id, 'attrs/circle', this.selectedstyle.body)
                timer(50).subscribe(next => { this.createpanel('port', port) });


              }
            }
            break;
        }
      }

      this.portiseselected = false;
    });

    this.graph.on('edge:click', ({ e, x, y, edge, view }) => {

    })

    this.graph.on('cell:mousedown', ({ cell }) => {
      //  cell.removeTools(); //只读状态下移除Node中的操作按钮
    });

    this.graph.on('node:click', (e, x, y, node, view) => {




      var matadata = e.node.getProp('Biz');
      if (matadata) {
        if (!this.portiseselected) {
          this.createpanel(matadata.type, matadata)
        }
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


      }





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
        this.createshape(data, 'device');

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
        this.createshape(data, 'gateway');
        break;
    }
  }

  createshape(data: any, type: string) {
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

  loadsense() { }

  savesense() {
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



