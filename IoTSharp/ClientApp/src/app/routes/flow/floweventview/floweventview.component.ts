import { Component, Input, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { EChartsOption } from 'echarts';
import { interval, Subscription } from 'rxjs';
import { concat, } from 'rxjs'; import { map } from 'rxjs/operators';
import { appmessage } from '../../common/AppMessage';
import { FlowviewerComponent } from '../../widgets/flowviewer/flowviewer.component';
import { baseevent } from '../flowevents/flowevents.component';
import { flowrule } from '../flowlist/flowlist.component';

@Component({
  selector: 'app-floweventview',
  templateUrl: './floweventview.component.html',
  styleUrls: ['./floweventview.component.less']
})
export class FloweventviewComponent implements OnInit, OnDestroy {
  @Input()
  event: baseevent;
  listOfOption = [];
  obs: Subscription;
  steps: any = [];
  nodes = [];
  @ViewChild('flowview', { static: true })
  flowview: FlowviewerComponent;
  current: 0;
  option: EChartsOption;
  constructor(private http: _HttpClient,) { }

  ngOnDestroy(): void {
    if (this.obs) {
      this.obs.unsubscribe();
    }
  }

  ngOnInit(): void {
    concat(this.http.get<appmessage<flowrule>>('api/rules/get?id=' + this.event.ruleId).pipe(
      map((x) => {
        this.flowview.diagramdata = x.data;
        this.flowview.loadXml();
      }),
    ), this.http.get('api/rules/GetFlowOperations?eventId=' + this.event.eventId).pipe(
      map((x) => {

        if(x.data&&x.data.steps.length>0){
          this.nodes = x.data.steps;
          this.option = {
            // title: {
            //   text: ''
            // },
            tooltip: {
              trigger: 'item',
              triggerOn: 'mousemove'
            },
            series: [
              {
                type: 'sankey',
                data: x.data.charts.sankey.nodes,
                links: x.data.charts.sankey.links,
                emphasis: {
                  focus: 'adjacency'
                },
                lineStyle: {
                  color: 'gradient',
                  curveness: 0.5
                }
              }
            ]
          };
          this.play();
        }
    
      }),
    )).subscribe();

  }

  play() {
    if (this.obs) {
      this.obs.unsubscribe();
    }
    this.obs = interval(1500).subscribe(async (x) => {
      var index = x % this.nodes.length;
      if (index == 0) {
        await this.flowview.redraw();
        this.steps = [];
      }
      for (var element of this.steps) {
        element.nzStatus = 'finish';
      }

      for (const node of this.nodes[index].nodes) {
        this.flowview.sethighlight(node.bpmnid);
        this.steps = [...this.steps, { addDate: node.addDate, operationDesc: node.operationDesc, nzStatus: 'process', data: node.data }];
      }

      // if (this.nodes.length + 3 == x) {
      //   this.obs.unsubscribe();
      // }
    });
  }

}
