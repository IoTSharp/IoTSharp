import { Component, Input, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { STColumn, STColumnTag, STRes } from '@delon/abc/st';
import { _HttpClient, SettingsService } from '@delon/theme';
import { Guid } from 'guid-typescript';
import { NzDrawerService } from 'ng-zorro-antd/drawer';

@Component({
  selector: 'app-widgetdevice',
  templateUrl: './widgetdevice.component.html',
  styleUrls: ['./widgetdevice.component.less']
})
export class WidgetdeviceComponent implements OnInit {
  @Input() id: string = Guid.EMPTY;
  device: any;


  // alarm


  alarmapi='api/alarm/list'
  erveritybadge: STColumnTag = {
    'Indeterminate': { text: '不确定', color: '#8c8c8c' },
    'Warning': { text: '警告', color: '#faad14' },
    'Minor': { text: '次要', color: '#bae637' },
    'Major': { text: '主要', color: '#1890ff' },
    'Critical': { text: '错误', color: '#f5222d' }
  };

  alarmstatusTAG: STColumnTag = {
    'Active_UnAck': { text: '激活未应答', color: '#ffa39e' },
    'Active_Ack': { text: '激活已应答', color: '#f759ab' },
    'Cleared_UnAck': { text: '清除未应答', color: '#87e8de' },
    'Cleared_Act': { text: '清除已应答', color: '#d3f261' }
  };
  originatorTypeTAG: STColumnTag = {
    'Unknow': { text: '未知', color: '#ffa39e' },
    'Device': { text: '设备', color: '#f759ab' },
    'Gateway': { text: '网关', color: '#87e8de' },
    'Asset': { text: '资产', color: '#d3f261' }
  };

  serveritybadge: STColumnTag = {
    'Indeterminate': { text: '不确定', color: '#8c8c8c' },
    'Warning': { text: '警告', color: '#faad14' },
    'Minor': { text: '次要', color: '#bae637' },
    'Major': { text: '主要', color: '#1890ff' },
    'Critical': { text: '错误', color: '#f5222d' }
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
    { title: '设备类型', index: 'originatorType', type: 'tag', tag: this.originatorTypeTAG },
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
    }

  constructor(
    private _router: ActivatedRoute,
    private http: _HttpClient,
    private settingService: SettingsService,
    private drawerService: NzDrawerService
  ) {}

  ngOnInit(): void {
    this.http.get('api/Devices/' + this.id).subscribe(
      next => {
        this.device = next.data;
      },
      error => {},
      () => {}
    );
  }
}
