import { Component, Input, OnInit, ViewChild } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { concat, interval, map, Subscription } from 'rxjs';
import { appmessage } from 'src/app/models/appmessage';
import { flowrule } from 'src/app/models/flowrule';
import { FlowviewerComponent } from '../flowviewer/flowviewer.component';

@Component({
  selector: 'app-flowsimulator',
  templateUrl: './flowsimulator.component.html',
  styleUrls: ['./flowsimulator.component.less']
})
export class FlowsimulatorComponent implements OnInit {
  @Input()
  id: string;
  listOfOption = [];
  obs: Subscription;
  param: string;
  @ViewChild('flowview', { static: true })
  flowview: FlowviewerComponent;
  // @ViewChild('dynamicformview', { static: true })
  // dynamicformview: DynamicformviewComponent;
  constructor(private http: _HttpClient) {}
  ngOnDestroy(): void {
    if (this.obs) {
      this.obs.unsubscribe();
    }
  }

  thisisyourtestdataformid: Number = 1;
  steps: any = [];
  nodes = [];
  current: 0;
  ngOnInit(): void {
    this.http.get<appmessage<flowrule>>('api/rules/get?id=' + this.id).subscribe({
      next: _next => {
        this.flowview.diagramdata = _next.data;
        this.flowview.loadXml();
      },
      error: _error => {},
      complete: () => {}
    });
  }
  formIdChange(iamnottheformidyouwantit): void {
    // this.dynamicformview.id = this.thisisyourtestdataformid;
  }
  onsubmit(formdata) {
    this.http
      .post('api/rules/active', {
        form: JSON.parse(this.param),
        extradata: {
          //       formid: this.thisisyourtestdataformid,
          ruleflowid: this.id
        }
      })
      .subscribe({
        next: _next => {
          this.nodes = _next.data; //
          this.play();
        },
        error: _error => {},
        complete: () => {}
      });
  }

  play() {
    if (this.obs) {
      this.obs.unsubscribe();
    }
    this.obs = interval(1500).subscribe(async x => {
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
    });
  }
}
