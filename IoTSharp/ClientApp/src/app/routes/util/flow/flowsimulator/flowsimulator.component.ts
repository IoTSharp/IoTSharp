import { Component, OnInit, ViewChild, Input, ChangeDetectorRef, TemplateRef, OnDestroy } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { NzTimelineComponent, NzTimelineItemComponent, TimelineService } from 'ng-zorro-antd/timeline';

import { concat, interval, Observable, Subject, Subscription } from 'rxjs';
import { map } from 'rxjs/operators';
import { appmessage } from 'src/app/routes/common/AppMessage';
import { ruleflow } from 'src/app/routes/flow/flowlist/flowlist.component';
import { FlowviewerComponent } from 'src/app/routes/widgets/flowviewer/flowviewer.component';
import { DynamicformviewComponent } from '../../dynamicform/dynamicformview/dynamicformview.component';

@Component({
  selector: 'app-flowsimulator',
  templateUrl: './flowsimulator.component.html',
  styleUrls: ['./flowsimulator.component.less'],
})
export class FlowsimulatorComponent implements OnInit, OnDestroy {
  @Input()
  id: Number;
  listOfOption = [];
  obs: Subscription;
  // @ViewChild('flowtimeline', { static: true })
  // flowtimeline: NzTimelineComponent;
  @ViewChild('flowview', { static: true })
  flowview: FlowviewerComponent;
  @ViewChild('dynamicformview', { static: true })
  dynamicformview: DynamicformviewComponent;
  constructor(
    private http: _HttpClient,
    private cdr: ChangeDetectorRef, //  private timelineservice: TimelineService
  ) {}
  ngOnDestroy(): void {
    if (this.obs) {
      this.obs.unsubscribe();
    }
  }
  // @ViewChild('TimeLineItemTemplate', { read: TemplateRef }) TimeLineItemTemplate: TemplateRef<any>;
  thisisyourtestdataformid: Number = 1;
  steps: any = [];
  nodes = [];
  current: 0;
  ngOnInit(): void {
    concat(
      this.http.post('api/dynamicforminfo/index', { DictionaryGroupId: 1, pi: 0, ps: 20, limit: 20, offset: 0 }).pipe(
        map((x) => {
          this.listOfOption = x.result.rows.map((x) => {
            return { label: x.formName, value: x.formId };
          });

          this.dynamicformview.id = this.listOfOption[0]?.value;
          console.log(this.dynamicformview.id);
        }),
      ),
      this.http.get<appmessage<ruleflow>>('api/rules/get?id=' + this.id).pipe(
        map((x) => {
          this.flowview.diagramdata = x.result;
          this.flowview.loadXml();
        }),
      ),
    ).subscribe();

    // this.http.get<appmessage<ruleflow>>('api/rules/get?id=' + this.id).subscribe(
    //   (next) => {
    //     this.flowview.diagramdata = next.result;

    //     this.flowview.loadXml();
    //   },
    //   (error) => {},
    //   () => {},
    // );

    //it's deadend，nothing you can get
    // let item = new NzTimelineItemComponent(this.cdr, this.timelineservice);
    // item.borderColor = '#eeeeff';
    // item.template = this.TimeLineItemTemplate;

    // let item1 = new NzTimelineItemComponent(this.cdr, this.timelineservice);
    // item1.borderColor = '#eeeeff';
    // item1.template = this.TimeLineItemTemplate;

    // this.flowtimeline.timelineItems = [...this.flowtimeline.timelineItems, item, item1];
    // this.cdr.detectChanges();

    // console.log(this.flowtimeline.timelineItems);
  }
  formIdChange(iamnottheformidyouwantit): void {
    this.dynamicformview.id = this.thisisyourtestdataformid;
  }
  onsubmit(formdata) {
    this.http
      .post('api/rules/active', {
        form: formdata,
        extradata: {
          formid: this.thisisyourtestdataformid,
          ruleflowid: this.id,
        },
      })
      .subscribe(
        (next) => {
          this.nodes = next.result; //
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
    this.obs = interval(1000).subscribe(async (x) => {
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
        this.steps = [...this.steps, { addDate: node.addDate, operationDesc: node.operationDesc, nzStatus: 'process' }];
      }

      // if (this.nodes.length + 3 == x) {
      //   this.obs.unsubscribe();
      // }
    });
  }
}
