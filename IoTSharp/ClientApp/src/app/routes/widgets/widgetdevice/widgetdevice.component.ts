import { Component, Input, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { STColumn, STColumnTag, STRes, STComponent } from '@delon/abc/st';
import { _HttpClient, SettingsService } from '@delon/theme';
import { Guid } from 'guid-typescript';
import { NzDrawerService } from 'ng-zorro-antd/drawer';
import { zip } from 'rxjs';
import { appmessage } from '../../common/AppMessage';
import { attributeitem, deviceitem, ruleitem, telemetryitem } from '../../device/devicemodel';
@Component({
  selector: 'app-widgetdevice',
  templateUrl: './widgetdevice.component.html',
  styleUrls: ['./widgetdevice.component.less']
})
export class WidgetdeviceComponent implements OnInit {
  @Input() id: string = Guid.EMPTY;
  device: any;

  cetd: telemetryitem[] = [];
  cead: attributeitem[] = [];
  cerd: ruleitem[] = [];
  // alarm

  @ViewChild('stalarm', { static: true })
  stalarm!: STComponent;
  alarmapi = 'api/alarm/list';
  erveritybadge: STColumnTag = {
    Indeterminate: { text: '不确定', color: '#8c8c8c' },
    Warning: { text: '警告', color: '#faad14' },
    Minor: { text: '次要', color: '#bae637' },
    Major: { text: '主要', color: '#1890ff' },
    Critical: { text: '错误', color: '#f5222d' }
  };

  alarmstatusTAG: STColumnTag = {
    Active_UnAck: { text: '激活未应答', color: '#ffa39e' },
    Active_Ack: { text: '激活已应答', color: '#f759ab' },
    Cleared_UnAck: { text: '清除未应答', color: '#87e8de' },
    Cleared_Act: { text: '清除已应答', color: '#d3f261' }
  };
  originatorTypeTAG: STColumnTag = {
    Unknow: { text: '未知', color: '#ffa39e' },
    Device: { text: '设备', color: '#f759ab' },
    Gateway: { text: '网关', color: '#87e8de' },
    Asset: { text: '资产', color: '#d3f261' }
  };

  serveritybadge: STColumnTag = {
    Indeterminate: { text: '不确定', color: '#8c8c8c' },
    Warning: { text: '警告', color: '#faad14' },
    Minor: { text: '次要', color: '#bae637' },
    Major: { text: '主要', color: '#1890ff' },
    Critical: { text: '错误', color: '#f5222d' }
  };

  columns: STColumn[] = [
    { title: '', index: 'Id', type: 'checkbox' },
    // { title: 'id', index: 'id' },
    {
      title: '告警类型',
      index: 'alarmType'
    },
    { title: '创建时间', index: 'ackDateTime', type: 'date' },
    { title: '警告持续的开始时间', index: 'startDateTime', type: 'date' },
    { title: '警告持续的结束时间', index: 'endDateTime', type: 'date' },
    { title: '清除时间', index: 'clearDateTime', type: 'date' },
    { title: '告警状态', index: 'alarmStatus', type: 'tag', tag: this.alarmstatusTAG },
    { title: '严重程度', index: 'serverity', type: 'tag', tag: this.serveritybadge },
    { title: '设备类型', index: 'originatorType', type: 'tag', tag: this.originatorTypeTAG }
  ];
  res: STRes = {
    reName: {
      total: 'data.total',
      list: 'data.rows'
    }
  };
  qal: {
    pi: number;
    ps: number;
    sorter: string;
    status: number | null;
    Name: string;
    AckDateTime: any;
    ClearDateTime: any;
    EndDateTime: any;
    StartDateTime: any;
    AlarmType: any;
    alarmStatus: any;
    OriginatorId: any;
    OriginatorName: string;
    serverity: any;
    originatorType: any;
  } = {
    pi: 0,
    ps: 10,
    Name: '',
    sorter: '',
    status: null,
    AckDateTime: null,
    ClearDateTime: null,
    StartDateTime: null,
    EndDateTime: null,
    AlarmType: '',
    OriginatorName: '',
    alarmStatus: '-1',
    OriginatorId: this.id,
    serverity: '-1',
    originatorType: '1'
  };
  alarmerq = { method: 'POST', allInBody: true, reName: { pi: 'offset', ps: 'limit' }, params: this.qal };

  //events
  eventsurl = 'api/rules/flowevents';

  qevent: {
    pi: number;
    ps: number;
    Name: string;
    Creator: string;
    RuleId: string;
    CreatorName: string;
    CreatTime: Date[];
    sorter: string;
    status: number | null;
  } = {
    pi: 0,
    ps: 10,
    Name: '',
    Creator: '',
    RuleId: '',
    CreatorName: '',
    CreatTime: [],
    sorter: '',
    status: null
  };
  eventreq = { method: 'POST', allInBody: true, reName: { pi: 'offset', ps: 'limit' }, params: this.qevent };
  eventtag: STColumnTag = {
    Normal: { text: '设备', color: 'green' },
    TestPurpose: { text: '测试', color: 'orange' }
  };

  @ViewChild('stevent', { static: true })
  stevent!: STComponent;
  eventcolumns: STColumn[] = [
    { title: '', index: 'eventId', type: 'checkbox' },
    { title: '事件名称', index: 'eventName', render: 'name' },
    { title: '类型', index: 'type', type: 'tag', tag: this.eventtag },
    { title: '触发规则', index: 'name' },
    { title: '事件源', index: 'creatorName' },
    { title: '创建时间', type: 'date', index: 'createrDateTime' }
  ];
  constructor(
    private _router: ActivatedRoute,
    private http: _HttpClient,
    private settingService: SettingsService,
    private drawerService: NzDrawerService
  ) {}

  ngOnInit(): void {
    this.getdevice();
    this.getdevicedata(this.id);
    this.getattrs(this.id);
  }

  getdevice() {
    this.http.get('api/Devices/' + this.id).subscribe(
      next => {
        this.device = next.data;
        this.qal.originatorType = this.device.deviceType == 'Gateway' ? '2' : '1';
        this.qal.OriginatorId = this.id;
        this.stalarm.req = this.alarmerq;
        this.stalarm.load(1);
        this.qevent.Creator = this.id;
        this.stevent.req = this.eventreq;
        this.stevent.load(1);
      },
      error => {},
      () => {}
    );
  }

  getattrs(deviceid) {
    this.http.get<appmessage<attributeitem[]>>('api/Devices/' + deviceid + '/AttributeLatest').subscribe(
      next => {
        if (this.cead.length === 0) {
          this.cead = next.data;
        } else {
          for (var i = 0; i < next.data.length; i++) {
            var flag = false;
            for (var j = 0; j < this.cead.length; j++) {
              if (next.data[i].keyName === this.cead[j].keyName) {
                switch (typeof next.data[i].value) {
                  case 'number':
                    if (this.cead[i]['value']) {
                      if (this.cead[i]['value'] > next.data[j]['value']) {
                        this.cead[i]['class'] = 'valdown';
                      } else if (this.cead[i]['value'] < next.data[j]['value']) {
                        this.cead[i]['class'] = 'valup';
                      } else {
                        this.cead[i]['class'] = 'valnom';
                      }
                    } else {
                      this.cead[i]['class'] = 'valnom';
                    }

                    break;
                  default:
                    if (this.cead[i]['value']) {
                      if (this.cead[i]['value'] === next.data[j]['value']) {
                        this.cead[i]['class'] = 'valnom';
                      } else {
                        this.cead[i]['class'] = 'valchange';
                      }
                    } else {
                      this.cead[i]['class'] = 'valnom';
                    }
                    break;
                }

                this.cead[j].value = next.data[i].value;
                flag = true;
              }
            }
            if (!flag) {
              next.data[i].class = 'valnew';
              this.cead.push(next.data[i]);
            }
          }
        }
      },
      error => {},
      () => {}
    );
  }

  gettemps(deviceid) {
    this.http.get<appmessage<attributeitem[]>>('api/Devices/' + deviceid + '/AttributeLatest').subscribe(
      next => {
        if (this.cetd.length === 0) {
          this.cetd = next.data;
        } else {
          for (var i = 0; i < next.data.length; i++) {
            var flag = false;
            for (var j = 0; j < this.cetd.length; j++) {
              if (next.data[i].keyName === this.cetd[j].keyName) {
                switch (typeof next.data[i].value) {
                  case 'number':
                    if (this.cetd[i]['value']) {
                      if (this.cetd[i]['value'] > next.data[j]['value']) {
                        this.cetd[i]['class'] = 'valdown';
                      } else if (this.cetd[i]['value'] < next.data[j]['value']) {
                        this.cetd[i]['class'] = 'valup';
                      } else {
                        this.cetd[i]['class'] = 'valnom';
                      }
                    } else {
                      this.cetd[i]['class'] = 'valnom';
                    }
                    break;
                  default:
                    if (this.cetd[i]['value']) {
                      if (this.cetd[i]['value'] === next.data[j]['value']) {
                        this.cetd[i]['class'] = 'valnom';
                      } else {
                        this.cetd[i]['class'] = 'valchange';
                      }
                    } else {
                      this.cetd[i]['class'] = 'valnom';
                    }
                    break;
                }
                this.cetd[j].value = next.data[i].value;
                flag = true;
              }
            }
            if (!flag) {
              next.data[i].class = 'valnew';
              this.cetd.push(next.data[i]);
            }
          }
        }
      },
      error => {},
      () => {}
    );
  }

  getrules(deviceid) {
    this.http.get<appmessage<ruleitem[]>>('api/Rules/GetDeviceRules?deviceId=' + deviceid).subscribe(
      next => {
        if (next.data.length == 0) {
          this.cerd = [];
        } else {
          for (var i = 0; i < next.data.length; i++) {
            var index = this.cerd.findIndex(c => c.ruleId == next.data[i].ruleId);
            if (index === -1) {
              this.cerd.push(next.data[i]);
            }
          }

          var removed: ruleitem[] = [];

          for (var i = 0; i < this.cerd.length; i++) {
            if (!next.data.some(c => c.ruleId == this.cerd[i].ruleId)) {
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
      error => {},
      () => {}
    );
  }

  getevetts() {}

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
                    if (this.cetd[i]['value']) {
                      if (this.cetd[i]['value'] > telemetries.data[j]['value']) {
                        this.cetd[i]['class'] = 'valdown';
                      } else if (this.cetd[i]['value'] < telemetries.data[j]['value']) {
                        this.cetd[i]['class'] = 'valup';
                      } else {
                        this.cetd[i]['class'] = 'valnom';
                      }
                    } else {
                      this.cetd[i]['class'] = 'valnom';
                    }
                    break;
                  default:
                    if (this.cetd[i]['value']) {
                      if (this.cetd[i]['value'] === telemetries.data[j]['value']) {
                        this.cetd[i]['class'] = 'valnom';
                      } else {
                        this.cetd[i]['class'] = 'valchange';
                      }
                    } else {
                      this.cetd[i]['class'] = 'valnom';
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
                    if (this.cead[i]['value']) {
                      if (this.cead[i]['value'] > attributes.data[j]['value']) {
                        this.cead[i]['class'] = 'valdown';
                      } else if (this.cead[i]['value'] < attributes.data[j]['value']) {
                        this.cead[i]['class'] = 'valup';
                      } else {
                        this.cead[i]['class'] = 'valnom';
                      }
                    } else {
                      this.cead[i]['class'] = 'valnom';
                    }

                    break;
                  default:
                    if (this.cead[i]['value']) {
                      if (this.cead[i]['value'] === attributes.data[j]['value']) {
                        this.cead[i]['class'] = 'valnom';
                      } else {
                        this.cead[i]['class'] = 'valchange';
                      }
                    } else {
                      this.cead[i]['class'] = 'valnom';
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

  removeprop(prop: attributeitem) {
    this.http
      .delete('api/devices/removeAttribute?deviceId=' + this.id + '&KeyName=' + prop.keyName + '&dataSide=' + prop.dataSide)
      .subscribe(
        () => {
          this.cead = this.cead.filter(x => x.keyName != prop.keyName);
        },
        () => {},
        () => {}
      );
  }

  removerule(item) {
    this.http.get('api/Rules/DeleteDeviceRules?deviceId=' + this.id + '&ruleId=' + item.ruleId).subscribe(
      () => {
        this.cerd = this.cerd.filter(x => x.ruleId != item.ruleId);
      },
      () => {},
      () => {}
    );
  }
}
