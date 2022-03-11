import { ChangeDetectorRef, Component, ComponentFactoryResolver, ElementRef, Input, OnInit, Type, ViewChild } from '@angular/core';
import { Cell, Graph, Node, NodeView, Shape } from '@antv/x6';

import { STColumn, STColumnTag, STComponent, STData } from '@delon/abc/st';
import { SettingsService } from '@delon/theme';
import { Observable, Subscription, timer } from 'rxjs';
import { DeviceItem, EdgeItem, PortItem } from './models/data';
import { Device, GateWay } from './models/shape';
import { ConnectionedgeComponent } from './panels/connectionedge/connectionedge.component';
import { DevivceshapeComponent } from './panels/devivceshape/devivceshape.component';
import { GatewayshapeComponent } from './panels/gatewayshape/gatewayshape.component';
import { PortshapeComponent } from './panels/portshape/portshape.component';
import { toolpaneldirective } from './panels/toolpaneldirective';
import { IBizData, IToolsPanel, PanelItem } from './panels/toolspanel';

@Component({
  selector: 'app-devicegraph',
  templateUrl: './devicegraph.component.html',
  styleUrls: ['./devicegraph.component.less']
})
export class DevicegraphComponent implements OnInit {
  @ViewChild(toolpaneldirective, { static: true })
  toolpanelcontainer!: toolpaneldirective;
  portiseselected = false;
  //注册工具面板
  toolpanels = [
    new PanelItem<IToolsPanel>('device', DevivceshapeComponent, {}, null, false),
    new PanelItem<IToolsPanel>(
      'gateway',
      GatewayshapeComponent,
      {
        //  someneedtransferdata: "yourdata,don't forget declara a @Input someneedtransferdata Property ",
      },
      null,
      false
    ),
    new PanelItem<IToolsPanel>(
      'edge',
      ConnectionedgeComponent,
      {
        //  someneedtransferdata: "yourdata,don't forget declara a @Input someneedtransferdata Property ",
      },
      null,
      false
    ),

    new PanelItem<IToolsPanel>(
      'port',
      PortshapeComponent,
      {
        //  someneedtransferdata: "yourdata,don't forget declara a @Input someneedtransferdata Property ",
      },
      null,
      false
    )
  ];
  selectedstyle = {
    body: {
      stroke: '#00ff00',
      strokeWidth: 5
    }
  };
  selcetedDevice: any = {
    ports: {
      in: []
    }
  };
  ports: PortItem[] = [];
  selectedport = {
    port: {},
    owner: {},
    orginattr: {}
  };
  subscription: Subscription;
  droppedData: string;
  @Input()
  id: Number = -1;
  @ViewChild('container', { static: true })
  private container!: ElementRef;
  @ViewChild('st')
  private st: STComponent;
  data: Array<DeviceItem> = [
    {
      devicename: '设备1',
      id: '11',
      type: 'device',
      logo: 'control',
      image: './assets/logo.png',
      remark: '这是一个设备，拖动它放到设计器上',
      width: 0,
      height: 0,
      x: 0,
      y: 0,
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
        GraphShape: ''
      },
      ports: {}
    },

    {
      devicename: '网关1',
      id: '44',
      type: 'gateway',
      logo: 'ungroup',
      image: './assets/logo.png',
      remark: '这是一个网关，拖动它放到设计器上',
      width: 0,
      height: 0,
      x: 0,
      y: 0,
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
        GraphShape: ''
      },
      ports: {}
    },
    {
      devicename: '网关2',
      id: '55',
      type: 'gateway',
      logo: 'ungroup',
      image: './assets/logo.png',
      remark: '这是一个网关，拖动它放到设计器上',
      width: 0,
      height: 0,
      x: 0,
      y: 0,
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
        GraphShape: ''
      },
      ports: {}
    },

    {
      devicename: '设备7',
      id: '99',
      type: 'device',
      logo: 'control',
      image: './assets/logo.png',
      remark: '这是一个设备，拖动它放到设计器上',
      width: 0,
      height: 0,
      x: 0,
      y: 0,
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
        GraphShape: ''
      },
      ports: {}
    }
  ];

  graph!: Graph;
  magnetAvailabilityHighlighter = {
    name: 'stroke',
    args: {
      attrs: {
        fill: '#fff',
        stroke: '#47C769'
      }
    }
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
              cursor: 'pointer'
            }
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
              y: '0.3em'
            }
          }
        ],
        x: '50%',
        y: '10%',
        offset: { x: -0, y: -0 },
        onClick: this.toolbtnclick
      }
    }
  ];
  constructor(
    cdr: ChangeDetectorRef,
    private settingService: SettingsService,
    private componentFactoryResolver: ComponentFactoryResolver
  ) {}

  newport(id) {
    this.selcetedDevice.ports.in = [...this.selcetedDevice.ports.in, { id: '0', portname: '新端口', type: 1, iotype: 1 }];
    console.log(this.selcetedDevice.ports.in);
  }

  private createpanel(panel: string, BizData: IBizData) {
    var _panel = this.toolpanels.find(c => c.name === panel);
    if (_panel.instance == null || !_panel.isselected) {
      const componentFactory = this.componentFactoryResolver.resolveComponentFactory(_panel?.component);
      const viewContainerRef = this.toolpanelcontainer.viewContainerRef;
      viewContainerRef.clear();
      const componentRef = viewContainerRef.createComponent<IToolsPanel>(componentFactory);
      componentRef.instance.BizData = BizData;
      _panel.instance = componentRef.instance;
      for (const iterator of this.toolpanels) {
        iterator.isselected = false;
      }
      _panel.isselected = true;
    }
    _panel.instance.BizData = BizData;
  }

  private submit(i: STData): void {
    this.updateEdit(i, false);
  }

  private updateEdit(i: STData, edit: boolean): void {
    this.st.setRow(i, { edit }, { refreshSchema: true });
  }
  ngOnInit(): void {
    this.graph = new Graph({
      container: this.container.nativeElement,
      selecting: {
        enabled: true,
        rubberband: true,
        showNodeSelectionBox: true
      },
      background: { color: '' },
      mousewheel: {
        enabled: true,
        zoomAtMousePosition: true,
        modifiers: 'alt',
        minScale: 0.5,
        maxScale: 10
      },
      resizing: true,
      rotating: true,
      autoResize: true,
      grid: true,
      height: 800,
      highlighting: {
        magnetAvailable: this.magnetAvailabilityHighlighter,
        magnetAdsorbed: {
          name: 'stroke',
          args: {
            attrs: {
              fill: '#fff',
              stroke: '#31d0c6'
            }
          }
        }
      },
      onPortRendered: args => {},
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
            direction: 'H'
          }
        },
        createEdge() {
          return new Shape.Edge({
            attrs: {
              line: {
                stroke: '#a0a0a0',
                strokeWidth: 1,
                targetMarker: {
                  name: 'classic',
                  size: 7
                }
              }
            }
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
              if (usedInPorts.find(port => port && port.id === portId)) {
                return false;
              }
            }
          }

          return true;
        }
      }
    });

    this.graph.on('edge:connected', ({ previousView, currentView, previousCell, currentCell, isNew, edge }) => {
      if (previousView) {
        this.update(previousView as NodeView);
      }
      if (currentView) {
        this.update(currentView as NodeView);
      }
      var targetcell = edge.getTargetCell();
      var sourcecell = edge.getSourceCell() as Node;
      var targetport = edge.getTargetPortId();
      var sourceport = edge.getSourcePortId();
      var td = targetcell.getProp('Biz') as DeviceItem;
      var sd = sourcecell.getProp('Biz') as DeviceItem;
      var g = sourcecell as Node;
      var d = targetcell as Node;
      var ip = d.getPort(targetport);
      sourcecell.setPortProp(sourceport, 'attrs/text', {
        text: ip.attrs.text.text
      });
      var sp = g.getPort(sourceport);
      sp.attrs = { text: { text: 'dasd' } };
      console.log(sp);
    });

    this.graph.on('edge:added', ({ edge, index, options }) => {});

    this.graph.on('node:resized', ({ e, x, y, node, view }) => {
      var d = node.getProp('Biz') as DeviceItem;
      d.width = x;
      d.height = y;
      d.mateData = node;
      node.setProp('Biz', d);
      this.createpanel(d.type, d);
    });

    this.graph.on('node:moved', ({ e, x, y, node, view }) => {
      var d = node.getProp('Biz') as DeviceItem;
      d.x = x;
      d.y = y;
      d.mateData = node;
      node.setProp('Biz', d);
      this.createpanel(d.type, d);
    });

    this.graph.on('edge:click', ({ e, x, y, edge, view }) => {
      var _edge: EdgeItem = { id: edge.id, mateData: edge };
      this.createpanel('edge', _edge);
      // console.log(edge.getTargetNode());
      // console.log(edge.getSourceNode());
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
            distance: -30
          }
        }
      ]);
    });

    this.graph.on('cell:click', ({ e, x, y, cell, view }) => {
      if (this.selectedport.port['id'] && this.selectedport.owner['id']) {
        var shape = this.graph.getCellById(this.selectedport.owner['id']);
        var md = shape.getProp('Biz') as DeviceItem;
        switch (md.type) {
          case 'device':
            {
              var d = shape as Device;
              switch (this.selectedport.port['group']) {
                case 'in':
                  d.setPortProp(this.selectedport.port['id'], 'attrs/circle', {
                    stroke: '#ff0000',
                    strokeWidth: 2
                  });
                  break;
                case 'out':
                  d.setPortProp(this.selectedport.port['id'], 'attrs/circle', {
                    stroke: '#3199FF',
                    strokeWidth: 2
                  });
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
                    strokeWidth: 2
                  });
                  break;
                case 'out':
                  g.setPortProp(this.selectedport.port['id'], 'attrs/circle', {
                    stroke: '#3199FF',
                    strokeWidth: 2
                  });
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
                var device = cell as Device;
                var port = device.getPort(e.target.attributes.port.value);
                this.selectedport = {
                  port: port,
                  owner: device,
                  orginattr: port.attrs
                };
                device.setPortProp(port.id, 'attrs/circle', this.selectedstyle.body);
                timer(50).subscribe(next => {
                  var _port: PortItem = {
                    id: port.id,
                    mateData: port,
                    parentRef:device,
                  };
                  this.createpanel('port', _port);
                });
              }
            }
            break;
          case 'gateway':
            {
              if (e.target.attributes?.port?.value) {
                var gateway = cell as GateWay;
                var port = gateway.getPort(e.target.attributes.port.value);
                this.selectedport = {
                  port: port,
                  owner: gateway,
                  orginattr: port.attrs
                };
                gateway.setPortProp(port.id, 'attrs/circle', this.selectedstyle.body);
                timer(50).subscribe(next => {
                  var _port: PortItem = {
                     id: port.id,
                     mateData: port,
                     parentRef:gateway, };
                  this.createpanel('port', _port);
                });
              }
            }
            break;
        }
      }

      this.portiseselected = false;
    });

    this.graph.on('cell:mousedown', ({ cell }) => {
      //  cell.removeTools(); //只读状态下移除Node中的操作按钮
    });

    this.graph.on('node:click', (e, x, y, node, view) => {
     
      var matadata = e.node.getProp('Biz') as DeviceItem;

      if (matadata) {
        if (!this.portiseselected) {
          matadata.devicename;
          matadata.mateData = e.node;
          this.createpanel(matadata.type, matadata);
        }
        this.graph.getNodes;

        for (var item of this.graph.getNodes()) {
          var _matadata = e.node.getProp('Biz');

          item.attr({
            root: {
              magnet: false
            },
            body: {
              fill: '#eeffee',
              stroke: _matadata.prop.GraphStroke,
              strokeWidth: _matadata.prop.GraphStrokeWidth
            }
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
      cell.getInPorts().forEach(port => {
        const portNode = view.findPortElem(port.id!, 'portBody');
        view.unhighlight(portNode, {
          highlighter: this.magnetAvailabilityHighlighter
        });
      });
      cell.updateInPorts(this.graph);
    }
  }

  dragend($event) {}
  onDrop($event) {
    switch ($event.dropData.type) {
      case 'device':
        {
          var ports = [];
          if ($event.dropData.ports?.in) {
            for (var item of $event.dropData.ports?.in) {
              var port = {
                id: item.id,
                group: 'in',

                zIndex: 'auto',
                attrs: {
                  text: {
                    // 标签选择器
                    text: item.portname 
                  }
                }
              };
              ports = [...ports, port];
            }
          }
          if ($event.dropData.ports?.out) {
            for (var item of $event.dropData.ports?.out) {
              var port = {
                id: item.id,
                group: 'out',
                zIndex: 'auto',
                attrs: {
                  text: {
              
                    text: item.portname 
                  }
                }
              };
              ports = [...ports, port];
            }
          }
          $event.dropData.width = 160;
          $event.dropData.height = 80;

          $event.dropData.x = this.dragEndlocation.offsetX;
          $event.dropData.y = this.dragEndlocation.offsetY;
          var data = {
            label: $event.dropData.devicename,
            height: $event.dropData.height,
            width: $event.dropData.width,
            tools: this.tools,
            offsetX: $event.dropData.x,
            offsetY: $event.dropData.y,
            Biz: $event.dropData,

            portdata: {
              group: {
                in: {
                  label: {
                    position: 'left'
                  },
                  position: {
                    name: 'right'
                  },
                  attrs: {
                    portBody: {
                      magnet: 'passive',
                      r: 6,
                      stroke: '#ff0000',
                      fill: '#fff',
                      strokeWidth: 2
                    }
                  }
                },
                out: {
                  position: {
                    name: 'left'
                  },
                  label: {
                    position: 'right'
                  },
                  attrs: {
                    portBody: {
                      magnet: true,
                      r: 6,
                      fill: '#fff',
                      stroke: '#3199FF',
                      strokeWidth: 2
                    }
                  }
                }
              },
              ports: ports
            }
          };
          this.createshape(data, 'device');
        }
        break;

      case 'gateway':
        {
          var ports = [];
          if ($event.dropData.ports?.in) {
            for (var item of $event.dropData.ports?.in) {
              var port = {
                id: item.id,
                group: 'in',

                zIndex: 'auto',
                attrs: {
                  text: {
                    text: item.portname 
                  }
                }
              };
              ports = [...ports, port];
            }
          }
          if ($event.dropData.ports?.out) {
            for (var item of $event.dropData.ports?.out) {
              var port = {
                id: item.id,
                group: 'out',

                zIndex: 'auto',
                attrs: {
                  text: {
                 
                    text: item.portname
                  }
                }
              };
              ports = [...ports, port];
            }
          }
          $event.dropData.width = 240;
          $event.dropData.height = 200;

          $event.dropData.x = this.dragEndlocation.offsetX;
          $event.dropData.y = this.dragEndlocation.offsetY;
          var data = {
            label: $event.dropData.devicename,
            height: $event.dropData.height,
            width: $event.dropData.width,
            tools: this.tools,
            offsetX: $event.dropData.x,
            offsetY: $event.dropData.y,
            Biz: $event.dropData,
            portdata: {
              group: {
                in: {
                  label: {
                    position: 'left'
                  },
                  position: {
                    name: 'right'
                  },
                  attrs: {
                    portBody: {
                      magnet: 'passive',
                      r: 6,
                      stroke: '#ff0000',
                      fill: '#fff',
                      strokeWidth: 2
                    }
                  }
                },
                out: {
                  position: {
                    name: 'left'
                  },
                  label: {
                    position: 'right'
                  },
                  attrs: {
                    portBody: {
                      magnet: true,
                      r: 6,
                      fill: '#fff',
                      stroke: '#3199FF',
                      strokeWidth: 2
                    }
                  }
                }
              },
              ports: ports
            }
          };
          this.createshape(data, 'gateway');
        }
        break;
    }
  }

  createshape(data: any, type: string) {
    switch (type) {
      case 'gateway':
        var node = this.graph.addNode(
          new GateWay({
            label: data.label,
            tools: data.tools,
            imageUrl: data.Biz.image
          })
            .setProp('Biz', data.Biz)
            .resize(data.width, data.height)
            .position(this.dragEndlocation.offsetX, this.dragEndlocation.offsetY)
            .initports(data.portdata)
            .setAttrs({
              root: {
                magnet: false
              },
              body: {
                fill: '#3199FF',
                stroke: data.Biz.prop.GraphStroke,
                strokeWidth: data.Biz.prop.GraphStrokeWidth
              }
            })
          //   .updateInPorts(this.graph),
        );
        node.setPortLabelMarkup;
        this.data.splice(this.data.indexOf(data.Biz), 1);
        node.setAttrs({});

        break;
      case 'device':
        var node = this.graph.addNode(
          new Device({
            label: data.label,
            tools: data.tools,
            imageUrl: data.Biz.image
          })
            .setProp('Biz', data.Biz)
            .resize(data.width, data.height)
            .position(this.dragEndlocation.offsetX, this.dragEndlocation.offsetY)
            .initports(data.portdata)
            .setAttrs({
              root: {
                magnet: false
              },
              body: {
                fill: '#3199FF',
                stroke: data.Biz.prop.GraphStroke,
                strokeWidth: data.Biz.prop.GraphStrokeWidth
              }
            })
        );
        node.setPortLabelMarkup;
        this.data.splice(this.data.indexOf(data.Biz), 1);
        node.setAttrs({});
        break;
    }
  }

  dragEnd(event) {}

  onmove($event) {
    this.dragEndlocation = $event;
  }

  dragEndlocation: any;

  loadsense() {}

  savesense() {
    var edges = this.graph.getEdges();
    var nodes = this.graph.getNodes();
    var shapes = [];
    var mappings = [];
    for (var item of nodes) {
      var port = item.ports.items;
      var incomes = port.filter(x => x.group === 'in').map(x => x.id);
      var outgoings = port.filter(x => x.group === 'out').map(x => x.id);
      var data = item.getProp('Biz');
      var dev = {
        incomes,
        outgoings,

        prop: {
          position: item.getPosition(),
          size: item.getSize(),
          body: item.getAttrs().body,
          text: item.getAttrs().text
        },
        id: data.id,
        type: data.type
      };
      shapes = [...shapes, dev];
    }

    for (var _item of edges) {
      var edge = {
        id: _item.id,
        source: _item.source,
        target: _item.target
      };
      mappings = [...mappings, edge];
    }

    var graph = {
      shapes,
      mappings
    };

    console.log(graph);
  }
}
