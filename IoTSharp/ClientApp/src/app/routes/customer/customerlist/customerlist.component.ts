import { ChangeDetectionStrategy, ChangeDetectorRef, Component, OnInit, TemplateRef, ViewChild } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { STChange, STColumn, STComponent, STData, STPage, STReq, STRes } from '@delon/abc/st';
import { ModalHelper, SettingsService, _HttpClient } from '@delon/theme';
import { NzMessageService } from 'ng-zorro-antd/message';
import { map, tap } from 'rxjs/operators';
import { Validators, FormBuilder, FormGroup } from '@angular/forms/forms';
import { NzDrawerRef, NzDrawerService } from 'ng-zorro-antd/drawer';

import { ACLService } from '@delon/acl';

import { CustomerformComponent } from '../customerform/customerform.component';
@Component({
  selector: 'app-customerlist',
  templateUrl: './customerlist.component.html',

  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CustomerlistComponent implements OnInit {
  constructor(
    private http: _HttpClient,
    public msg: NzMessageService,
    private _router: Router,
    private router: ActivatedRoute,
    private drawerService: NzDrawerService,
    private settingService: SettingsService,
 
  ) {



  
  }


  ngOnInit(): void {

    console.log(   )
      
    this.router.queryParams.subscribe(
      (x) => {

        if( !x.id){

          this.q.tenantId= this.settingService.user.tenant
          this.tenantId = this.settingService.user.tenant;
        }else{
          this.q.tenantId = x.id as unknown as string;
          this.tenantId = x.id as unknown as string;
        

        }
        this.url = 'api/Customers/Tenant/' + this.tenantId;


      },
      () => {},
      () => {},
    );

  }
  tenantId: string = '';
  page: STPage = {
    front: false,
    total: true,
    zeroIndexed: true,
  };
  q: {
    tenantId: string;
    pi: number;
    ps: number;
    sorter: string;
    name: string;
    status: number | null;
  } = {
    tenantId: '',
    pi: 0,
    ps: 10,
    name: '',
    sorter: '',
    status: null,
  };
  total = 0;
  data: any[] = [];
  loading = false;

  url = 'api/Customers/Tenant/' + this.tenantId;
  req: STReq = { method: 'Post', allInBody: true, reName: { pi: 'offset', ps: 'limit' }, params: this.q };

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
          //  acl: 9,
          text: '修改',
          click: (item: any) => {
            this.edit(item.id);
          },
        },
        {
          //   acl: 9,
          text: '设备管理',
          click: (item: any) => {
            this._router.navigateByUrl('iot/device/devicelist?id=' + item.id);
          },
        },
        {
          // acl: 9,
          text: '人员管理',
          click: (item: any) => {
            this._router.navigateByUrl('iot/user/userlist?id=' + item.id);
          },
        },
        {
          //  acl: 10,

          pop: {
            title: '确认删除客户?',
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
  expandForm = false;

  edit(id: string): void {
    var { nzMaskClosable, width } = this.settingService.getData('drawerconfig');
    let title = id == '-1' ? '新建客户' : '修改客户';
    const drawerRef = this.drawerService.create<
      CustomerformComponent,
      {
        params: {
          id: string;
          tenantId: string;
        };
      },
      any
    >({
      nzTitle: title,
      nzContent: CustomerformComponent,
      nzWidth: width,
      nzMaskClosable: nzMaskClosable,
      nzContentParams: {
        params: {
          id: id,
          tenantId: this.tenantId,
        },
      },
    });

    drawerRef.afterOpen.subscribe(() => {});

    drawerRef.afterClose.subscribe(() => {
      this.getData();
    });
  }
  getData() {
    this.st.req = this.req;
    this.st.load(1);
  }

  reset() {}

  delete(id: string) {
    this.http.delete('api/Customers/' + id, {}).subscribe(
      () => {
        this.getData();
      },
      () => {},
      () => {},
    );
  }
}
