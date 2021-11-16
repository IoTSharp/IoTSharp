import { Component, OnInit, ViewChild } from '@angular/core';
import { Router } from '@angular/router';
import { STColumnTag, STPage, STReq, STRes, STComponent, STColumn, STData } from '@delon/abc/st';
import { _HttpClient, SettingsService } from '@delon/theme';
import { Guid } from 'guid-typescript';
import { NzDrawerService } from 'ng-zorro-antd/drawer';
import { NzMessageService } from 'ng-zorro-antd/message';
import { appmessage } from '../../common/AppMessage';
import { SubscriptionbindformComponent } from '../subscriptionbindform/subscriptionbindform.component';
import { SubscriptionformComponent } from '../subscriptionform/subscriptionform.component';

@Component({
  selector: 'app-subscriptionlist',
  templateUrl: './subscriptionlist.component.html',
  styleUrls: ['./subscriptionlist.component.less']
})
export class SubscriptionlistComponent implements OnInit {

  constructor(
    private http: _HttpClient,
    private msg: NzMessageService,
    private _router: Router,
    private drawerService: NzDrawerService,
    private settingService: SettingsService,

  ) {}

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

  url = 'api/subscriptionevent/index';
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
    { title: 'id', index: 'eventId' },
    { title: '事件名称', index: 'eventName', render: 'name' },
    { title: '命名空间', index: 'eventDesc' },
    { title: '命名空间', index: 'eventNameSpace' },

    
    { title: '创建时间', type: 'date', index: 'creatTime' },

    {
      title: { i18n: 'i18n.columns.Status' },
      index: 'ruleStatus',
      render: 'ruleStatus',
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
          click: (item: any) => {
            this.openComponent(item.ruleId);
          },
        },
        {
          text: (record) => (record.rulestatus == 1 ? '禁用' : '启用'),
          pop: {
            title: '确认修改订阅事件状态?',
            okType: 'danger',
            icon: 'warning',
          },
          click: () => {
            //do something
          },
        },

     
        {
          text: '订阅',
          //    acl: 104,
          pop: {
            title: '确认删除订阅事件?',
            okType: 'danger',
            icon: 'warning',
          },
          click: (item: any) => {
            this.http.get('api/subscriptionevent/delete?id=' + item.eventId).subscribe(
              () => {
                this.msg.create('success', '订阅事件删除成功');
                this.getData();
              },
              () => {   
                 this.msg.create('error', '订阅事件删除失败');
              this.getData();},
              () => {},
            );
          },
        },

        {
          text: '删除',
          //    acl: 104,
          pop: {
            title: '确认删除订阅事件?',
            okType: 'danger',
            icon: 'warning',
          },
          click: (item: any) => {
            this.http.get('api/subscriptionevent/delete?id=' + item.eventId).subscribe(
              () => {
                this.msg.create('success', '订阅事件删除成功');
                this.getData();
              },
              () => {   
                 this.msg.create('error', '订阅事件删除失败');
              this.getData();},
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
  openComponent(id: string): void {
    var { nzMaskClosable, width } = this.settingService.getData('drawerconfig');
    var title = id === Guid.EMPTY ? '新建事件' : '修改事件';
    const drawerRef = this.drawerService.create<SubscriptionformComponent, { id: string }, string>({
      nzTitle: title,
      nzContent: SubscriptionformComponent,
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

  onchange($event) {
    switch ($event.type) {
      case 'expand':
        if ($event.expand.expand) {
          this.http.get<appmessage<any>>('api/rules/GetFlows?ruleId=' + $event.expand?.ruleId).subscribe(
            (next) => {
              console.log(next);
              $event.expand.flows = next.data;
            },
            () => {},
            () => {},
          );
        }
        break;
    }
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
  setstatus() {}

}
