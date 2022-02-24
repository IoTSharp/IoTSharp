import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { STColumn } from '@delon/abc/st';
import { _HttpClient } from '@delon/theme';
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
  data: any[] = [];

  eventcolumns: STColumn[] =  [ 
    
    { title: 'id', index: 'id', render: 'name' },
  { title: '应用', index: 'name' },
  { title: '状态', index: 'status' },

]
  constructor(private http: _HttpClient, private cdr: ChangeDetectorRef,) {




  }

  ngOnInit(): void {

    this.http.get('http://localhost:5000/cap/api/metrics').subscribe(next=>{

      console.log(next);

      
      },error=>{},()=>{});
   
      this.http.get('http://localhost:5000/healthchecks-api').subscribe(next=>{

        this.data= next[0].entries

        },error=>{},()=>{});
  }
}
