import { Component, OnInit, ViewChild, Input, ChangeDetectorRef, TemplateRef } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { NzTimelineComponent, NzTimelineItemComponent, TimelineService } from 'ng-zorro-antd/timeline';

import { Observable } from 'rxjs';
import { appmessage } from 'src/app/routes/common/AppMessage';
import { ruleflow } from 'src/app/routes/flow/flowlist/flowlist.component';
import { FlowviewerComponent } from 'src/app/routes/widgets/flowviewer/flowviewer.component';
import { DynamicformviewComponent } from '../../dynamicform/dynamicformview/dynamicformview.component';

@Component({
  selector: 'app-flowsimulator',
  templateUrl: './flowsimulator.component.html',
  styleUrls: ['./flowsimulator.component.less'],
})
export class FlowsimulatorComponent implements OnInit {
  @Input()
  id: Number;
  // @ViewChild('flowtimeline', { static: true })
  // flowtimeline: NzTimelineComponent;
  @ViewChild('flowview', { static: true })
  flowview: FlowviewerComponent;
  @ViewChild('dynamicformview', { static: true })
  dynamicformview: DynamicformviewComponent;
  constructor(
    private httpClient: _HttpClient,
    private cdr: ChangeDetectorRef, //  private timelineservice: TimelineService
  ) {}
  // @ViewChild('TimeLineItemTemplate', { read: TemplateRef }) TimeLineItemTemplate: TemplateRef<any>;
  thisisyourtestdataformid: number = 1;
  steps: [];
  current: 0;
  ngOnInit(): void {
    this.httpClient.get<appmessage<ruleflow>>('api/rules/get?id=' + this.id).subscribe(
      (next) => {
        this.flowview.diagramdata = next.result;

        this.flowview.loadXml();
      },
      (error) => {},
      () => {},
    );
    this.dynamicformview.id = this.thisisyourtestdataformid;

    //it's deadend
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

  onsubmit(formdata) {
    this.httpClient
      .post('api/rules/active', {
        form: formdata,
        extradata: {
          formid: this.thisisyourtestdataformid,
          ruleflowid: this.id,
        },
      })
      .subscribe(
        (next) => {},
        (error) => {},
        () => {},
      );
  }
}
