import { Component, Input, OnInit, ViewChild } from '@angular/core';
import { STComponent, STData, STColumn } from '@delon/abc/st';
import { _HttpClient } from '@delon/theme';
import { Guid } from 'guid-typescript';
import { NzMessageService } from 'ng-zorro-antd/message';
import { appmessage } from 'src/app/models/appmessage';

@Component({
  selector: 'app-producedataform',
  templateUrl: './producedataform.component.html',
  styleUrls: ['./producedataform.component.less']
})
export class ProducedataformComponent implements OnInit {

  @Input() id: string = Guid.EMPTY;
  @ViewChild('st') private st!: STComponent;
  produceData: STData[] = []

  columns: STColumn[] = [
    { title: 'KeyName', index: 'keyName', render: 'ColumnTitleTpl' },
    { title: '数据侧', index: 'dataSide', render: 'ColumnTypeTpl' },
    { title: '数据类型', index: 'type', render: 'ColumnDataTypeTpl' },
    {
      title: '操作',
      buttons: [
        {
          text: `编辑`,
          iif: i => !i.edit,
          click: i => this.updateEdit(i, true),
        },
        {
          text: `删除`,
          iif: i => !i.edit, pop: {
            title: '确认删除属性?',
            okType: 'danger',
            icon: 'warning',
          },
          click: i => this.remove(i),
        },
        {
          text: `保存`,
          iif: i => i.edit,
          click: (i, v) => {
            console.log(i)
            this.submit(i);
          },
        },
        {
          text: `取消`,
          iif: i => i.edit,
          click: i => this.updateEdit(i, false),
        },
      ],
    },
  ];
  constructor(private msg: NzMessageService, private http: _HttpClient,) {



  }

  ngOnInit(): void {

    this.http.get<appmessage<any>>('api/produces/GetProduceData?produceId=' + this.id).subscribe(next => {
      this.produceData = next.data;
    }, error => {

    }, () => { });
  }

  private submit(i: STData): void {
    this.updateEdit(i, false)
    this.produceData = this.st.list;

  }


  private remove(i: STData) {
    this.st.removeRow(i); this.produceData = this.st.list;
  }
  private updateEdit(i: STData, edit: boolean): void {

    this.st.setRow(i, { edit }, { refreshSchema: true });
  }
  _id = 0;
  newRow() {
    this.produceData = [{ edit: true }, ...this.produceData]
  }

  saveRow() {
   this.http.post('api/produces/editProduceData', { produceId: this.id, produceData: this.produceData }).subscribe(next => { 
    if (next.code === 10000) {
      this.msg.success('保存成功')
    }else{
      this.msg.warning('保存失败:'+next.msg)
    }}, error => { }, () => { });
  }
  


  
}
