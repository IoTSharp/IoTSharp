import { Component, Input, OnInit } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { NzMessageService } from 'ng-zorro-antd/message';
import { flow } from '../flowlist/flowlist.component';

@Component({
  selector: 'app-sequenceflowtester',
  templateUrl: './sequenceflowtester.component.html',
  styleUrls: ['./sequenceflowtester.component.less'],
})
export class SequenceflowtesterComponent implements OnInit {
  @Input()
  flow: flow;
  data={
    passed: [],
    failed: [],
  };
  submitting: false;
  param: {};
  constructor(private http: _HttpClient, private message: NzMessageService) {}

  ngOnInit(): void {
    console.log(this.flow);
  }

  test($event) {
    this.http
      .post('api/rules/RuleCondition', {
        ruleId: this.flow.flowRule.ruleId,
        flowId: this.flow.flowId,
        Data: this.param,
      })
      .subscribe(
        (next) => {
        this.data=next.data;
        },
        (error) => {},
        () => {},
      );
  }
}
