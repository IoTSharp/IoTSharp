import { Component, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { STColumnBadge, STColumnTag, STPage, STReq, STRes, STComponent, STColumn, STData, STChange } from '@delon/abc/st';
import { _HttpClient, SettingsService } from '@delon/theme';
import { NzDrawerService } from 'ng-zorro-antd/drawer';
import { NzMessageService } from 'ng-zorro-antd/message';
import { interval, Subscription, zip } from 'rxjs';
import { appmessage } from 'src/app/models/appmessage';
import { attributeitem, deviceitem, devicemodelcommand, ruleitem, telemetryitem } from 'src/app/models/deviceitem';
import { CommonDialogSevice } from '../../util/commonDialogSevice';
import { DevicecertdialogComponent } from '../devicecertdialog/devicecertdialog.component';
import { DeviceformComponent } from '../deviceform/deviceform.component';
import { DevicepropComponent } from '../deviceprop/deviceprop.component';
import { DevicetokendialogComponent } from '../devicetokendialog/devicetokendialog.component';
import { ExporttoassetComponent } from '../exporttoasset/exporttoasset.component';
import { PropformComponent } from '../propform/propform.component';
import { RulesdownlinkComponent } from '../rulesdownlink/rulesdownlink.component';

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
  obsdevice: Subscription;
  customerId: string = '';
  expand: any;
  constructor(
    private http: _HttpClient,
    public msg: NzMessageService,
    private router: ActivatedRoute,
    private drawerService: NzDrawerService,
    private settingService: SettingsService,
    private commonDialogSevice: CommonDialogSevice
  ) {
    //console.log(commonDialogSevice)
  }
  ngOnDestroy(): void {
    if (this.obs) {
      this.obs.unsubscribe();
    }
    if (this.obsdevice) {
      this.obsdevice.unsubscribe();
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
    zeroIndexed: true,
    showSize: true,
    pageSizes: [10, 20, 50, 100, 500]
  };
  q: {
    pi: number;
    ps: number;
    sorter: string;
    onlyActive:boolean;
    customerId: string;
    name: string;
    // anothor query field:The type you expect
  } = {
      pi: 0,
      ps: 20,
      sorter: '',
      onlyActive:false,
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

  customColumns = [
    { label: 'id', value: 'id', checked: false },
    { label: '设备名称', value: 'name', checked: true },
    { label: '设备类型', value: 'deviceType', checked: true },
    { label: '在线状态', value: 'active', checked: true },
    { label: '最后活动时间', value: 'lastActivityDateTime', checked: true },
    { label: '认证方式', value: 'identityType', checked: true }
  ];
  columns: STColumn[] = [
    { title: '', index: 'id', type: 'checkbox' },
    { title: 'id', index: 'id', iif: () => this.isChoose('id') },
    {
      title: '名称',
      index: 'name',
      render: 'name',
      type: 'link',
      iif: () => this.isChoose('name'),
      click: (item: any) => {
        this.showdeviceDetail(item.id);
      }
    },
    { title: '设备类型', index: 'deviceType', type: 'tag', tag: this.DeviceTAG, iif: () => this.isChoose('deviceType') },
    { title: '在线状态', index: 'active', type: 'badge', badge: this.BADGE, iif: () => this.isChoose('active'), sort: true },
    { title: '最后活动时间', index: 'lastActivityDateTime', type: 'date', iif: () => this.isChoose('lastActivityDateTime'), sort: true },
    { title: '认证方式', index: 'identityType', type: 'tag', tag: this.TAG, iif: () => this.isChoose('identityType') },
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
          children: [
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
            }
          ]
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
          click: () => { }
        },

        {
          acl: 111,
          text: '下载证书', type: 'modal',
          iif: record => record.identityType === 'X509Certificate',
          modal: {
            component: DevicecertdialogComponent
          }, click: () => { }
          // click: record => {
          //   this.download(record);
          //  }
        },
        {
          acl: 110,
          text: '删除',
          pop: {
            title: '确认删除设备?',
            okType: 'danger',
            icon: 'warning'
          },
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
  couponFormat() { }
  isChoose(key: string): boolean {
    return !!this.customColumns.find(w => w.value === key && w.checked);
  }


  ngOnInit(): void {
    this.router.queryParams.subscribe(
      {

        next: x => {
          if (!x['id']) {
            this.q.customerId = this.settingService.user['customer'];
            this.customerId = this.settingService.user['customer'];
            this.url = 'api/Devices/Customers';
          } else {
            this.q.customerId = x['id'] as unknown as string;
            this.customerId = x['id'] as unknown as string;
            this.url = 'api/Devices/Customers';
          }
        },
        error: error => { },
        complete: () => { }
      }
    );
    this.getOnlineStatus()
    // this.obsdevice = interval(6000).subscribe(async () => {
    //   this.refreshData();
    // });
  }

  exporttoasset(dev: any[]) {
    if (dev.length == 0) {
      dev = this.st.list.filter(c => c.checked);
    }
    var { nzMaskClosable, width } = this.settingService.getData('drawerconfig');
    let title = '属性修改';
    const drawerRef = this.drawerService.create<
      ExporttoassetComponent,
      {
        params: {
          id: string;
          customerId: string;
        };
      },
      any
    >({
      nzTitle: title,
      nzContent: ExporttoassetComponent,
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
    drawerRef.afterClose.subscribe(() => { });
  }

  downlink(dev: any[]) {
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
    drawerRef.afterOpen.subscribe(() => { });
    drawerRef.afterClose.subscribe(() => {
      this.getData();
    });


  }
  edit(id: string): void {
    this.getOnlineStatus()
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
    drawerRef.afterOpen.subscribe(() => { });
    drawerRef.afterClose.subscribe(() => {
      this.getData();
    });
  }

  addAttribute(id: string): void {
    var { nzMaskClosable, width } = this.settingService.getData('drawerconfig');
    let title = '增加属性';
    const drawerRef = this.drawerService.create<
      DevicepropComponent,
      {
        params: {
          id: string;
          customerId: string;
        };
      },
      any
    >({
      nzTitle: title,
      nzContent: DevicepropComponent,
      nzWidth: width,
      nzMaskClosable: nzMaskClosable,
      nzContentParams: {
        params: {
          id: id,
          customerId: this.customerId
        }
      }
    });

    drawerRef.afterClose.subscribe(() => { });
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
    drawerRef.afterOpen.subscribe(() => { });
    drawerRef.afterClose.subscribe(() => { });
  }

  reset() { }
  delete(id: string) {
    this.http.delete('api/Devices/' + id, {}).subscribe(
      {
        next: next => {
          if (next?.data) {
            this.msg.create('success', '设备删除成功');
            this.getData();
          } else {
            this.msg.create('error', '错误：' + next?.msg, { nzDuration: 3000 });
          }
        },
        error: error => {
          this.msg.create('error', '设备删除失败:' + error, { nzDuration: 3000 });
        },
        complete: () => { }
      }
    );
  }

  getData() {
    this.st.req = this.req;
    this.st.load(this.st.pi);
  }



  getOnlineStatus() {
    if (this.st.list.length > 0) {
      this.http
        .post<appmessage<any>>('api/Devices/AttributeLatestByKeyNameAndDeviceId', {
          deviceIds: this.st.list.map(c => c['id']) ,
          keyNames: ['Active','LastActivityDateTime'],

        })
        .subscribe({
          next: next => {

            for (var item of next.data) {
              var data = this.st.list.findIndex(c => c['id'] == item['deviceId']);

              if(item['keyName']==='Active'){
                this.st.setRow(data, { online: item.value });
              }
              if(item['keyName']==='LastActivityDateTime'){
                this.st.setRow(data, { lastActive: item.value });
              }
            }
    
          }, error: error => { }, complete: () => { }

        });

    }

  }
  refreshData() {
    this.http
      .get<appmessage<any>>(this.url, {
        offset: this.st.pi - 1,
        limit: this.st.ps,
        sorter: '',
        customerId: this.q.customerId,
        name: this.q.name
      })
      .subscribe({

        next: next => {
          for (var item of next.data.rows) {
            var data = this.st.list.findIndex(c => c['Id'] == item.Id);
            this.st.setRow(data, { online: item.online, name: item.name });
          }
        },
        error: error => { }, complete: () => { }
      });
  }




  onchange($events: STChange): void {
    if (this.obs) {
      this.obs.unsubscribe();
      this.obs = null;
    }

    console.log($events.type)
    switch ($events.type) {
      case 'expand':
        if ($events.expand.expand) {
          //   this.getdevicedata($events.expand?.id);

          this.getattrs($events.expand?.id);
          this.gettemps($events.expand?.id);
          this.getrules($events.expand?.id);
          if (this.obs) {
            this.obs.unsubscribe();
          }
          // this.cead = [];
          this.cetd = [];
          // this.cerd = [];
          // this.cett =  [];
          this.obs = interval(6000).subscribe(async () => {
            this.gettemps($events.expand?.id);
          });
        } else {
          // this.cead = [];
          this.cetd = [];
          // this.cerd = [];
          // this.cett = [];
          if (this.obs) {
            this.obs.unsubscribe();
          }
        }
    

        break;


        case 'loaded':
//this.getOnlineStatus();
        break;
    }
  }

  getrules(deviceid) {
    this.http.get<appmessage<ruleitem[]>>('api/Rules/GetDeviceRules?deviceId=' + deviceid).subscribe(
      rules => {
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
      },
      error => { },
      () => { }
    );
  }

  gettemps(deviceid) {
    this.http.get<appmessage<telemetryitem[]>>('api/Devices/' + deviceid + '/TelemetryLatest').subscribe(
      {
        next: telemetries => {
          if (this.cetd.length === 0) {
            this.cetd = telemetries.data;
          } else {
            for (var i = 0; i < telemetries.data.length; i++) {
              var flag = false;
              for (var j = 0; j < this.cetd.length; j++) {
                if (telemetries.data[i].keyName === this.cetd[j].keyName) {
                  switch (typeof telemetries.data[i].value) {
                    case 'number':
                      if (this.cetd[j]['value']) {
                        if (this.cetd[j]['value'] > telemetries.data[i]['value']) {
                          this.cetd[j]['class'] = 'valdown';
                        } else if (this.cetd[j]['value'] < telemetries.data[i]['value']) {
                          this.cetd[j]['class'] = 'valup';
                        } else {
                          this.cetd[j]['class'] = 'valnom';
                        }
                      } else {
                        this.cetd[j]['class'] = 'valnom';
                      }
                      break;
                    default:
                      if (this.cetd[j]['value']) {
                        if (this.cetd[j]['value'] === telemetries.data[i]['value']) {
                          this.cetd[j]['class'] = 'valnom';
                        } else {
                          this.cetd[j]['class'] = 'valchange';
                        }
                      } else {
                        this.cetd[j]['class'] = 'valnom';
                      }
                      break;
                  }
                  this.cetd[j].value = telemetries.data[i].value;
                  this.cetd[j].dateTime = telemetries.data[i].dateTime;
                  flag = true;
                }
              }
              if (!flag) {
                telemetries.data[i].class = 'valnew';
                this.cetd.push(telemetries.data[i]);
              }
            }
          }
        },
        error: error => { },
        complete: () => { }
      }
    );
  }
  getattrs(deviceid) {
    this.http.get<appmessage<attributeitem[]>>('api/Devices/' + deviceid + '/AttributeLatest').subscribe(
      {


        next: attributes => {
          if (this.cead.length === 0) {
            this.cead = attributes.data;
          } else {
            for (var i = 0; i < attributes.data.length; i++) {
              var flag = false;
              for (var j = 0; j < this.cead.length; j++) {
                if (attributes.data[i].keyName === this.cead[j].keyName) {
                  // switch (typeof attributes.data[i].value) {
                  //   case 'number':
                  //     if (this.cead[j]['value']) {
                  //       if (this.cead[j]['value'] > attributes.data[i]['value']) {
                  //         this.cead[j]['class'] = 'valdown';
                  //       } else if (this.cead[j]['value'] < attributes.data[i]['value']) {
                  //         this.cead[j]['class'] = 'valup';
                  //       } else {
                  //         this.cead[j]['class'] = 'valnom';
                  //       }
                  //     } else {
                  //       this.cead[j]['class'] = 'valnom';
                  //     }

                  //     break;
                  //   default:
                  //     if (this.cead[j]['value']) {
                  //       if (this.cead[j]['value'] === attributes.data[i]['value']) {
                  //         this.cead[j]['class'] = 'valnom';
                  //       } else {
                  //         this.cead[j]['class'] = 'valchange';
                  //       }
                  //     } else {
                  //       this.cead[j]['class'] = 'valnom';
                  //     }
                  //     break;
                  // }

                  this.cead[j].value = attributes.data[i].value;
                  this.cead[j].dateTime = attributes.data[i].dateTime;
                  flag = true;
                }
              }
              if (!flag) {
                // attributes.data[i].class = 'valnew';
                this.cead.push(attributes.data[i]);
              }
            }
          }
        },
        error: error => { },
        complete: () => { }
      }
    );
  }
  executeCommand(item: deviceitem, command: devicemodelcommand) {
    this.http.post<appmessage<any>>('api/Devices/' + item.identityId + '/Rpc/' + command.commandName + '?timeout=300', {}).subscribe(
      {
        next: next => { },
        error: error => { },
        complete: () => { }
      }
    );
  }

  removerule(item: deviceitem, rule: ruleitem) {
    this.http.get<appmessage<any>>('api/Rules/DeleteDeviceRules?deviceId=' + item.id + '&ruleId=' + rule.ruleId).subscribe(
      {
        next: next => {
          this.cerd = this.cerd.filter(x => x.ruleId != rule.ruleId);
        },
        error: error => { },
        complete: () => { }
      }
    );
  }

  removeprop(device: deviceitem, prop: attributeitem) {
    this.http
      .delete('api/devices/removeAttribute?deviceId=' + device.id + '&KeyName=' + prop.keyName + '&dataSide=' + prop.dataSide)
      .subscribe(
        {
          next: next => {
            this.cead = this.cead.filter(x => x.keyName != prop.keyName);
          },
          error: error => { },
          complete: () => { }
        }
      );
  }

  showdeviceDetail(id) {
    this.commonDialogSevice.showDeviceDialog(id);

    //   var { nzMaskClosable, width } = this.settingService.getData('drawerconfig');
    //   let title = '设备详情';
    //   const drawerRef = this.drawerService.create<
    //     WidgetdeviceComponent,
    //     {
    //       id: string;
    //     },
    //     string
    //   >({
    //     nzTitle: title,
    //     nzContent: WidgetdeviceComponent,
    //     nzWidth: window.innerWidth*0.8,
    //     nzMaskClosable: nzMaskClosable,
    //     nzContentParams: {
    //       id: id
    //     }
    //   });
    //   drawerRef.afterOpen.subscribe(() => {

    //   });
    //   drawerRef.afterClose.subscribe(() => { });
  }

  getdevicedata(deviceid) {
    zip(
      this.http.get<appmessage<attributeitem[]>>('api/Devices/' + deviceid + '/AttributeLatest'),
      this.http.get<appmessage<ruleitem[]>>('api/Rules/GetDeviceRules?deviceId=' + deviceid),
      this.http.get<appmessage<telemetryitem[]>>('api/Devices/' + deviceid + '/TelemetryLatest')
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
              if (telemetries.data[i].keyName === this.cetd[j].keyName) {
                switch (typeof telemetries.data[i].value) {
                  case 'number':
                    if (this.cetd[j]['value']) {
                      if (this.cetd[j]['value'] > telemetries.data[i]['value']) {
                        this.cetd[j]['class'] = 'valdown';
                      } else if (this.cetd[j]['value'] < telemetries.data[i]['value']) {
                        this.cetd[j]['class'] = 'valup';
                      } else {
                        this.cetd[j]['class'] = 'valnom';
                      }
                    } else {
                      this.cetd[j]['class'] = 'valnom';
                    }
                    break;
                  default:
                    if (this.cetd[j]['value']) {
                      if (this.cetd[j]['value'] === telemetries.data[i]['value']) {
                        this.cetd[j]['class'] = 'valnom';
                      } else {
                        this.cetd[j]['class'] = 'valchange';
                      }
                    } else {
                      this.cetd[j]['class'] = 'valnom';
                    }
                    break;
                }
                this.cetd[j].value = telemetries.data[i].value;
                flag = true;
              }
            }
            if (!flag) {
              telemetries.data[i].class = 'valnew';
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
              if (attributes.data[i].keyName === this.cead[j].keyName) {
                switch (typeof attributes.data[i].value) {
                  case 'number':
                    if (this.cead[j]['value']) {
                      if (this.cead[j]['value'] > attributes.data[i]['value']) {
                        this.cead[j]['class'] = 'valdown';
                      } else if (this.cead[j]['value'] < attributes.data[i]['value']) {
                        this.cead[j]['class'] = 'valup';
                      } else {
                        this.cead[j]['class'] = 'valnom';
                      }
                    } else {
                      this.cead[j]['class'] = 'valnom';
                    }

                    break;
                  default:
                    if (this.cead[j]['value']) {
                      if (this.cead[j]['value'] === attributes.data[i]['value']) {
                        this.cead[j]['class'] = 'valnom';
                      } else {
                        this.cead[j]['class'] = 'valchange';
                      }
                    } else {
                      this.cead[j]['class'] = 'valnom';
                    }
                    break;
                }

                this.cead[j].value = attributes.data[i].value;
                flag = true;
              }
            }
            if (!flag) {
              attributes.data[i].class = 'valnew';
              this.cead.push(attributes.data[i]);
            }
          }
        }

        // if(this.cett.length==0){
        //   this.cett=commands.data;
        // }
      }
    );
  }
}
