import { ChangeDetectorRef } from '@angular/core';
import { Input, ViewChild } from '@angular/core';
import { Component, OnInit } from '@angular/core';
import { SFComponent, SFNumberWidgetSchema, SFSchema, SFTextareaWidgetSchema } from '@delon/form';
import { _HttpClient } from '@delon/theme';
import { NzNotificationService } from 'ng-zorro-antd/notification';
import { switchMap } from 'rxjs/operators';

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
  constructor(private http: _HttpClient, private cd: ChangeDetectorRef,private notification: NzNotificationService) {}
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
    this.http.get<deviceattributeitem[]>('api/Devices/' + this.params.id + '/AttributeLatest').subscribe(
      (next) => {
        var properties: any = {};
        for (var item of next) {
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
      .get<deviceidentityinfo>('api/Devices/' + this.params.id + '/Identity')
      .pipe(
        switchMap((deviceidentityinfo: deviceidentityinfo) =>
          this.http.post('api/Devices/' + deviceidentityinfo.identityId + '/Attributes', value),
        ),
      )
      .subscribe(
        (next) => {},
        (error) => {
          this.notification.create(
            'warning',
            '修改错误',
            '属性数据修改失败.'
          );
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
