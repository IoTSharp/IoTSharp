import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { STColumn, STColumnBadge, STColumnTag } from '@delon/abc/st';
import { _HttpClient } from '@delon/theme';
import { IWidgetComponent } from '../../v1/widgetcomponent';


@Component({
  selector: 'app-newdevice',
  templateUrl: './newdevice.component.html',
  styleUrls: ['./newdevice.component.less']
})
export class NewdeviceComponent implements OnInit, IWidgetComponent {
  EventTAG: STColumnTag = {
    'Normal': { text: '设备', color: 'green' },
    'TestPurpose': { text: '测试', color: 'orange' },

  };
  BADGE: STColumnBadge = {
    true: { text: '在线', color: 'success' },
    false: { text: '离线', color: 'error' },
  };
  TAG: STColumnTag = {
    AccessToken: { text: 'AccessToken', color: 'green' },
    X509Certificate: { text: 'X509Certificate', color: 'blue' },
  };
  devices: []
  devicecolumns: STColumn[] = [

    { title: '名称', index: 'name', render: 'name' },
    { title: '设备类型', index: 'deviceType', type: 'tag', tag: this.TAG },
    { title: '在线状态', index: 'online', type: 'badge', badge: this.BADGE },]
  events: []
  eventcolumns: STColumn[] =  [   { title: '事件名称', index: 'eventName', render: 'name' },
  { title: '类型', index: 'type',type:'tag',tag: this.EventTAG },
  { title: '触发规则', index: 'name' },
  { title: '事件源', index: 'creatorName' },]
  constructor(private http: _HttpClient, private cdr: ChangeDetectorRef,) { }

  ngOnInit(): void {

    this.http.get('api/home/toptendevice').subscribe(
      next => {
this.devices=next.data;
      },
      error => { },
      () => { }
    );

    this.http.get('api/home/toptenevents').subscribe(
      next => {
        this.events=next.data;
      },
      error => { },
      () => { }
    );
  }

}
