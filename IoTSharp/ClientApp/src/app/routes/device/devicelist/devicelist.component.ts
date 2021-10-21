import { ChangeDetectorRef, Component, EventEmitter, OnInit, TemplateRef, ViewChild } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { STChange, STColumn, STComponent, STData, STPage, STReq, STRes } from '@delon/abc/st';
import { ModalHelper, SettingsService, _HttpClient } from '@delon/theme';
import { NzMessageService } from 'ng-zorro-antd/message';
import { map, tap } from 'rxjs/operators';
import { Validators, FormBuilder, FormGroup } from '@angular/forms/forms';
import { NzDrawerRef, NzDrawerService } from 'ng-zorro-antd/drawer';

import { ACLService } from '@delon/acl';

import { DeviceformComponent } from '../deviceform/deviceform.component';
import { PropformComponent } from '../propform/propform.component';
import { zip } from 'rxjs';
import { RulesdownlinkComponent } from '../rulesdownlink/rulesdownlink.component';
import { appmessage, AppMessage } from '../../common/AppMessage';

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
    private settingService: SettingsService,
    aclSrv: ACLService,
  ) {
    
  }
  url = 'api/Devices/Customers';

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
    { title: '设备类型', index: 'email' },
    { title: '所有者', index: 'phone' },
    { title: '租户', index: 'country' },
    { title: '客户', index: 'province' },
    {
      title: '操作',
      buttons: [
        {
          acl: 91,
          text: '修改',
          click: (item: any) => {
            this.edit(item.id);
          },
        },
        {
          acl: 111,
          text: '属性修改',
          click: (item: any) => {
            this.setAttribute(item.id);
          },
        },

        {
          acl: 111,
          text: '规则下发',
          click: (item: any) => {
            this.downlink([item]);
          },
        },
        {
          acl: 110,
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
  

    this.router.queryParams.subscribe(
      (x) => {
        if( !x.id){
          this.q.customerId= this.settingService.user.comstomer
          this.customerId = this.settingService.user.comstomer;


         this. url = 'api/Devices/Customers';
        }else{
          this.q.customerId = x.id as unknown as string;
          this.customerId = x.id as unknown as string;      this. url = 'api/Devices/Customers';
        }
      },
      (y) => {},
      () => {},
    );



  }

  downlink(dev: any[]) {
    console.log(dev);
    if (dev.length == 0) {
      dev = this.st.list.filter((c) => c.checked);
    }
    var { nzMaskClosable, width } = this.settingService.getData('drawerconfig');
    let title = '属性修改';
    const drawerRef = this.drawerService.create<
      RulesdownlinkComponent,
      {
        params: {
          id: string;
          customerId: string;
        };
      },
      any
    >({
      nzTitle: title,
      nzContent: RulesdownlinkComponent,
      nzWidth: width,
      nzMaskClosable: nzMaskClosable,
      nzContentParams: {
        params: {
          dev: dev,
        },
      },
    });
    drawerRef.afterOpen.subscribe(() => {
      this.getData();
    });
    drawerRef.afterClose.subscribe((data) => {});
  }
  edit(id: string): void {
    var { nzMaskClosable, width } = this.settingService.getData('drawerconfig');
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
      nzWidth: width,
      nzMaskClosable: nzMaskClosable,
      nzContentParams: {
        params: {
          id: id,
          customerId: this.customerId,
        },
      },
    });
    drawerRef.afterOpen.subscribe(() => {});
    drawerRef.afterClose.subscribe((data) => {
      this.getData();
    });
  }

  setAttribute(id: string): void {
    var { nzMaskClosable, width } = this.settingService.getData('drawerconfig');
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
      nzWidth: width,
      nzMaskClosable: nzMaskClosable,
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
          this.http.get<appmessage<attributeitem[]>>('api/Devices/' + $events.expand?.id + '/AttributeLatest'),
          this.http.get<appmessage<ruleitem[]>>('api/Rules/GetDeviceRules?deviceId=' + $events.expand?.id),
          // this.http.get<appmessage<telemetryitem[]>>('api/Devices/' + $events.expand?.id + '/TelemetryLatest'),
        ).subscribe(
          ([
            attributes,
            rules,
            //  telemetries
          ]) => {
            $events.expand.attributes = attributes.data;
            $events.expand.rules = rules.data;
            //  $events.expand.telemetries = telemetries;
            this.cdr.detectChanges();
          },
        );

        break;
    }
  }

  removerule(item:deviceitem, rule: ruleitem) {
    this.http.get('api/Rules/DeleteDeviceRules?deviceId='+item.id+'&ruleId='+rule.ruleId).subscribe(next=>{
      item.rules = item.rules.filter(x=>x.ruleId!=rule.ruleId);
    },error=>{},()=>{});

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
  rules: ruleitem[];
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

export interface ruleitem {
  ruleId: number;
  name: string;
  ruleDesc: string;
  describes: string;
}
