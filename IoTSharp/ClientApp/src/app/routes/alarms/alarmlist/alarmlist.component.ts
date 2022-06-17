import { ChangeDetectorRef, Component, OnInit, TemplateRef, ViewChild } from '@angular/core';
import { Router } from '@angular/router';
import { STPage, STReq, STRes, STComponent, STColumnTag, STColumn, STData } from '@delon/abc/st';
import { _HttpClient, ModalHelper, SettingsService } from '@delon/theme';
import { Guid } from 'guid-typescript';
import { NzDrawerService } from 'ng-zorro-antd/drawer';
import { NzMessageService } from 'ng-zorro-antd/message';
import { debounceTime } from 'rxjs';
import { appmessage } from 'src/app/models/appmessage';

@Component({
  selector: 'app-alarmlist',
  templateUrl: './alarmlist.component.html',
  styleUrls: ['./alarmlist.component.less']
})
export class AlarmlistComponent implements OnInit {
  originators: originator[] = [];
  page: STPage = {
    front: false,
    total: true,
    zeroIndexed: true
  };
  q: {
    pi: number;
    ps: number;
    sorter: string;
    status: number | null;
    Name: string;
    AckDateTime: any;
    ClearDateTime: any;
    EndDateTime: any;
    StartDateTime: any;
    AlarmType: any;
    alarmStatus: any;
    OriginatorId: any;
    OriginatorName: string;
    serverity: any;
    originatorType: any;
  } = {
    pi: 0,
    ps: 10,
    Name: '',
    sorter: '',
    status: null,
    AckDateTime: null,
    ClearDateTime: null,
    StartDateTime: null,
    EndDateTime: null,
    AlarmType: '',
    OriginatorName: '',
    alarmStatus: '-1',
    OriginatorId: Guid.EMPTY,
    serverity: '-1',
    originatorType: '-1'
  };

  total = 0;
  data: any[] = [];
  loading = false;
  status = [
    { index: 1, text: '正常', value: false, type: 'default', checked: false },
    {
      index: 2,
      text: '已禁用',
      value: false,
      type: 'processing',
      checked: false
    },
    {
      index: -1,
      text: '已删除',
      value: false,
      type: 'processing',
      checked: false
    }
  ];

  url = 'api/alarm/list';
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

  serveritybadge: STColumnTag = {
    Indeterminate: { text: '不确定', color: '#8c8c8c' },
    Warning: { text: '警告', color: '#faad14' },
    Minor: { text: '次要', color: '#bae637' },
    Major: { text: '主要', color: '#1890ff' },
    Critical: { text: '错误', color: '#f5222d' }
  };

  alarmstatusTAG: STColumnTag = {
    Active_UnAck: { text: '激活未应答', color: '#ffa39e' },
    Active_Ack: { text: '激活已应答', color: '#f759ab' },
    Cleared_UnAck: { text: '清除未应答', color: '#87e8de' },
    Cleared_Act: { text: '清除已应答', color: '#7cb305' }
  };
  originatorTypeTAG: STColumnTag = {
    Unknow: { text: '未知', color: '#ffa39e' },
    Device: { text: '设备', color: '#f759ab' },
    Gateway: { text: '网关', color: '#87e8de' },
    Asset: { text: '资产', color: '#d3f261' }
  };

  ServerityTAG: STColumnTag = {
    AccessToken: { text: 'AccessToken', color: 'green' },
    X509Certificate: { text: 'X509Certificate', color: 'blue' }
  };

  columns: STColumn[] = [
    { title: '', index: 'Id', type: 'checkbox' },
    // { title: 'id', index: 'id' },
    {
      title: '告警类型',
      index: 'alarmType'
    },
    { title: '创建时间', index: 'ackDateTime', type: 'date' },
    { title: '警告持续的开始时间', index: 'startDateTime', type: 'date' },
    { title: '警告持续的结束时间', index: 'endDateTime', type: 'date' },
    { title: '清除时间', index: 'clearDateTime', type: 'date' },
    { title: '告警状态', index: 'alarmStatus', type: 'tag', tag: this.alarmstatusTAG },
    { title: '严重程度', index: 'serverity', type: 'tag', tag: this.serveritybadge },

    { title: '设备类型', index: 'originatorType', type: 'tag', tag: this.originatorTypeTAG },

    {
      title: '操作',
      index: 'Id',
      buttons: [
        {
          text: '确认告警',
          pop: {
            title: '确认告警?',
            okType: 'primary',
            icon: 'warning'
          },
          click: (item: any) => {
            this.acquireAlarm(item);
          },
          iif: record => record.alarmStatus === 'Active_UnAck' || record.alarmStatus === 'Cleared_UnAck'
        },
        {
          text: '清除告警',
          pop: {
            title: '清除告警?',
            okType: 'dashed',
            icon: 'warning'
          },
          iif: record => record.alarmStatus === 'Active_Ack' || record.alarmStatus === 'Active_UnAck',
          click: (item: any) => {
            this.clearAlarm(item);
          }
        }
      ]
    }
  ];
  selectedRows: STData[] = [];
  description = '';
  totalCallNo = 0;
  expandForm = false;
  constructor(
    private http: _HttpClient,
    public msg: NzMessageService,
    private modal: ModalHelper,
    private cdr: ChangeDetectorRef,
    private _router: Router,
    private drawerService: NzDrawerService,
    private settingService: SettingsService
  ) {}

  ngOnInit() {}

  onOriginatorTypeChange($event) {
    this.q.OriginatorName = '';
    this.q.OriginatorId = Guid.EMPTY;
    this.originators = [];
  }

  onOriginatorInput($event) {
    var element = $event.target as HTMLInputElement;
    this.http
      .post<appmessage<any>>('api/alarm/originators', {
        originatorName: element?.value ?? '',
        OriginatorType: this.q.originatorType
      })
      .pipe(debounceTime(500))
      .subscribe({
        next: next => {
          this.originators = [
            ...next.data.map(x => {
              return { id: x.id, name: x.name };
            })
          ];
        },
        error: error => {},
        complete: () => {}
      });
  }
  //
  clearAlarm(item) {
    this.http
      .post<appmessage<Boolean>>('api/alarm/clearAlarm', {
        id: item.id
      })
      .subscribe({
        next: next => {
          if (next?.data) {
            this.msg.create('success', '警告清除成功');
            this.getData();
          } else {
            this.msg.create('error', '警告清除异常:' + next.msg);
          }
        },
        error: error => {
          this.msg.create('error', '警告清除异常');
        },
        complete: () => {}
      });
  }

  acquireAlarm(item) {
    this.http
      .post<appmessage<Boolean>>('api/alarm/ackAlarm', {
        id: item.id
      })
      .subscribe({
        next: next => {
          if (next?.data) {
            this.msg.create('success', '警告确认成功');
            this.getData();
          } else {
            this.msg.create('error', '警告确认异常:' + next.msg);
          }
        },
        error: error => {
          this.msg.create('error', '警告确认异常');
        },
        complete: () => {}
      });
  }

  onOriginatorChange($event) {
    this.q.OriginatorId = this.originators.find(c => c.name == $event)?.id;
  }

  getData() {
    this.st.req = this.req;
    this.st.load(this.st.pi);
  }

  add(tpl: TemplateRef<{}>) {}

  reset() {
    setTimeout(() => {}, 1000);
  }
  setstatus(number: number, status: number) {}
}

export interface originator {
  id: string;
  name: string;
}
