import { Component, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { STColumn, STComponent, STData, STPage, STReq, STRes } from '@delon/abc/st';
import { SettingsService, _HttpClient } from '@delon/theme';
import { NzMessageService } from 'ng-zorro-antd/message';
import { NzDrawerService } from 'ng-zorro-antd/drawer';


import { UserformComponent } from '../userform/userform.component';

@Component({
  selector: 'app-userlist',
  templateUrl: './userlist.component.html',
  styleUrls: ['./userlist.component.less'],
})
export class UserlistComponent implements OnInit {
  customerId: string = '';
  url = 'api/Account/All/'+this.customerId;
  page: STPage = {
    front: false,
    total: true,
    zeroIndexed: true,
  };
  q: {
    pi: number;
    ps: number;
    sorter: string;
    customerId: string;
    name: string;
    // anothor query field:The type you expect
  } = {
    pi: 0,
    ps: 10,
    sorter: '',
    name: '',
    customerId: '',
  };
  req: STReq = { method: 'Get', allInBody: true, reName: { pi: 'offset', ps: 'limit' }, params: this.q };

  // 定义返回的参数
  res: STRes = {
    reName: {
      total: 'data',
      list: 'data',
    },
  };

  @ViewChild('st', { static: true })
  st!: STComponent;
  columns: STColumn[] = [
    { title: '', index: 'id', type: 'checkbox' },
    { title: 'id', index: 'id' },
    { title: '邮件', index: 'email' },
    { title: '电话', index: 'phoneNumber' },
    { title: '国家', index: 'accessFailedCount' },
    {
      title: '操作',
      buttons: [
        {
          acl: 9,
          text: '修改',
          click: (item: any) => {
            this.edit(item.id);
          },
        },
        {
          acl: 10,
          text: '删除',
          click: (item: any) => {
            this.delete(item.id);
          },
        },
      ],
    },
  ];
  selectedRows: STData[] = [];
  description = '';
  totalCallNo = 0;
  constructor(
    private http: _HttpClient,
    public msg: NzMessageService,
    private router: ActivatedRoute,
    private drawerService: NzDrawerService,
    private settingService: SettingsService,
  ) {}

  ngOnInit(): void {
    this.router.queryParams.subscribe(
      (x) => {


        if( !x.id){
          this.q.customerId= this.settingService.user.comstomer
          this.customerId = this.settingService.user.comstomer;
        }else{
          this.q.customerId = x.id as unknown as string;
          this.customerId = x.id as unknown as string;
        }
        this.url = 'api/Account/All/'+this.customerId;
        // this.q.customerId = x.id as unknown as string;
        // this.customerId = x.id as unknown as string;

        // if (x.id) {
        //   this.url = 'api/Account/All/' + this.customerId;
        // } else {
        //   this.url = 'api/Account/All';
        // }
      },
      () => {},
      () => {},
    );
  }

  edit(id: string): void {
    var { nzMaskClosable, width } = this.settingService.getData('drawerconfig');
    let title = id == '-1' ? '新建设备' : '修改设备';
    const drawerRef = this.drawerService.create<UserformComponent, { id: string }, string>({
      nzTitle: title,
      nzContent: UserformComponent,
      nzWidth: width,
      nzMaskClosable: nzMaskClosable,
      nzContentParams: {
        id: id,
      },
    });
    drawerRef.afterOpen.subscribe(() => {});
    drawerRef.afterClose.subscribe(() => {
      this.getData();
    });
  }

  reset() {}
  delete(id: string) {
    this.http.delete('/api/Tenants/' + id, {}).subscribe(
      () => {
        this.msg.info('租户已删除');
        this.getData();
      },
      () => {},
      () => {},
    );
  }

  getData() {
    this.st.req = this.req;
    this.st.load(1);
  }
}
