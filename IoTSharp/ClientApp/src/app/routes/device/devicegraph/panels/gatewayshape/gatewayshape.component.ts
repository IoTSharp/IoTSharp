import { ChangeDetectorRef, Component, OnInit, ViewChild } from '@angular/core';
import { Node } from '@antv/x6';
import { STColumn, STColumnTag, STComponent, STData } from '@delon/abc/st';
import { Guid } from 'guid-typescript';
import { DeviceItem } from '../../models/data';
import { GateWay } from '../../models/shape';
import { IBizData, IShapeData, IToolsPanel } from '../toolspanel';

@Component({
  selector: 'app-gatewayshape',
  templateUrl: './gatewayshape.component.html',
  styleUrls: ['./gatewayshape.component.less']
})
export class GatewayshapeComponent implements OnInit, IToolsPanel {
  @ViewChild('st')
  private st: STComponent;

  ports: any[] = [];
  TAG: STColumnTag = {
    1: { text: '以太网', color: 'green' },
    2: { text: 'RS232', color: 'blue' },
    3: { text: 'RS485', color: 'orange' },
    4: { text: 'RJ11', color: 'orange' },
  };

  IOType: STColumnTag = {
    1: { text: '输入', color: 'green' },
    2: { text: '输出', color: 'blue' }
  };
  columns: STColumn[] = [
    { title: '名称', index: 'portname', render: 'portNameTpl' },
    { title: 'IO类型', index: 'iotype', render: 'portTypeTpl', tag: this.IOType },
    {
      title: '类型',
      index: 'type',
      render: 'portPhyTypeTpl',
      type: 'enum',
      tag: this.TAG
    },
    {
      title: '操作',
      buttons: [
        {
          text: `修改`,
          iif: i => !i.edit,
          click: i => this.updateEdit(i, true)
        },
        {
          text: `保存`,
          iif: i => i.edit,
          click: i => {
            this.submit(i);
          }
        },
        {
          text: `取消`,
          iif: i => i.edit,
          click: i => this.updateEdit(i, false)
        }
      ]
    }
  ];
  constructor(private cdr: ChangeDetectorRef) {}
  BizData: DeviceItem = { id: '-1', x: 11 };
  ShapeData: IShapeData;
  ngOnInit(): void {
    console.log(this.BizData);
    var _in = this.BizData?.ports?.in ?? [];
    var _out = this.BizData?.ports?.out ?? [];
    this.ports = [..._in, ..._out];

    this.cdr.detectChanges();
  }
  private updateEdit(i: STData, edit: boolean): void {
    this.st.setRow(i, { edit }, { refreshSchema: true });
    var gateway = this.BizData.mateData as GateWay;
    gateway.setPortProp(i.id, {
      group: i.iotype == 1 ? 'in' : 'out',
      attrs: {
        text: {
          text: i.portname
        }
      }
    });
    if (i.iotype === 1) {
     var item= this.BizData.ports.in.find(c=>c.id===i.id);
     item=i
    } else {
      var item= this.BizData.ports.out.find(c=>c.id===i.id); 
      item=i
    }
  }

  private submit(i: STData): void {
    this.updateEdit(i, false);
  }
  newport(id) {
    var _port = { id: Guid.create().toString(), portname: '新端口', type: 1, iotype: 1 };
    this.ports = [...this.ports, _port];
    var gateway = this.BizData.mateData as GateWay;
    this.BizData.ports.in = this.BizData.ports.in ?? [];
    this.BizData.ports.out = this.BizData.ports.out ?? [];
    if (_port.iotype == 1) {
      this.BizData.ports.in = [...this.BizData.ports.in, _port];
    } else {
      this.BizData.ports.out = [...this.BizData.ports.out, _port];
    }
    this.BizData.mateData.setProp('Biz', this.BizData);
    gateway.addPort({
      group: _port.iotype == 1 ? 'in' : 'out',
      id: _port.id,
      attrs: {
        text: {
          text: _port.portname
        }
      }
    });
  }
}
