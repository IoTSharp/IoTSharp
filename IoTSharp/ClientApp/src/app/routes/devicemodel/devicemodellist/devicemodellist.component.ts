import { Component, OnInit, ViewChild } from '@angular/core';
import { Router } from '@angular/router';
import { STColumnTag, STPage, STReq, STRes, STComponent, STColumn, STData } from '@delon/abc/st';
import { _HttpClient, SettingsService } from '@delon/theme';
import { Guid } from 'guid-typescript';
import { NzDrawerService } from 'ng-zorro-antd/drawer';
import { NzMessageService } from 'ng-zorro-antd/message';
import { appmessage } from '../../common/AppMessage';
import { DevicemodelcommandComponent } from '../devicemodelcommand/devicemodelcommand.component';
import { devicemodel, devicemodelcommand, devicemodelcommandparam } from '../devicemodelcommandparam';

import { DevicemodelformComponent } from '../devicemodelform/devicemodelform.component';

@Component({
  selector: 'app-devicemodellist',
  templateUrl: './devicemodellist.component.html',
  styleUrls: ['./devicemodellist.component.less']
})
export class DevicemodellistComponent implements OnInit {
  constructor(
    private http: _HttpClient,
    private msg: NzMessageService,
    private _router: Router,
    private drawerService: NzDrawerService,
    private settingService: SettingsService
  ) {}

  page: STPage = {
    front: false,
    total: true,
    zeroIndexed: true
  };
  q: {
    pi: number;
    ps: number;
    Name: string;

    sorter: string;
    status: number | null;
  } = {
    pi: 0,
    ps: 10,
    Name: '',
    sorter: '',
    status: null
  };
  total = 0;

  loading = false;

  url = 'api/devicemodel/index';
  req: STReq = { method: 'POST', allInBody: true, reName: { pi: 'offset', ps: 'limit' }, params: this.q };

  // 定义返回的参数
  res: STRes = {
    reName: {
      total: 'data.total',
      list: 'data.rows'
    }
  };

  @ViewChild('st', { static: true })
  st!: STComponent;
  columns: STColumn[] = [
    { title: '', index: 'deviceModelId', type: 'checkbox' },
    { title: 'id', index: 'deviceModelId' },
    { title: '型号名称', index: 'modelName', render: 'modelName' },
    { title: '备注', index: 'modelDesc' },
    { title: '创建时间', type: 'date', index: 'createDateTime' },

    {
      title: { i18n: 'i18n.columns.Status' },
      index: 'mdelStatus',
      render: 'modelStatus',
      type: 'badge',
      badge: {
        0: { text: '禁用', color: 'error' },
        1: { text: '启用', color: 'success' }
      }
    },

    {
      title: '操作',
      buttons: [
        {
          acl: 9,
          text: '新增指令',
          click: (item: devicemodel) => {
            this.AddNewCommandComponent(item.deviceModelId, Guid.EMPTY);
          }
        },
        {
          acl: 9,
          text: '修改',
          click: (item: devicemodel) => {
            this.openComponent(item.deviceModelId);
          }
        },
        {
          text: record => (record.rulestatus == 1 ? '禁用' : '启用'),
          pop: {
            title: '确认修改型号状态?',
            okType: 'danger',
            icon: 'warning'
          },
          click: () => {
            //do something
          }
        },

        {
          text: '删除',
          //    acl: 104,
          pop: {
            title: '确认删除型号?',
            okType: 'danger',
            icon: 'warning'
          },
          click: (item: devicemodel) => {
            this.http.get('api/deviceModel/delete?id=' + item.deviceModelId).subscribe(
              () => {
                this.msg.create('success', '型号删除成功');
                this.getData();
              },
              () => {
                this.msg.create('error', '型号删除失败');
                this.getData();
              },
              () => {}
            );
          }
        }
      ]
    }
  ];
  selectedRows: STData[] = [];
  description = '';
  totalCallNo = 0;
  expandForm = false;

  ngOnInit() {}

  openComponent(id: string): void {
    var { nzMaskClosable, width } = this.settingService.getData('drawerconfig');
    var title = id === Guid.EMPTY ? '新建型号' : '修改型号';
    const drawerRef = this.drawerService.create<DevicemodelformComponent, { id: string }, string>({
      nzTitle: title,
      nzContent: DevicemodelformComponent,
      nzWidth: width,
      nzMaskClosable: nzMaskClosable,
      nzContentParams: {
        id: id
      }
    });

    drawerRef.afterOpen.subscribe(() => {});

    drawerRef.afterClose.subscribe(data => {
      this.st.load(this.st.pi);
      if (typeof data === 'string') {
      }
    });
  }

  AddNewCommandComponent(id: string, commandid: string): void {
    var { nzMaskClosable, width } = this.settingService.getData('drawerconfig');
    var title = id === Guid.EMPTY ? '新建型号' : '修改型号';
    const drawerRef = this.drawerService.create<DevicemodelcommandComponent, { id: devicemodelcommandparam }, string>({
      nzTitle: title,
      nzContent: DevicemodelcommandComponent,
      nzWidth: width,
      nzMaskClosable: nzMaskClosable,
      nzContentParams: {
        id: { commandId: commandid, deviceModelId: id }
      }
    });

    drawerRef.afterOpen.subscribe(() => {});

    drawerRef.afterClose.subscribe(data => {
      this.st.load(this.st.pi);
      if (typeof data === 'string') {
      }
    });
  }

  onchange($event) {
    switch ($event.type) {
      case 'expand':
        if ($event.expand.expand) {
          this.http.get<appmessage<devicemodelcommand>>('api/deviceModel/GetCommands?id=' + $event.expand?.deviceModelId).subscribe(
            next => {
              console.log(next);
              $event.expand.deviceModelCommands = next.data;
            },
            () => {},
            () => {}
          );
        }
        break;
    }
  }

  editCommand(item: devicemodelcommand) {
    this.AddNewCommandComponent(item.deviceModelId, item.commandId);
  }

  deleteCommand(item: devicemodelcommand,model:devicemodel) {
    this.http.get<appmessage<Boolean>>('api/deviceModel/deleteCommand?id=' + item.commandId).subscribe(
      next => {
        if (next.data) {

          model.deviceModelCommands=    model.deviceModelCommands.filter(c=>c.commandId!=item.commandId);

        }
      },
      error => {},
      () => {}
    );
  }
  getData() {
    this.st.req = this.req;
    this.st.load(this.st.pi);
  }
  reset() {
    this.q = {
      pi: 0,
      ps: 10,
      Name: '',

      sorter: '',
      status: null
    };
  }
}
