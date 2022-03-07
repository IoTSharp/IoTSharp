import { Component, OnInit, ViewChild, Input, OnDestroy } from '@angular/core';
import { _HttpClient } from '@delon/theme';

import { concat, interval, Subscription } from 'rxjs';
import { map } from 'rxjs/operators';
import { appmessage } from 'src/app/routes/common/AppMessage';
import { flowrule } from 'src/app/routes/flow/flowlist/flowlist.component';
import { FlowviewerComponent } from 'src/app/routes/widgets/flowviewer/flowviewer.component';
import { DynamicformviewComponent } from '../../dynamicform/dynamicformview/dynamicformview.component';

@Component({
  selector: 'app-flowsimulator',
  templateUrl: './flowsimulator.component.html',
  styleUrls: ['./flowsimulator.component.less'],
})
export class FlowsimulatorComponent implements OnInit, OnDestroy {
  @Input()
  id: string;
  listOfOption = [];
  obs: Subscription;
  param:string;
  @ViewChild('flowview', { static: true })
  flowview: FlowviewerComponent;
  @ViewChild('dynamicformview', { static: true })
  dynamicformview: DynamicformviewComponent;
  constructor(
    private http: _HttpClient, 
  ) {}
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
    concat(
      // this.http.post('api/dynamicforminfo/index', { DictionaryGroupId: 1, pi: 0, ps: 20, limit: 20, offset: 0 }).pipe(
      //   map((x) => {
      //     this.listOfOption = x.data.rows.map((x) => {
      //       return { label: x.formName, value: x.formId };
      //     });

      //     this.dynamicformview.id = this.listOfOption[0]?.value;
      //     console.log(this.dynamicformview.id);
      //   }),
      // ),
      this.http.get<appmessage<flowrule>>('api/rules/get?id=' + this.id).pipe(
        map((x) => {
          this.flowview.diagramdata = x.data;
          this.flowview.loadXml();
        }),
      ),
    ).subscribe();

  }
  formIdChange(iamnottheformidyouwantit): void {
    this.dynamicformview.id = this.thisisyourtestdataformid;
  }
  onsubmit(formdata) {

console.log(
  JSON.parse(this.param))
    this.http
      .post('api/rules/active', {
        form: JSON.parse(this.param),
        extradata: {
  //       formid: this.thisisyourtestdataformid,
          ruleflowid: this.id,
        },
      })
      .subscribe(
        (next) => {
          this.nodes = next.data; //
          this.play();
        },
        (error) => {},
        () => {},
      );
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
        this.steps = [...this.steps, { addDate: node.addDate, operationDesc: node.operationDesc, nzStatus: 'process',data:node.data }];
      }

    });
  }
}
