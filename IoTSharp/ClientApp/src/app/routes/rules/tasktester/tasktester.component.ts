import { Component, Input, OnInit, ViewChild } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { NzMessageService } from 'ng-zorro-antd/message';
import { flow } from 'src/app/models/flowrule';
import { CodeviewComponent } from '../../widgets/codeview/codeview.component';

@Component({
  selector: 'app-tasktester',
  templateUrl: './tasktester.component.html',
  styleUrls: ['./tasktester.component.less']
})
export class TasktesterComponent implements OnInit {
  @ViewChild('dcv', { static: true })
  dcv: CodeviewComponent;
  @Input()
  flow: flow;
  result = {};
  submitting: false;
  param: {};

  config = '';
  constructor(private http: _HttpClient, private message: NzMessageService) {}

  ngOnInit(): void {
    this.config = this.flow.nodeProcessParams;
  }

  test($event) {
    this.http
      .post('api/rules/TestTask', {
        ruleId: this.flow.flowRule.ruleId,
        flowId: this.flow.flowId,
        Data: this.param
      })
      .subscribe(
        next => {
          this.result = next.data;
        },
        error => {},
        () => {}
      );
  }
}
