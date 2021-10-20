import { ChangeDetectorRef, Component, OnInit, TemplateRef, ViewChild } from '@angular/core';
import { Router } from '@angular/router';
import { STColumn, STComponent, STData, STPage, STReq, STRes } from '@delon/abc/st';
import { ACLService } from '@delon/acl';
import { _HttpClient, ModalHelper, SettingsService } from '@delon/theme';
import { NzDrawerService } from 'ng-zorro-antd/drawer';
import { NzMessageService } from 'ng-zorro-antd/message';
import { FlowsimulatorComponent } from '../../util/flow/flowsimulator/flowsimulator.component';

import { FlowformComponent } from '../flowform/flowform.component';

@Component({
  selector: 'app-flowlist',
  templateUrl: './flowlist.component.html',
  styleUrls: ['./flowlist.component.less'],
})
export class FlowlistComponent implements OnInit {
  constructor(
    private http: _HttpClient,
    public msg: NzMessageService,
    private modal: ModalHelper,
    private cdr: ChangeDetectorRef,
    private _router: Router,
    private drawerService: NzDrawerService,
    private settingService: SettingsService,
    aclSrv: ACLService,
  ) {
    aclSrv.setFull(false);
  }

  page: STPage = {
    front: false,
    total: true,
    zeroIndexed: true,
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
    status: null,
  };
  total = 0;

  loading = false;

  url = 'api/rules/index';
  req: STReq = { method: 'POST', allInBody: true, reName: { pi: 'offset', ps: 'limit' }, params: this.q };

  // 定义返回的参数
  res: STRes = {
    reName: {
      total: 'data.total',
      list: 'data.rows',
    },
  };

  @ViewChild('st', { static: true })
  st!: STComponent;
  columns: STColumn[] = [
    { title: '', index: 'ruleId', type: 'checkbox' },
    { title: 'id', index: 'ruleId' },
    { title: '规则名称', index: 'name', render: 'name' },
    { title: '备注', index: 'ruledesc' },
    { title: '创建时间', type: 'date', index: 'CreatTime' },
    {
      title: { i18n: 'i18n.columns.Status' },
      index: 'rulestatus',
      render: 'rulestatus',
      type: 'badge',
      badge: {
        0: { text: '禁用', color: 'error' },
        1: { text: '启用', color: 'success' },
      },
    },

    {
      title: '操作',
      buttons: [
        {
          acl: 9,
          text: '修改',
          click: (item: ruleflow) => {
            this.openComponent(item.ruleId);
          },
        },
        {
          text: (record) => (record.rulestatus == 1 ? '禁用' : '启用'),
          pop: {
            title: '确认修改规则状态?',
            okType: 'danger',
            icon: 'warning',
          },
          click: (item: ruleflow) => {


            //do something

          },
        },
        {
          text: '设计',
          //   acl: 104,
          click: (item: ruleflow) => {
            this._router.navigate(['/iot/flow/designer'], {
              queryParams: {
                Id: item.ruleId,
                type: 'clone',
              },
            });
          },
        },
        {
          text: '测试',
          //  acl: 104,

          click: (item: ruleflow) => {
            this.testthisflow(item);
          },
        },

        {
          text: '删除',
          //    acl: 104,
          pop: {
            title: '确认删除规则?',
            okType: 'danger',
            icon: 'warning',
          },
          click: (item: ruleflow) => {
            this.http.get('api/rules/delete?id=' + item.ruleId).subscribe(
              (x) => {
                this.getData();
              },
              (y) => {},
              () => {},
            );
          },
        },
      ],
    },
  ];
  selectedRows: STData[] = [];
  description = '';
  totalCallNo = 0;
  expandForm = false;

  ngOnInit() {}
  openComponent(id: number): void {
    var { nzMaskClosable, width } = this.settingService.getData('drawerconfig');
    var title = id == -1 ? '新建规则' : '修改规则';
    const drawerRef = this.drawerService.create<FlowformComponent, { id: number }, string>({
      nzTitle: title,
      nzContent: FlowformComponent,
      nzWidth: width,
      nzMaskClosable: nzMaskClosable,
      nzContentParams: {
        id: id,
      },
    });

    drawerRef.afterOpen.subscribe(() => {});

    drawerRef.afterClose.subscribe((data) => {
      this.st.load(this.st.pi);
      if (typeof data === 'string') {
      }
    });
  }

  testthisflow(ruleflow: ruleflow): void {
    var { nzMaskClosable, width } = this.settingService.getData('drawerconfig');

    var title = '测试' + ruleflow.name;
    const drawerRef = this.drawerService.create<FlowsimulatorComponent, { id: number }, string>({
      nzTitle: title,
      nzContent: FlowsimulatorComponent,
      nzWidth: width < 1280 ? 1280 : width,
      nzMaskClosable: nzMaskClosable,
      nzContentParams: {
        id: ruleflow.ruleId,
      },
    });

    drawerRef.afterOpen.subscribe(() => {});

    drawerRef.afterClose.subscribe((data) => {
      this.st.load(this.st.pi);
      if (typeof data === 'string') {
      }
    });
  }

  getData() {
    this.st.req = this.req;
    this.st.load(this.st.pi);
  }
  reset() {
    this.q ==
      {
        pi: 0,
        ps: 10,
        Name: '',
        Creator: '',
        CreatTime: [],
        sorter: '',
        status: null,
      };
  }
  setstatus(number: number, status: number) {}
}

export interface ruleflow {
  ruleId: number;
  name: string;
  ruledesc: string;
  CreatTime: Date;
  rulestatus: number;
  definitionsXml: string;
}
