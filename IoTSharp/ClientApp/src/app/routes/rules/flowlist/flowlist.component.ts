import { Component, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { STColumn, STColumnBadge, STColumnTag, STComponent, STData, STPage, STReq, STRes } from '@delon/abc/st';
import { _HttpClient, SettingsService } from '@delon/theme';
import { Guid } from 'guid-typescript';
import { NzDrawerService } from 'ng-zorro-antd/drawer';
import { NzMessageService } from 'ng-zorro-antd/message';
import { Subscription } from 'rxjs';
import { appmessage } from 'src/app/models/appmessage';
import { flow, flowrule } from 'src/app/models/flowrule';
import { CommonDialogSevice } from '../../util/commonDialogSevice';
import { FlowsimulatorComponent } from '../../widgets/flowsimulator/flowsimulator.component';
import { FlowformComponent } from '../flowform/flowform.component';
import { ForkdialogComponent } from '../forkdialog/forkdialog.component';
import { SequenceflowtesterComponent } from '../sequenceflowtester/sequenceflowtester.component';
import { TasktesterComponent } from '../tasktester/tasktester.component';
@Component({
  selector: 'app-flowlist',
  templateUrl: './flowlist.component.html',
  styleUrls: ['./flowlist.component.less']
})
export class FlowlistComponent implements OnInit {
  constructor(
    private http: _HttpClient,
    private msg: NzMessageService,
    private _router: Router,
    private drawerService: NzDrawerService,
    private settingService: SettingsService
  ) { }
  TAG: STColumnTag = {
    Telemetry: { text: '遥测', color: 'geekblue' },
    Attribute: { text: '属性', color: 'orange' },
    RAW: { text: 'RAW', color: 'blue' },
    RPC: { text: 'RPC', color: 'cyan' },
    Online: { text: 'Online', color: 'green' },
    Offline: { text: 'Offline', color: 'red' },
    TelemetryArray: { text: '遥测数组', color: 'purple' },
    Alarm: { text: '告警', color: 'magenta' }
  };
  page: STPage = {
    front: false,
    total: true,
    zeroIndexed: true
  };
  q: {
    pi: number;
    ps: number;
    Name: string;
    Creator: string;
    CreatTime: Date[];
    sorter: string;
    status: number | null;
  } = {
      pi: 0,
      ps: 10,
      Name: '',
      Creator: '',
      CreatTime: [],
      sorter: '',
      status: null
    };
  total = 0;

  loading = false;

  url = 'api/rules/index';
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
    { title: '', index: 'ruleId', type: 'checkbox' },
    // { title: 'id', index: 'ruleId' },
    { title: '规则名称', index: 'name', render: 'name' },
    { title: '备注', index: 'ruledesc' },
    { title: '创建时间', type: 'date', index: 'creatTime' },
    { title: '挂载点', type: 'tag', index: 'mountType', tag: this.TAG },
    {
      title: { i18n: 'i18n.columns.Status' },
      index: 'ruleStatus',
      render: 'ruleStatus',
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
          text: '修改',
          click: (item: flowrule) => {
            this.openComponent(item.ruleId);
          }
        },
        {
          text: record => (record.rulestatus == 1 ? '禁用' : '启用'),
          pop: {
            title: '确认修改规则状态?',
            okType: 'danger',
            icon: 'warning'
          },
          click: () => {
            //do something
          }
        },

        {
          text: () => '复制',

          type: 'modal',
          modal: {
            component: ForkdialogComponent
          },
          click: (record, modal) => {
            this.msg.create(`${modal.data ? 'success' : 'error'}`, `规则复制：${modal.data ? '成功' : '失败'}`);
            this.getData();
          }
        },
        {
          text: '设计',
          //   acl: 104,
          click: (item: flowrule) => {
            this._router.navigate(['/iot/rules/designer'], {
              queryParams: {
                Id: item.ruleId,
                type: 'clone'
              }
            });
          }
        },
        {
          text: '测试',
          //  acl: 104,

          click: (item: flowrule) => {
            this.testthisflow(item);
          }
        },
        {
          text: '设备管理',
          /* i18n: 'common.asset',*/
          acl: 60,
          click: (item: any) => {
            this._router.navigateByUrl('iot/rules/ruledevice?id=' + item.ruleId);
          }
        },
        {
          text: '删除',
          //    acl: 104,
          pop: {
            title: '确认删除规则?',
            okType: 'danger',
            icon: 'warning'
          },
          click: (item: flowrule) => {
            this.http.get<appmessage<any>>('api/rules/delete?id=' + item.ruleId).subscribe(
              {

                next: next => {
                  this.msg.create('success', '规则删除成功');
                  this.getData();
                },
                error: error => {
                  this.msg.create('error', '规则删除失败');
                  this.getData();
                },
                complete: () => { }
              }
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

  ngOnInit() { }
  openComponent(id: string): void {
    var { nzMaskClosable, width } = this.settingService.getData('drawerconfig');
    var title = id === Guid.EMPTY ? '新建规则' : '修改规则';
    const drawerRef = this.drawerService.create<FlowformComponent, { id: string }, string>({
      nzTitle: title,
      nzContent: FlowformComponent,
      nzWidth: width,
      nzMaskClosable: nzMaskClosable,
      nzContentParams: {
        id: id
      }
    });

    drawerRef.afterOpen.subscribe(() => { });

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
          this.http.get<appmessage<flow>>('api/rules/GetFlows?ruleId=' + $event.expand?.ruleId).subscribe(
            next => {
              console.log(next);
              $event.expand.flows = next.data;
            },
            () => { },
            () => { }
          );
        }
        break;
    }
  }

  testthisflow(ruleflow: flowrule): void {
    var { nzMaskClosable, width } = this.settingService.getData('drawerconfig');
    var title = '测试' + ruleflow.name;
    const drawerRef = this.drawerService.create<FlowsimulatorComponent, { id: string }, string>({
      nzTitle: title,
      nzContent: FlowsimulatorComponent,
      nzWidth: width < 1280 ? 1280 : width,
      nzMaskClosable: nzMaskClosable,
      nzContentParams: {
        id: ruleflow.ruleId
      }
    });
    drawerRef.afterOpen.subscribe(() => { });
    drawerRef.afterClose.subscribe(data => {
      this.st.load(this.st.pi);
      if (typeof data === 'string') {
      }
    });
  }

  testunit(flow: flow) {
    switch (flow.flowType) {
      case 'bpmn:Task':
        {
          var { nzMaskClosable, width } = this.settingService.getData('drawerconfig');
          var title = '测试' + (flow.flowname ?? flow.bpmnid);
          const drawerRef = this.drawerService.create<SequenceflowtesterComponent, { flow: flow }, string>({
            nzTitle: title,
            nzContent: SequenceflowtesterComponent,
            nzWidth: width < 1280 ? 1280 : width,
            nzMaskClosable: nzMaskClosable,
            nzContentParams: {
              flow: flow
            }
          });
          drawerRef.afterOpen.subscribe(() => { });
          drawerRef.afterClose.subscribe(data => {
            if (typeof data === 'string') {
            }
          });
        }
        break;
      case 'bpmn:StartEvent':
        {
          var { nzMaskClosable, width } = this.settingService.getData('drawerconfig');
          var title = '测试' + (flow.flowname ?? flow.bpmnid);
          const drawerRef = this.drawerService.create<SequenceflowtesterComponent, { flow: flow }, string>({
            nzTitle: title,
            nzContent: SequenceflowtesterComponent,
            nzWidth: width < 1280 ? 1280 : width,
            nzMaskClosable: nzMaskClosable,
            nzContentParams: {
              flow: flow
            }
          });
          drawerRef.afterOpen.subscribe(() => { });
          drawerRef.afterClose.subscribe(data => {
            if (typeof data === 'string') {
            }
          });
        }

        break;
    }
  }

  testscript(flow: flow) {
    switch (flow.flowType) {
      case 'bpmn:Task':
        var { nzMaskClosable, width } = this.settingService.getData('drawerconfig');
        var title = '测试' + (flow.flowname ?? flow.bpmnid);
        const drawerRef = this.drawerService.create<TasktesterComponent, { flow: flow }, string>({
          nzTitle: title,
          nzContent: TasktesterComponent,
          nzWidth: width < 1280 ? 1280 : width,
          nzMaskClosable: nzMaskClosable,
          nzContentParams: {
            flow: flow
          }
        });
        drawerRef.afterOpen.subscribe(() => { });
        drawerRef.afterClose.subscribe(data => {
          if (typeof data === 'string') {
          }
        });
        break;
      case 'bpmn:SequenceFlow':
        break;
    }
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
      Creator: '',
      CreatTime: [],
      sorter: '',
      status: null
    };
  }
  setstatus() { }
}
