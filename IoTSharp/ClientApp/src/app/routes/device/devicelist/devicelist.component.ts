import { ChangeDetectorRef, Component, EventEmitter, OnDestroy, OnInit, TemplateRef, ViewChild } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { STChange, STColumn, STColumnBadge, STColumnTag, STComponent, STData, STPage, STReq, STRes } from '@delon/abc/st';
import { ModalHelper, SettingsService, _HttpClient } from '@delon/theme';
import { NzMessageService } from 'ng-zorro-antd/message';
import { map, tap } from 'rxjs/operators';
import { Validators, FormBuilder, FormGroup } from '@angular/forms/forms';
import { NzDrawerRef, NzDrawerService } from 'ng-zorro-antd/drawer';

import { ACLService } from '@delon/acl';

import { DeviceformComponent } from '../deviceform/deviceform.component';
import { PropformComponent } from '../propform/propform.component';
import { interval, Subscription, zip } from 'rxjs';
import { RulesdownlinkComponent } from '../rulesdownlink/rulesdownlink.component';
import { appmessage, AppMessage } from '../../common/AppMessage';
import { HttpHeaders, HttpResponse } from '@angular/common/http';
import { saveAs, fileSaver } from 'file-saver';
import { ClipboardService } from 'ngx-clipboard';
import { DevicetokendialogComponent } from '../devicetokendialog/devicetokendialog.component';
import { fork } from 'child_process';
import { ProppartComponent } from '../deviceprop/proppart/proppart.component';
import { devicemodelcommand } from '../../devicemodel/devicemodelcommandparam';
@Component({
  selector: 'app-devicelist',
  templateUrl: './devicelist.component.html',
  styleUrls: ['./devicelist.component.less']
})
export class DevicelistComponent implements OnInit, OnDestroy {
  BADGE: STColumnBadge = {
    true: { text: '在线', color: 'success' },
    false: { text: '离线', color: 'error' }
  };
  TAG: STColumnTag = {
    AccessToken: { text: 'AccessToken', color: 'green' },
    X509Certificate: { text: 'X509Certificate', color: 'blue' }
  };

  DeviceTAG: STColumnTag = {
    Device: { text: '设备', color: 'green' },
    Gateway: { text: '网关', color: 'blue' }
  };

  obs: Subscription;
  customerId: string = '';
  expand: any;
  constructor(
    private http: _HttpClient,
    public msg: NzMessageService,
    private router: ActivatedRoute,
    private drawerService: NzDrawerService,
    private settingService: SettingsService
  ) {}
  ngOnDestroy(): void {
    if (this.obs) {
      this.obs.unsubscribe();
    }
  }
  url = 'api/Devices/Customers';
  cetd: telemetryitem[] = [];
  cead: attributeitem[] = [];
  cerd: ruleitem[] = [];
 // cett: devicemodelcommand[] = [];
  page: STPage = {
    front: false,
    total: true,
    zeroIndexed: true
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
    name: ''
  };
  req: STReq = { method: 'GET', allInBody: true, reName: { pi: 'offset', ps: 'limit' }, params: this.q };

  // 定义返回的参数
  res: STRes = {
    reName: {
      total: 'data.total',
      list: 'data.rows'
    }
  };

  @ViewChild('st', { static: true })
  st!: STComponent;
  ctype = {};
  columns: STColumn[] = [
    { title: '', index: 'id', type: 'checkbox' },
    { title: 'id', index: 'id', render: 'id' },
    { title: '名称', index: 'name', render: 'name' },
    { title: '设备类型', index: 'deviceType', type: 'tag', tag: this.DeviceTAG },
    { title: '在线状态', index: 'online', type: 'badge', badge: this.BADGE },
    { title: '最后活动时间', index: 'lastActive', type: 'date' },
    { title: '认证方式', index: 'identityType', type: 'tag', tag: this.TAG },
    {
      title: '操作',
      type: 'link',
      render: 'download',
      buttons: [
        {
          acl: 91,
          text: '修改',
          click: (item: any) => {
            this.edit(item.id);
          }
        },
        {
          acl: 111,
          text: '属性修改',
          click: (item: any) => {
            this.setAttribute(item.id);
          }
        },
        {
          acl: 111,
          text: '新增属性',
          click: (item: any) => {
            this.addAttribute(item.id);
          }
        },
        {
          acl: 111,
          text: '设置规则',
          click: (item: any) => {
            this.downlink([item]);
          }
        },

        {
          acl: 111,
          text: '获取Token',
          type: 'modal',
          iif: record => record.identityType === 'AccessToken',
          modal: {
            component: DevicetokendialogComponent
          },
          click: () => {}
        },

        {
          acl: 111,
          text: '下载证书',
          iif: record => record.identityType === 'X509Certificate',

          click: record => {
            this.download(record);
          }
        },
        {
          acl: 110,
          text: '删除',
          click: (item: any) => {
            this.delete(item.id);
          }
        }
      ]
    }
  ];
  selectedRows: STData[] = [];
  description = '';
  totalCallNo = 0;
  getbuttons() {
    return [];
  }
  couponFormat() {}

  private download(record) {
    this.http
      .get(
        'api/Devices/' + record.id + '/DownloadCertificates',
        {},
        {
          responseType: 'blob'
        }
      )
      .subscribe(
        res => {
          let url = window.URL.createObjectURL(res);
          let a = document.createElement('a');
          document.body.appendChild(a);
          a.setAttribute('style', 'display: none');
          a.href = url;
          a.download = record.id + '.zip';
          a.click();
          window.URL.revokeObjectURL(url);
          a.remove();
        },
        error => {
          console.log(error);
          this.msg.create('error', '证书下载失败,请检查是否未生成');
        }
      );
  }

  ngOnInit(): void {
    this.router.queryParams.subscribe(
      x => {
        if (!x.id) {
          this.q.customerId = this.settingService.user.comstomer;
          this.customerId = this.settingService.user.comstomer;
          this.url = 'api/Devices/Customers';
        } else {
          this.q.customerId = x.id as unknown as string;
          this.customerId = x.id as unknown as string;
          this.url = 'api/Devices/Customers';
        }
      },
      () => {},
      () => {}
    );
  }

  downlink(dev: any[]) {
    console.log(dev);
    if (dev.length == 0) {
      dev = this.st.list.filter(c => c.checked);
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
          dev: dev
        }
      }
    });
    drawerRef.afterOpen.subscribe(() => {
      this.getData();
    });
    drawerRef.afterClose.subscribe(() => {});
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
          customerId: this.customerId
        }
      }
    });
    drawerRef.afterOpen.subscribe(() => {});
    drawerRef.afterClose.subscribe(() => {
      this.getData();
    });
  }

  addAttribute(id: string): void {
    var { nzMaskClosable, width } = this.settingService.getData('drawerconfig');
    let title = '增加属性';
    const drawerRef = this.drawerService.create<
      ProppartComponent,
      {
        params: {
          id: string;
          customerId: string;
        };
      },
      any
    >({
      nzTitle: title,
      nzContent: ProppartComponent,
      nzWidth: width,
      nzMaskClosable: nzMaskClosable,
      nzContentParams: {
        params: {
          id: id,
          customerId: this.customerId
        }
      }
    });

    drawerRef.afterClose.subscribe(() => {});
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
          customerId: this.customerId
        }
      }
    });
    drawerRef.afterOpen.subscribe(() => {});
    drawerRef.afterClose.subscribe(() => {});
  }

  reset() {}
  delete(id: string) {
    this.http.delete('api/Devices/' + id, {}).subscribe(
      () => {
        this.msg.create('success', '设备删除成功');
        this.getData();
      },
      () => {
        this.msg.create('error', '设备删除失败');
      },
      () => {}
    );
  }

  getData() {
    this.st.req = this.req;
    this.st.load(1);
  }

  onchange($events: STChange): void {
    if (this.obs) {
      this.obs.unsubscribe();
      this.obs = null;
    }
    switch ($events.type) {
      case 'expand':
        if ($events.expand.expand) {
          if (this.obs) {
            this.obs.unsubscribe();
          }
          this.cead = [];
          this.cetd = [];
          this.cerd = [];
          // this.cett = [];
          this.obs = interval(1000).subscribe(async () => {
            zip(
              this.http.get<appmessage<attributeitem[]>>('api/Devices/' + $events.expand?.id + '/AttributeLatest'),
              this.http.get<appmessage<ruleitem[]>>('api/Rules/GetDeviceRules?deviceId=' + $events.expand?.id),
              this.http.get<appmessage<telemetryitem[]>>('api/Devices/' + $events.expand?.id + '/TelemetryLatest')
              //   this.http.get<appmessage<devicemodelcommand[]>>('api/deviceModel/getCommandsByDevice?id=' + $events.expand?.id ),
            ).subscribe(
              ([
                attributes,
                rules,
                telemetries
                //  commands
              ]) => {
                // $events.expand.attributes = attributes.data;
                // $events.expand.rules = rules.data;
                // $events.expand.telemetries = telemetries.data;

                if (rules.data.length == 0) {
                  this.cerd = [];
                } else {
                  for (var i = 0; i < rules.data.length; i++) {
                    var index = this.cerd.findIndex(c => c.ruleId == rules.data[i].ruleId);
                    if (index === -1) {
                      this.cerd.push(rules.data[i]);
                    }
                  }

                  var removed: ruleitem[] = [];

                  for (var i = 0; i < this.cerd.length; i++) {
                    if (!rules.data.some(c => c.ruleId == this.cerd[i].ruleId)) {
                      removed = [...removed, this.cerd[i]];
                    }
                  }

                  for (var item of removed) {
                    this.cerd.slice(
                      this.cerd.findIndex(c => c.ruleId == item.ruleId),
                      1
                    );
                  }
                }

                if (this.cetd.length === 0) {
                  this.cetd = telemetries.data;
                } else {
                  for (var i = 0; i < telemetries.data.length; i++) {
                    var flag = false;
                    for (var j = 0; j < this.cetd.length; j++) {
                      if (telemetries.data[i].keyName == this.cetd[j].keyName) {
                        this.cetd[j].value = telemetries.data[i].value;
                        flag = true;
                      }
                    }
                    if (!flag) {
                      this.cetd.push(telemetries.data[i]);
                    }
                  }
                }

                if (this.cead.length === 0) {
                  this.cead = attributes.data;
                } else {
                  for (var i = 0; i < attributes.data.length; i++) {
                    var flag = false;
                    for (var j = 0; j < this.cead.length; j++) {
                      if (attributes.data[i].keyName == this.cead[j].keyName) {
                        this.cead[j].value = attributes.data[i].value;
                        flag = true;
                      }
                    }
                    if (!flag) {
                      this.cead.push(attributes.data[i]);
                    }
                  }
                }

                // if(this.cett.length==0){
                //   this.cett=commands.data;
                // }
              }
            );
          });
        } else {
          this.cead = [];
          this.cetd = [];
          this.cerd = [];
          // this.cett = [];
          if (this.obs) {
            this.obs.unsubscribe();
          }
        }

        break;
    }
  }

  executeCommand(item: deviceitem, command: devicemodelcommand) {
    this.http.post('api/Devices/' + item.identityId + '/Rpc/' + command.commandName + '?timeout=300', {}).subscribe(
      next => {},
      error => {},
      () => {}
    );
  }

  removerule(item: deviceitem, rule: ruleitem) {
    this.http.get('api/Rules/DeleteDeviceRules?deviceId=' + item.id + '&ruleId=' + rule.ruleId).subscribe(
      () => {
        this.cerd = this.cerd.filter(x => x.ruleId != rule.ruleId);
      },
      () => {},
      () => {}
    );
  }
}

export interface deviceitem {
  deviceType?: string;
  id?: string;
  lastActive?: string;
  name?: string;
  online?: string;
  owner?: string;
  tenant?: string;
  identityId?: string;
  timeout?: string;
  customerId?: string;
  telemetries?: telemetryitem[];
  attributes?: attributeitem[];
  rules?: ruleitem[];
  expand?: boolean;
  identityType?: string;
  identityValue?: string;
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
