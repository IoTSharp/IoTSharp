import { Component, OnInit, ViewChild } from '@angular/core';
import { Router } from '@angular/router';
import { STColumn, STComponent, STData, STPage, STReq, STRes } from '@delon/abc/st';
import { SettingsService, _HttpClient } from '@delon/theme';
import { NzMessageService } from 'ng-zorro-antd/message';
import { NzDrawerService } from 'ng-zorro-antd/drawer';


import { TenantformComponent } from '../tenantform/tenantform.component';

@Component({
  selector: 'app-tenantlist',
  templateUrl: './tenantlist.component.html',
  styleUrls: ['./tenantlist.component.less'],
})
export class TenantlistComponent implements OnInit {
  url = 'api/Tenants';

  page: STPage = {
    front: false,
    total: true,
    zeroIndexed: true,
  };
  q: {
    pi: number;
    ps: number;
    sorter: string;

    name: string;
    // anothor query field:The type you expect
  } = {
    pi: 0,
    ps: 100,
    sorter: '',
    name: '',
  };
  req: STReq = { method: 'Get', allInBody: true, reName: { pi: 'offset', ps: 'limit' }, params: this.q };

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
    { title: '', index: 'id', type: 'checkbox' },
    { title: 'id', index: 'id' },
    { title: '名称', index: 'name', render: 'name' },
    { title: '邮件', index: 'email' },
    { title: '电话', index: 'phone' },
    { title: '国家', index: 'country' },
    { title: '省', index: 'province' },
    { title: '市', index: 'city' },
    { title: '街道', index: 'street' },
    { title: '地址', index: 'address' },
    { title: '邮编', index: 'zipCode' },
    {
      title: '操作',
      buttons: [
        {
          //    acl: 9,
          text: '修改',
          click: (item: any) => {
            this.edit(item.id);
          },
        },

        {
          // acl: 10,
          text: '客户管理',
          click: (item: any) => {
            this._router.navigateByUrl('iot/customer/customerlist?id=' + item.id);
          },
        },
        {
          //    acl: 10,

          pop: {
            title: '确认删除租户?',
            okType: 'danger',
            icon: 'delete',
          },
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
    private _router: Router,

    private drawerService: NzDrawerService,
    private settingService: SettingsService,
  ) {}

  ngOnInit(): void {}

  edit(id: string): void {
    let title = id == '-1' ? '新建租户' : '修改租户';
    var { nzMaskClosable, width } = this.settingService.getData('drawerconfig');
    const drawerRef = this.drawerService.create<TenantformComponent, { id: string }, string>({
      nzTitle: title,
      nzContent: TenantformComponent,
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
    this.http.delete('api/Tenants/' + id, {}).subscribe(
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
