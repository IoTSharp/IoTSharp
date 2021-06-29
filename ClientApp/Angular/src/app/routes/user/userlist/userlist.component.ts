import { ChangeDetectorRef, Component, OnInit, TemplateRef, ViewChild } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { STChange, STColumn, STComponent, STData, STPage, STReq, STRes } from '@delon/abc/st';
import { ModalHelper, _HttpClient } from '@delon/theme';
import { NzMessageService } from 'ng-zorro-antd/message';
import { map, tap } from 'rxjs/operators';
import { Validators, FormBuilder, FormGroup } from '@angular/forms/forms';
import { NzDrawerRef, NzDrawerService } from 'ng-zorro-antd/drawer';

import { ACLService } from '@delon/acl';
import { Globals } from 'src/app/core/Globals';
import { UserformComponent } from '../userform/userform.component';

@Component({
  selector: 'app-userlist',
  templateUrl: './userlist.component.html',
  styleUrls: ['./userlist.component.less'],
})
export class UserlistComponent implements OnInit {
  customerId: string = '';
  url = '/api/Account/All';
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
  req: STReq = { method: 'GET', allInBody: true, reName: { pi: 'offset', ps: 'limit' }, params: this.q };

  // 定义返回的参数
  res: STRes = {
    reName: {
      total: 'total',
      list: 'rows',
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
    private modal: ModalHelper,
    private cdr: ChangeDetectorRef,
    private _router: Router,

    private router: ActivatedRoute,
    private drawerService: NzDrawerService,
    private globals: Globals,
    aclSrv: ACLService,
  ) {}

  ngOnInit(): void {
    this.router.queryParams.subscribe(
      (x) => {
        this.q.customerId = x.id as unknown as string;
        this.customerId = x.id as unknown as string;

        if (x.id) {
          this.url = 'api/Account/All/' + this.customerId;
        } else {
          this.url = 'api/Account/All';
        }
      },
      (y) => {},
      () => {},
    );
  }

  edit(id: string): void {
    let title = id == '-1' ? '新建设备' : '修改设备';
    const drawerRef = this.drawerService.create<UserformComponent, { id: string }, string>({
      nzTitle: title,
      nzContent: UserformComponent,
      nzWidth: this.globals.drawerwidth,
      nzMaskClosable: this.globals.nzMaskClosable,
      nzContentParams: {
        id: id,
      },
    });
    drawerRef.afterOpen.subscribe(() => {});
    drawerRef.afterClose.subscribe((data) => {
      this.getData();
    });
  }

  reset() {}
  delete(id: string) {
    this.http.delete('/api/Tenants/' + id, {}).subscribe(
      (x) => {
        this.msg.info('租户已删除');
        this.getData();
      },
      (y) => {},
      () => {},
    );
  }

  getData() {
    this.st.req = this.req;
    this.st.load(1);
  }
}
