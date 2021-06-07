import { ChangeDetectorRef, Component, OnInit, TemplateRef, ViewChild } from '@angular/core';
import { Router } from '@angular/router';
import { STChange, STColumn, STComponent, STData, STPage, STReq, STRes } from '@delon/abc/st';
import { ModalHelper, _HttpClient } from '@delon/theme';
import { NzMessageService } from 'ng-zorro-antd/message';
import { map, tap } from 'rxjs/operators';
import { Validators, FormBuilder, FormGroup } from '@angular/forms/forms';
import { NzDrawerRef, NzDrawerService } from 'ng-zorro-antd/drawer';

import { ACLService } from '@delon/acl';
import { Globals } from 'src/app/core/Globals';
import { CustomerformComponent } from '../customerform/customerform.component';
@Component({
  selector: 'app-customerlist',
  templateUrl: './customerlist.component.html',
  styleUrls: ['./customerlist.component.less']
})
export class CustomerlistComponent implements OnInit {
  constructor(
    private http: _HttpClient,
    public msg: NzMessageService,
    private modal: ModalHelper,
    private cdr: ChangeDetectorRef,
    private _router: Router,
    private drawerService: NzDrawerService,
    private globals: Globals,
    aclSrv: ACLService,
  ) { }
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
    status: number | null;
  } = {
      pi: 0,
      ps: 10,
      name: '',
      sorter: '',
      status: null,
    };
  total = 0;
  data: any[] = [];
  loading = false;


  url = 'api/Customers/Tenant';
  req: STReq = { method: 'POST', allInBody: true, reName: { pi: 'offset', ps: 'limit' }, params: this.q };

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
    { title: '客户名称', index: 'name', render: 'name' },
    { title: '联系电话', index: 'email' },
    { title: '客户邮箱地址', index: 'phone' },
    { title: '国家', index: 'country' },
    { title: '省', index: 'province' },
    { title: '城市', index: 'city' },
    { title: '街道', index: 'street' },
    { title: '地址', index: 'address' },
    { title: '邮编', index: 'zipCode' },

    {
      title: '操作',
      buttons: [
        {
          acl: 9,
          text: '修改',
          click: (item: any) => {
            this.edit(item.UserId);
          },
        },


        {
          acl: 10,
          text: '删除',
          click: (item: any) => {

            this.delete(item.UserId);

          },
        },
      ],
    },
  ];
  selectedRows: STData[] = [];
  description = '';
  totalCallNo = 0;
  expandForm = false;
  ngOnInit(): void {
  }
  edit(id: string): void {
    let title = id == '-1' ? '新建客户' : '修改客户'
    const drawerRef = this.drawerService.create<CustomerformComponent, { id: string }, string>({
      nzTitle: title,
      nzContent: CustomerformComponent,
      nzWidth: this.globals.drawerwidth,
      nzMaskClosable: this.globals.nzMaskClosable,
      nzContentParams: {
        id: id,
      },
    });

    drawerRef.afterOpen.subscribe(() => { });

    drawerRef.afterClose.subscribe((data: any) => {


      this.getData()
    });
  }
  getData() {
    this.st.req = this.req;
    this.st.load(1);
  }

  reset() {

  }

  delete(id: string) {
    this.http.delete('/api/Customers/' + id, {}).subscribe(x => {

      this.getData()
    }, y => { }, () => { })

  }
}
