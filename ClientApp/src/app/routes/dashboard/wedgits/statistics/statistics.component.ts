import { Component, OnInit } from '@angular/core';
import { IWidgetComponent } from '../../v1/widgetcomponent';

@Component({
  selector: 'app-statistics',
  templateUrl: './statistics.component.html',
  styleUrls: ['./statistics.component.less'],
})
export class StatisticsComponent implements OnInit ,IWidgetComponent {
  offlineData: any[] = [];

  offlineChartData: any[] = [];
  salesData: any[] = [];
  constructor() {}

  ngOnInit(): void {
    for (let i = 0; i < 10; i += 1) {
      this.offlineData.push({
        name: `门店${i}`,
        cvr: Math.ceil(Math.random() * 9) / 10,
      });
    }

    for (let i = 0; i < 20; i += 1) {
      this.offlineChartData.push({
        time: new Date().getTime() + 1000 * 60 * 30 * i,
        y1: Math.floor(Math.random() * 100) + 10,
        y2: Math.floor(Math.random() * 100) + 10,
      });
    }

    for (let i = 0; i < 12; i += 1) {
      this.salesData.push({
        x: `${i + 1}月`,
        y: Math.floor(Math.random() * 1000) + 200,
      });
    }
  }
}
