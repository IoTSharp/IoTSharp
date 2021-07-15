import { ChangeDetectorRef, Component, EventEmitter, OnInit, TemplateRef, ViewChild } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { STChange, STColumn, STComponent, STData, STPage, STReq, STRes } from '@delon/abc/st';
import { ModalHelper, _HttpClient } from '@delon/theme';
import { NzMessageService } from 'ng-zorro-antd/message';
import { map, tap } from 'rxjs/operators';
import { Validators, FormBuilder, FormGroup } from '@angular/forms/forms';
import { NzDrawerRef, NzDrawerService } from 'ng-zorro-antd/drawer';

import { ACLService } from '@delon/acl';
import { Globals } from 'src/app/core/Globals';
import { DeviceformComponent } from '../deviceform/deviceform.component';
import { PropformComponent } from '../propform/propform.component';
import { zip } from 'rxjs';

@Component({
  selector: 'app-devicelist',
  templateUrl: './devicelist.component.html',
  styleUrls: ['./devicelist.component.less'],
})
export class DevicelistComponent implements OnInit {
  customerId: string = '';
  expand: any;
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
  ) {
    this.router.queryParams.subscribe(
      (x) => {
        this.q.customerId = x.id as unknown as string;
        this.customerId = x.id as unknown as string;

        if (x.id) {
          this.url = 'api/Devices/Customers/' + this.customerId;
        } else {
          this.url = 'api/Devices';
        }
      },
      (y) => {},
      () => {},
    );
  }
  url = 'api/Devices/Customers/' + this.customerId;

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
    customerId: '',
    name: '',
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
    { title: '名称', index: 'name', render: 'name' },
    { title: '设备类型', index: 'email' },
    { title: '所有者', index: 'phone' },
    { title: '租户', index: 'country' },
    { title: '客户', index: 'province' },
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
          acl: 11,
          text: '属性修改',
          click: (item: any) => {
            this.SetAttribute(item.id);
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

  ngOnInit(): void {
    this.router.queryParams.subscribe((x: any) => {
      this.q.customerId = x.customerId as string;
    });
  }

  edit(id: string): void {
    let title = id == '-1' ? '新建设备' : '修改设备';
    const drawerRef = this.drawerService.create<
      DeviceformComponent,
      {
        params: {
          id: string;
          customerId: string;
        };
      },
      any
    >({
      nzTitle: title,
      nzContent: DeviceformComponent,
      nzWidth: this.globals.drawerwidth,
      nzMaskClosable: this.globals.nzMaskClosable,
      nzContentParams: {
        params: {
          id: id,
          customerId: this.customerId,
        },
      },
    });
    drawerRef.afterOpen.subscribe(() => {
      this.getData();
    });
    drawerRef.afterClose.subscribe((data) => {});
  }

  SetAttribute(id: string): void {
    let title = '属性修改';
    const drawerRef = this.drawerService.create<
      PropformComponent,
      {
        params: {
          id: string;
          customerId: string;
        };
      },
      any
    >({
      nzTitle: title,
      nzContent: PropformComponent,
      nzWidth: this.globals.drawerwidth,
      nzMaskClosable: this.globals.nzMaskClosable,
      nzContentParams: {
        params: {
          id: id,
          customerId: this.customerId,
        },
      },
    });
    drawerRef.afterOpen.subscribe(() => {
      this.getData();
    });
    drawerRef.afterClose.subscribe((data) => {});
  }

  reset() {}
  delete(id: string) {
    this.http.delete('/api/Devices/' + id, {}).subscribe(
      (x) => {
        this.msg.info('设备已删除');
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

  onchange($events: STChange): void {
    switch ($events.type) {
      case 'expand':
        zip(
          this.http.get<attributeitem[]>('api/Devices/' + $events.expand?.id + '/AttributeLatest'),
          // this.http.get<telemetryitem[]>('api/Devices/' + $events.expand?.id + '/TelemetryLatest'),
        ).subscribe(([attributes, telemetries]) => {
          $events.expand.attributes = attributes;
          //  $events.expand.telemetries = telemetries;
          this.cdr.detectChanges();
        });

        break;
    }
  }
}

export interface deviceitem {
  deviceType: string;
  id: string;
  lastActive: string;
  name: string;
  online: string;
  owner: string;
  tenant: string;
  timeout: string;
  telemetries: telemetryitem[];
  attributes: attributeitem[];
}

export interface telemetryitem {
  keyName: string;
  dateTime: string;
  value: string;
}
export interface attributeitem {
  keyName: string;
  dataSide: string;
  dateTime: string;
  value: string;
}
