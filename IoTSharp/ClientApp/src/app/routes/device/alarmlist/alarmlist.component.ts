import { ChangeDetectorRef, Component, OnInit, TemplateRef, ViewChild } from '@angular/core';
import { Router } from '@angular/router';
import { STPage, STReq, STRes, STComponent, STColumnTag, STColumn, STData, STColumnBadge } from '@delon/abc/st';
import { _HttpClient, ModalHelper, SettingsService } from '@delon/theme';
import { Guid } from 'guid-typescript';
import { NzDrawerService } from 'ng-zorro-antd/drawer';
import { NzMessageService } from 'ng-zorro-antd/message';
import { debounceTime } from 'rxjs/operators';
import { AppMessage } from '../../common/AppMessage';
import { AlarmdetailComponent } from '../alarmdetail/alarmdetail.component';

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
    AlarmStatus: Number;
    OriginatorId: any;
    OriginatorName: string;
    Serverity: Number;
    OriginatorType: Number;
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
    AlarmStatus: -1,
    OriginatorId: Guid.EMPTY,
    Serverity: -1,
    OriginatorType: -1
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
    0: { text: '不确定', color: '#8c8c8c' },
    1: { text: '警告', color: '#faad14' },
    2: { text: '次要', color: '#bae637' },
    3: { text: '主要', color: '#1890ff' },
    4: { text: '错误', color: '#f5222d' }
  };

  alarmstatusTAG: STColumnTag = {
    0: { text: '激活未应答', color: '#ffa39e' },
    1: { text: '激活已应答', color: '#f759ab' },
    2: { text: '清除未应答', color: '#87e8de' },
    3: { text: '清除已应答', color: '#d3f261' }
  };
  ServerityTAG: STColumnTag = {
    AccessToken: { text: 'AccessToken', color: 'green' },
    X509Certificate: { text: 'X509Certificate', color: 'blue' }
  };

  columns: STColumn[] = [
    { title: '', index: 'Id', type: 'checkbox' },
    { title: 'id', index: 'id' },
    {
      title: '类型',
      index: 'AlarmType'
    },
    { title: '创建时间', index: 'ackDateTime', type: 'date' },
    { title: '清除时间', index: 'clearDateTime', type: 'date' },
    { title: '警告持续的开始时间', index: 'startDateTime', type: 'date' },
    { title: '结束时间', index: 'endDateTime', type: 'date' },
    { title: '告警状态', index: 'alarmStatus', type: 'tag', tag: this.alarmstatusTAG },
    { title: '严重程度', index: 'serverity', type: 'tag', tag: this.serveritybadge },

    { title: '设备类型', index: 'originatorType' },

    {
      title: { i18n: 'table.operation' },
      buttons: [
        {
          text: '修改',
          i18n: 'common.edit',
          acl: 60,
          click: (item: any) => {
            this.openComponent(item.dictionaryGroupId);
            //this._router.navigate(['manage/role/roleform'],
            //  {
            //    queryParams: {
            //      UserId: item.UserId,
            //      type: 'clone'
            //    }
            //  });
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
    this.originators=[];
  }

  onOriginatorInput($event) {
    var element = $event.target as HTMLInputElement;

    this.http
      .post('api/alarm/originators', {
        originatorName: element?.value ?? '',
        OriginatorType: this.q.OriginatorType
      })
      .pipe(debounceTime(500))
      .subscribe(
        next => {
          this.originators = [
            ...next.data.map(x => {
              return { id: x.id, name: x.name };
            })
          ];
        },
        error => {},
        () => {}
      );
  }

  onOriginatorChange($event) {
    this.q.OriginatorId = this.originators.find(c => c.name == $event)?.id;
  }

  rowchange(event) {
    switch (event.type) {
      case 'expand':
        this.http
          .post<AppMessage>('api/dictionary/index', {
            DictionaryGroupId: event.expand.dictionaryGroupId,
            offset: 0,
            limit: 100
          })
          .subscribe(
            x => {
              event.expand.Children = x.data.rows;
            },
            y => {},
            () => {}
          );
        break;
    }
  }
  openComponent(id: Number): void {
    var { nzMaskClosable, width } = this.settingService.getData('drawerconfig');
    var title = id == -1 ? '新增字典分组' : '修改字典分组';
    const drawerRef = this.drawerService.create<AlarmdetailComponent, { id: Number }, string>({
      nzTitle: title,
      nzContent: AlarmdetailComponent,
      nzWidth: width,
      nzMaskClosable: nzMaskClosable,
      nzContentParams: {
        id: id
      }
    });

    drawerRef.afterOpen.subscribe(() => {});

    drawerRef.afterClose.subscribe(data => {
      if (typeof data === 'string') {
      }

      this.getData();
    });
  }

  r;
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
