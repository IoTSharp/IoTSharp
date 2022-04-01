import { Component, Input, OnInit } from '@angular/core';
import { STColumn } from '@delon/abc/st';
import { _HttpClient } from '@delon/theme';
import { Guid } from 'guid-typescript';
import { NzDrawerRef } from 'ng-zorro-antd/drawer';
import { NzMessageService } from 'ng-zorro-antd/message';
import { AppMessage } from '../../common/AppMessage';

@Component({
  selector: 'app-exporttoasset',
  templateUrl: './exporttoasset.component.html',
  styleUrls: ['./exporttoasset.component.less']
})
export class ExporttoassetComponent implements OnInit {

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
  assets: any[];
  url = 'api/asset/list';
  ngOnInit(): void {
    this.http
      .get<AppMessage>(this.url, {
        offset: 0,
        limit: 1000,
      })
      .subscribe(
        (x) => {      
            this.assets = x.data.rows;},
        (y) => {},
        () => {},
      );
  }

  selectchanged($event) {}

  expert() {
    if(this.rule===Guid.EMPTY){
     this.msg.warning('please select a rule to execute')
      return
    }
    this.http
      .post('api/asset/adddevice', {
        assetid: this.rule,
        relations: this.params.dev.map((x) =>{return {DeviceId:x.id,name:x.name}}),
      })
      .subscribe(
        (next) => {
          this.msg.create('success', '设备导出成功');
          this.drawerRef.close(this.params);
        },
        (error) => {
          this.msg.create('error', '设备导出失败');
          this.drawerRef.close(this.params);
        },
        () => {},
      );
  }
}
