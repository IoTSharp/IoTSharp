import { ChangeDetectorRef } from '@angular/core';
import { Input, ViewChild } from '@angular/core';
import { Component, OnInit } from '@angular/core';
import { SFComponent, SFNumberWidgetSchema, SFSchema, SFTextareaWidgetSchema } from '@delon/form';
import { _HttpClient } from '@delon/theme';
import { Guid } from 'guid-typescript';
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
  @ViewChild('sfserver', { static: false })
  sfserver!: SFComponent;

  @ViewChild('sfany', { static: false })
  sfany!: SFComponent;
  @Input() params: any = {
    id: Guid.EMPTY,
    customerId: Guid.EMPTY,
  };
  constructor(
    private http: _HttpClient,

    private drawerRef: NzDrawerRef<string>,
    private msg: NzMessageService,
    private cd: ChangeDetectorRef,
  ) {}
  schemaserver: SFSchema = {
    properties: {
      // "field1": {
      //     "type": "string",
      //     "title": "参12222数5",
      //     "maxlength": 20
      // },
    },
  };

  schemaany: SFSchema = {
    properties: {
      // "field1": {
      //     "type": "string",
      //     "title": "参12222数5",
      //     "maxlength": 20
      // },
    },
  };

  ngOnInit(): void {
    this.http.get<appmessage<deviceattributeitem[]>>('api/Devices/' + this.params.id + '/AttributeLatest').subscribe(
      (next) => {
        var propertiesserver: any = {};
        var propertiesany: any = {};
        var serverSide = next.data.filter((x) => x.dataSide == 'ServerSide');
        var anySide = next.data.filter((x) => x.dataSide == 'AnySide');
        for (var item of serverSide) {
          switch (item.dataType) {
            case 'XML':
              propertiesserver[item.keyName] = {
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
              propertiesserver[item.keyName] = {
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
              propertiesserver[item.keyName] = {
                type: 'string',
                title: item.keyName,
                // maxLength: item.maxLength,
                //  pattern: item.pattern,

                default: item.value,
              };

              break;

            case 'Long':
              propertiesserver[item.keyName] = {
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
              propertiesserver[item.keyName] = {
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
              propertiesserver[item.keyName] = {
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
              propertiesserver[item.keyName] = {
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
              propertiesserver[item.keyName] = {
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

        for (var item of anySide) {
          switch (item.dataType) {
            case 'XML':
              propertiesany[item.keyName] = {
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
              propertiesany[item.keyName] = {
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
              propertiesany[item.keyName] = {
                type: 'string',
                title: item.keyName,
                // maxLength: item.maxLength,
                //  pattern: item.pattern,

                default: item.value,
              };

              break;

            case 'Long':
              propertiesany[item.keyName] = {
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
              propertiesany[item.keyName] = {
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
              propertiesany[item.keyName] = {
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
              propertiesany[item.keyName] = {
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
              propertiesany[item.keyName] = {
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

        this.schemaserver.properties = propertiesserver;
        this.schemaany.properties = propertiesany;
        this.sfserver.refreshSchema();
        this.sfany.refreshSchema();
        this.cd.detectChanges();
      },
      (error) => {},
      () => {},
    );
  }

  save($event) {
    var val = {
      serverside: this.sfserver.value,
      anyside: this.sfany.value,
    };
    this.http.post<appmessage<any>>('api/Devices/' + this.params.id + '/EditAttribute', val).subscribe(
      (next) => {
        if (next.code === 10000) {
          this.msg.success('属性修改成功');
          this.drawerRef.close(this.params);
        } else {
          this.msg.error(next.msg);
        }
      },
      (error) => {},
      () => {},
    );
  }

  submit(value: any) {
    console.log(value);
    this.http.post<appmessage<any>>('api/Devices/' + this.params.id + '/EditAttribute', value).subscribe(
      (next) => {
        if (next.code === 10000) {
          this.msg.success('属性修改成功');
          this.drawerRef.close(this.params);
        } else {
          this.msg.error(next.msg);
        }
      },
      (error) => {},
      () => {},
    );

    // this.http
    //   .get<appmessage<deviceidentityinfo>>('api/Devices/' + this.params.id + '/Identity')
    //   .pipe(
    //     switchMap((deviceidentityinfo: appmessage<deviceidentityinfo>) =>
    //       this.http.post('api/Devices/' + deviceidentityinfo.data?.identityId + '/Attributes', value),
    //     ),
    //   )
    //   .subscribe(
    //     (next) => {
    //       this.msg.create('success', '设备属性更新成功');
    //       this.drawerRef.close(this.params);
    //     },
    //     (error) => {
    //       this.msg.create('error', '设备属性更新失败');
    //       this.drawerRef.close(this.params);
    //     },
    //     () => {},
    //   );
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
  dataSide: string;
  dateTime: string;
  value: string;
  dataType: string;
}
