import { Component, OnInit, ViewChild } from '@angular/core';
import { STPage, STReq, STRes, STComponent, STColumn, STData, STColumnTag } from '@delon/abc/st';
import { _HttpClient, SettingsService } from '@delon/theme';
import { Guid } from 'guid-typescript';
import { NzDrawerService } from 'ng-zorro-antd/drawer';
import { NzMessageService } from 'ng-zorro-antd/message';
import { debounceTime } from 'rxjs/operators';
import { FloweventviewComponent } from '../floweventview/floweventview.component';


@Component({
  selector: 'app-flowevents',
  templateUrl: './flowevents.component.html',
  styleUrls: ['./flowevents.component.less']
})
export class FloweventsComponent implements OnInit {
  devices: creator[] = [{ id: Guid.EMPTY, name: '测试' }];
  dateFormat = 'yyyy/MM/dd'
  TAG: STColumnTag = {
    'Normal': { text: '设备', color: 'green' },
    'TestPurpose': { text: '测试', color: 'orange' },

  };
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
    RuleId: string;
    CreatorName: string;
    CreatTime: Date[];
    sorter: string;
    status: number | null;
  } = {
      pi: 0,
      ps: 10,
      Name: '',
      Creator: '',
      RuleId: '',
      CreatorName: '',
      CreatTime: [],
      sorter: '',
      status: null,
    };
  total = 0;

  loading = false;
  rules = [];
  url = 'api/rules/flowevents';
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
    { title: '', index: 'eventId', type: 'checkbox' },
    { title: '事件名称', index: 'eventName', render: 'name' },
    { title: '类型', index: 'type', type: 'tag', tag: this.TAG },
    { title: '触发规则', index: 'name' },
    { title: '事件源', index: 'creatorName' },
    { title: '创建时间', type: 'date', index: 'createrDateTime' },
    {
      title: '操作',
      buttons: [
        {
          acl: 9,
          text: '回放',
          click: (item: baseevent) => {
            this.openComponent(item);
          },
        },

      ],
    },
  ];
  selectedRows: STData[] = [];
  description = '';
  totalCallNo = 0;
  expandForm = false;
  openComponent(event: baseevent): void {
    var { nzMaskClosable, width } = this.settingService.getData('drawerconfig');
    var title = '回放';
    const drawerRef = this.drawerService.create<FloweventviewComponent, { event: baseevent }, string>({
      nzTitle: title,
      nzContent: FloweventviewComponent,
      nzWidth: 1080,
      nzMaskClosable: nzMaskClosable,
      nzContentParams: {
        event: event,
      },
    });

    drawerRef.afterOpen.subscribe(() => { });

    drawerRef.afterClose.subscribe((data) => {
      this.st.load(this.st.pi);
      if (typeof data === 'string') {
      }
    });
  }
  onInput($event: Event) {
    var element = $event.target as HTMLInputElement

    this.http.get('api/Devices/Customers', {
      limit: 20,
      offset: 0,
      customerId: this.settingService.user.comstomer, name: element?.value ?? ''
    }).pipe(debounceTime(500)).subscribe(next => {
      this.devices = [...next.data.rows.map(x => { return { id: x.id, name: x.name } }), { id: Guid.EMPTY, name: '测试' }]
    }, error => { }, () => { })

  }

  onChange($event){
this.q.Creator=this.devices.find(c=>c.name==$event)?.id
  }
  getData() {
    this.st.req = this.req;
    this.st.load(this.st.pi);
  }

  reset() {
    this.q =
    {
      pi: 0,
      ps: 10,
      Name: '',
      Creator: '',
      CreatorName: '',
      RuleId: '',
      CreatTime: [],
      sorter: '',
      status: null,
    };
  }
  constructor(
    private http: _HttpClient,
    public msg: NzMessageService,
    private drawerService: NzDrawerService,
    private settingService: SettingsService,

  ) {

  }

  ngOnInit(): void {
    this.http.post('api/rules/index', { offset: 0, limit: 100 }).subscribe(next => {
      this.rules = next.data.rows;
    }, error => { }, () => { })
  }

}

export interface baseevent {
  eventId: string;
  eventName: string;
  eventDesc: string;
  eventStaus: string;
  type: string;
  Creator: string;
  Bizid: string;
  name: string;
  ruleId: string;
  createrDateTime: string;
  creatorName: string;

}


export interface creator {
  id: string;
  name: string;
}