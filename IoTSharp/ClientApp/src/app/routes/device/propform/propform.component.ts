import { ChangeDetectorRef } from '@angular/core';
import { Input, ViewChild } from '@angular/core';
import { Component, OnInit } from '@angular/core';
import { SFComponent, SFNumberWidgetSchema, SFSchema, SFTextareaWidgetSchema } from '@delon/form';
import { _HttpClient } from '@delon/theme';
import { NzDrawerRef } from 'ng-zorro-antd/drawer';
import { NzMessageService } from 'ng-zorro-antd/message';
import { NzNotificationService } from 'ng-zorro-antd/notification';
import { switchMap } from 'rxjs/operators';
import { appmessage } from '../../common/AppMessage';

@Component({
  selector: 'app-propform',
  templateUrl: './propform.component.html',
  styleUrls: ['./propform.component.less'],
})
export class PropformComponent implements OnInit {
  @ViewChild('sf', { static: false })
  sf!: SFComponent;
  @Input() params: any = {
    id: '-1',
    customerId: '-1',
  };
  constructor(
    private http: _HttpClient, 
    
    private drawerRef: NzDrawerRef<string>,
      private msg: NzMessageService,
       private cd: ChangeDetectorRef) {}
  schema: SFSchema = {
    properties: {
      // "field1": {
      //     "type": "string",
      //     "title": "参12222数5",
      //     "maxlength": 20
      // },
    },
  };

  ngOnInit(): void {
    this.http.get<any>('api/Devices/' + this.params.id + '/AttributeLatest').subscribe(
      (next) => {
        var properties: any = {};
        for (var item of next.data) {
          switch (item.dataType) {
            case 'XML':
              properties[item.keyName] = {
                type: 'string',
                title: item.keyName,
                // maxLength: item.maxLength,
                //  pattern: item.pattern,
                ui: {
                  widget: 'codefield',
                  loadingTip: 'loading...',
                  config: { theme: 'vs-dark', language: 'xml' },
                  value: item.value,
                },
                default: item.value,
              };

              break;

            case 'Boolean':
              properties[item.keyName] = {
                type: 'boolean',
                title: item.keyName,
                // maxLength: item.maxLength,
                //  pattern: item.pattern,
                ui: {
                  checkedChildren: 'True',
                  unCheckedChildren: 'False',
                },
                default: item.value,
              };

              break;

            case 'String':
              properties[item.keyName] = {
                type: 'string',
                title: item.keyName,
                // maxLength: item.maxLength,
                //  pattern: item.pattern,

                default: item.value,
              };

              break;

            case 'Long':
              properties[item.keyName] = {
                type: 'number',
                title: item.keyName,
                // maxLength: item.maxLength,
                //  pattern: item.pattern,
                ui: {
                  hideStep: true,
                } as SFNumberWidgetSchema,
                default: item.value,
              };

              break;

            case 'Double':
              properties[item.keyName] = {
                type: 'number',
                title: item.keyName,
                // maxLength: item.maxLength,
                //  pattern: item.pattern,
                ui: {
                  hideStep: true,
                } as SFNumberWidgetSchema,
                default: item.value,
              };

              break;

            case 'Json':
              properties[item.keyName] = {
                type: 'string',
                title: item.keyName,
                // maxLength: item.maxLength,
                //  pattern: item.pattern,
                ui: {
                  widget: 'codefield',
                  loadingTip: 'loading...',
                  config: { theme: 'vs-dark', language: 'json' },
                  value: item.value,
                },
                default: item.value,
              };

              break;

            case 'Binary':
              properties[item.keyName] = {
                type: 'string',
                title: item.keyName,
                // maxLength: item.maxLength,
                //  pattern: item.pattern,
                ui: {
                  widget: 'textarea',
                  autosize: { minRows: 2, maxRows: 10 },
                } as SFTextareaWidgetSchema,
                default: item.value,
              };

              break;
            case 'DateTime':
              properties[item.keyName] = {
                type: 'string',
                title: item.keyName,
                format: 'date-time',
                displayFormat: 'yyyy-MM-dd HH:mm:ss',
                ui: {},
                default: item.value,
              };

              break;
          }
        }

        this.schema.properties = properties;
        this.sf.refreshSchema();
        this.cd.detectChanges();
      },
      (error) => {},
      () => {},
    );
  }

  submit(value: any) {
    this.http
      .get<appmessage<deviceidentityinfo>>('api/Devices/' + this.params.id + '/Identity')
      .pipe(
        switchMap((deviceidentityinfo: appmessage<deviceidentityinfo>) =>
          this.http.post('api/Devices/' + deviceidentityinfo.data?.identityId + '/Attributes', value),
        ),
      )
      .subscribe(
        (next) => {
          this.msg.create('success', '设备属性更新成功');
          this.drawerRef.close(this.params);
        },
        (error) => {
          this.msg.create('error', '设备属性更新失败');
          this.drawerRef.close(this.params);
        },
        () => {},
      );
  }
}
export interface deviceidentityinfo {
  id: string;
  identityType: any;
  identityId: string;
  identityValue: string;
}

export interface deviceattributeitem {
  keyName: string;
  dataSide: any;
  dateTime: string;
  value: string;
  dataType: string;
}
