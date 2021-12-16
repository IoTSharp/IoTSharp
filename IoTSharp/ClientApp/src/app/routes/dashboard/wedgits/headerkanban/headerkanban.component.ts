import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { IWidgetComponent } from '../../v1/widgetcomponent';
import { _HttpClient } from '@delon/theme';
import { ThisReceiver } from '@angular/compiler';

@Component({
  selector: 'app-headerkanban',
  templateUrl: './headerkanban.component.html',
  styleUrls: ['./headerkanban.component.less']
})
export class HeaderkanbanComponent implements OnInit, IWidgetComponent {
  data:any= {
    device: {
      count: 11,
      data: [
        { x: 1, y: 1 },
        { x: 2, y: 2 },
        { x: 3, y: 3 },
        { x: 4, y: 4 },
        { x: 5, y: 5 },
        { x: 6, y: 6 }
      ]
    },
    onlinedevice: {
      count: 11,
      data: [
        { x: 1, y: 7 },
        { x: 2, y: 6 },
        { x: 3, y: 5 },
        { x: 4, y: 4 },
        { x: 5, y: 3 },
        { x: 6, y: 2 }
      ]
    },
    telemetrydata: {
      count: 11,
      data: [
        { x: 1, y: 1 },
        { x: 2, y: 2 },
        { x: 3, y: 3 },
        { x: 4, y: 4 },
        { x: 5, y: 5 },
        { x: 6, y: 6 }
      ]
    },
    events: {
      count: 11,
      data: [
        { x: 1, y: 1 },
        { x: 2, y: 2 },
        { x: 3, y: 3 },
        { x: 4, y: 4 },
        { x: 5, y: 5 },
        { x: 6, y: 6 }
      ]
    }
  };
  constructor(private http: _HttpClient , private cdr: ChangeDetectorRef, ) {}

  ngOnInit(): void {
    this.http.get('api/home/kanban').subscribe(
      next => {

        this.data.device.count=next.data.deviceCount
        this.data.onlinedevice.count=next.data.onlineDeviceCount
        this.data.telemetrydata.count=next.data.telemetryDataCount
        this.data.events.count=next.data.eventCount
        this.cdr.detectChanges();
      },
      error => {},
      () => {}
    );
  }
}
