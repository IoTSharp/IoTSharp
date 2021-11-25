import { Component, OnInit } from '@angular/core';
import { IWidgetComponent } from '../../v1/widgetcomponent';
import { _HttpClient } from '@delon/theme';

@Component({
  selector: 'app-headerkanban',
  templateUrl: './headerkanban.component.html',
  styleUrls: ['./headerkanban.component.less']
})
export class HeaderkanbanComponent implements OnInit, IWidgetComponent {
  data: any = {
    devive: {
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
    warning: {
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
    talent: {
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
    customer: {
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
  constructor(private http: _HttpClient) {}

  ngOnInit(): void {
    this.http.get('api/rules/getexecutor').subscribe(
      next => {},
      error => {},
      () => {}
    );
  }
}
