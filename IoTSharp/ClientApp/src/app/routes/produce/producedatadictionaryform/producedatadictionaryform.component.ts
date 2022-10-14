import { Component, Input, OnInit, ViewChild } from '@angular/core';
import { STComponent, STData, STColumn } from '@delon/abc/st';
import { _HttpClient } from '@delon/theme';
import { Guid } from 'guid-typescript';
import { NzMessageService } from 'ng-zorro-antd/message';
import { appmessage } from 'src/app/models/appmessage';

@Component({
  selector: 'app-producedatadictionaryform',
  templateUrl: './producedatadictionaryform.component.html',
  styleUrls: ['./producedatadictionaryform.component.less']
})
export class ProducedatadictionaryformComponent implements OnInit {


  @Input() id: string = Guid.EMPTY;
  @ViewChild('st') private st!: STComponent;
  dictionaryData: STData[] = []

  columns: STColumn[] = [

    { title: '字段名称', index: 'keyName', render: 'keyNameTpl' },
    { title: '字段显示名称', index: 'displayName', render: 'displayNameTpl' },
    { title: '单位', index: 'unit', render: 'unitTpl' },
    { title: '单位转换表达式', index: 'unitExpression', render: 'unitExpressionTpl' },
    { title: 'UnitConvert', index: 'unitConvert', render: 'unitConvertTpl' },
    { title: '字段备注', index: 'keyDesc', render: 'keyDescTpl' },
    { title: '默认值', index: 'defaultValue', render: 'defaultValueTpl' },
    { title: '是否显示', index: 'display', render: 'displayTpl' },
    { title: '位置名称', index: 'place0', render: 'place0Tpl' },
    { title: 'Tag', index: 'tag', render: 'tagtpl' },

    { title: '数据类型', index: 'dataType', render: 'dataTypeTpl' },
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

    this.http.get<appmessage<any>>('api/produces/getProduceDictionary?produceId=' + this.id)
      .subscribe({
        next: next => {
          this.dictionaryData = next.data;
        },

        error: error => {

        },

        complete: () => { }

      });
  }

  private submit(i: STData): void {
    this.updateEdit(i, false)
    this.dictionaryData = this.st.list;

  }


  private remove(i: STData) {
    this.st.removeRow(i); this.dictionaryData = this.st.list;
  }
  private updateEdit(i: STData, edit: boolean): void {

    this.st.setRow(i, { edit }, { refreshSchema: true });
  }
  _id = 0;
  newRow() {
    this.dictionaryData = [{ edit: true }, ...this.dictionaryData]
  }

  saveRow() {
    this.http.post('api/produces/editProduceDictionary', { produceId: this.id, produceDictionaryData: this.dictionaryData }).subscribe(
      {
        next: next => { 

          if (next.code === 10000) {
            this.msg.success('保存成功')
          }else{
            this.msg.warning('保存失败:'+next.msg)
          }

        },
        error: error => { },
        complete: () => { }
      }
    );
  }

}
