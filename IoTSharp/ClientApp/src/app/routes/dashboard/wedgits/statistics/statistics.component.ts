import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { STColumn } from '@delon/abc/st';
import { G2TimelineMap } from '@delon/chart/timeline';
import { _HttpClient } from '@delon/theme';
import { getTimeDistance, toDate } from '@delon/util';
import { IWidgetComponent } from '../../v1/widgetcomponent';

@Component({
  selector: 'app-statistics',
  templateUrl: './statistics.component.html',
  styleUrls: ['./statistics.component.less'],
})
export class StatisticsComponent implements OnInit, IWidgetComponent {
  chartdata: any[] = [];
  data: any[] = [];
  titleMap: G2TimelineMap = { y1: 'publishFailed', y2: 'publishSuccessed', y3: 'subscribeFailed', y4: 'subscribeSuccessed' };
  eventcolumns: STColumn[] = [

    { title: 'id', index: 'id', render: 'name' },
    { title: '应用', index: 'name' },
    { title: '状态', index: 'status' },
  ]
  constructor(private http: _HttpClient, private cdr: ChangeDetectorRef,) {

  }
  ngOnInit(): void {
    let date: Date = new Date();
    this.http.get('http://localhost:5000/cap/api/metrics').subscribe(next => {
      for (var i = 0; i < next.dayHour.length; i++) {
        this.chartdata.push({
          time: toDate(date.getFullYear() + '-' + next.dayHour[i]),
          y1: next.publishFailed[i],
          y2: next.publishSuccessed[i],
          y3: next.subscribeFailed[i],
          y4: next.subscribeSuccessed[i],
        })
      }
      this.cdr.detectChanges();

    }, error => { }, () => { });

    this.http.get('http://localhost:5000/healthchecks-api').subscribe(next => {
      this.data = next[0].entries
      this.cdr.detectChanges();
    }, error => { }, () => { });
  }
}
