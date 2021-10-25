import { ChangeDetectorRef, Component, OnInit, ViewChild } from '@angular/core';
import { Router } from '@angular/router';
import { STPage, STReq, STRes, STComponent, STColumn, STData, STColumnTag } from '@delon/abc/st';
import { _HttpClient, ModalHelper, SettingsService } from '@delon/theme';
import { NzDrawerService } from 'ng-zorro-antd/drawer';
import { NzMessageService } from 'ng-zorro-antd/message';
import { FloweventviewComponent } from '../floweventview/floweventview.component';
import { ruleflow } from '../flowlist/flowlist.component';

@Component({
  selector: 'app-flowevents',
  templateUrl: './flowevents.component.html',
  styleUrls: ['./flowevents.component.less']
})
export class FloweventsComponent implements OnInit {
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
    { title: '备注', index: 'eventDesc' },
    { title: '类型', index: 'type',type:'tag',tag: this.TAG },
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
    var title ='回放';
    const drawerRef = this.drawerService.create<FloweventviewComponent, { event: baseevent }, string>({
      nzTitle: title,
      nzContent: FloweventviewComponent,
      nzWidth: width,
      nzMaskClosable: nzMaskClosable,
      nzContentParams: {
        event: event,
      },
    });
  
    drawerRef.afterOpen.subscribe(() => {});

    drawerRef.afterClose.subscribe((data) => {
      this.st.load(this.st.pi);
      if (typeof data === 'string') {
      }
    });
  }

  constructor(
    private http: _HttpClient,
    public msg: NzMessageService,
    private modal: ModalHelper,
    private cdr: ChangeDetectorRef,
    private _router: Router,
    private drawerService: NzDrawerService,
    private settingService: SettingsService,
    
  ) {

  }

  ngOnInit(): void {
  }

}

export interface baseevent{
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
