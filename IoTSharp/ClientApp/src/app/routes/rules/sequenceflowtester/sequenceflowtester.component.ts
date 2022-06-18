import { Component, Input, OnInit } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { NzMessageService } from 'ng-zorro-antd/message';
import { appmessage } from 'src/app/models/appmessage';
import { flow } from 'src/app/models/flowrule';

@Component({
  selector: 'app-sequenceflowtester',
  templateUrl: './sequenceflowtester.component.html',
  styleUrls: ['./sequenceflowtester.component.less']
})
export class SequenceflowtesterComponent implements OnInit {
  @Input()
  flow: flow;
  data = {
    passed: [],
    failed: []
  };
  submitting: false;
  param: {};
  constructor(private http: _HttpClient, private message: NzMessageService) { }

  ngOnInit(): void {
    console.log(this.flow);
  }

  test($event) {
    this.http
      .post<appmessage<any>>('api/rules/RuleCondition', {
        ruleId: this.flow.flowRule.ruleId,
        flowId: this.flow.flowId,
        Data: this.param
      })
      .subscribe(
        {
          next: next => {
            this.data = next.data;
          },
          error: error => { },
          complete: () => { }
        }
      );
  }
}
