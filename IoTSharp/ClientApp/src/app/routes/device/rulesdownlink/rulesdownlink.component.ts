import { Component, Input, OnInit } from '@angular/core';
import { STColumn } from '@delon/abc/st';
import { _HttpClient } from '@delon/theme';
import { Guid } from 'guid-typescript';
import { NzDrawerRef } from 'ng-zorro-antd/drawer';
import { NzMessageService } from 'ng-zorro-antd/message';
import { AppMessage } from '../../common/AppMessage';

@Component({
  selector: 'app-rulesdownlink',
  templateUrl: './rulesdownlink.component.html',
  styleUrls: ['./rulesdownlink.component.less'],
})
export class RulesdownlinkComponent implements OnInit {
  rule = Guid.EMPTY;
  columns: STColumn[] = [
    { title: '', index: 'id', type: 'checkbox' },
    { title: 'id', index: 'id' },
    { title: '名称', index: 'name', render: 'name' },
    { title: '设备类型', index: 'email' },
    { title: '所有者', index: 'phone' },
    { title: '租户', index: 'country' },
    { title: '客户', index: 'province' },
  ];
  constructor(private http: _HttpClient,      private msg: NzMessageService,    private drawerRef: NzDrawerRef<string>,) {}
  @Input()
  params: any = {
    dev: [],
  };
  rules: any[];
  url = 'api/rules/index';
  ngOnInit(): void {
    this.http
      .post<AppMessage>(this.url, {
        offset: 0,
        limit: 100,
      })
      .subscribe(
        (x) => {      
            this.rules = x.data.rows;},
        (y) => {},
        () => {},
      );
  }

  selectchanged($event) {}

  downlink() {
    if(this.rule===Guid.EMPTY){
     this.msg.warning('please select a rule to execute')
      return
    }
    this.http
      .post('api/rules/binddevice', {
        rule: this.rule,
        dev: this.params.dev.map((x) => x.id),
      })
      .subscribe(
        (next) => {
          this.msg.create('success', '规则下发成功');
          this.drawerRef.close(this.params);
        },
        (error) => {
          this.msg.create('error', '规则下发失败');
          this.drawerRef.close(this.params);
        },
        () => {},
      );
  }
}
