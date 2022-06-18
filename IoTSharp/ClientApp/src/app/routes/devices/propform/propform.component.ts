import { ChangeDetectorRef, Component, Input, OnInit, ViewChild } from '@angular/core';
import { SFComponent, SFSchema, SFNumberWidgetSchema, SFTextareaWidgetSchema } from '@delon/form';
import { _HttpClient } from '@delon/theme';
import { Guid } from 'guid-typescript';
import { NzDrawerRef } from 'ng-zorro-antd/drawer';
import { NzMessageService } from 'ng-zorro-antd/message';
import { appmessage } from 'src/app/models/appmessage';
import { deviceattributeitem } from 'src/app/models/deviceidentityinfo';

@Component({
  selector: 'app-propform',
  templateUrl: './propform.component.html',
  styleUrls: ['./propform.component.less']
})
export class PropformComponent implements OnInit {

  @ViewChild('sfserver', { static: false })
  sfserver!: SFComponent;

  @ViewChild('sfany', { static: false })
  sfany!: SFComponent;

  @ViewChild('sfdevice', { static: false })
  sfdevice!: SFComponent;

  @Input() params: any = {
    id: Guid.EMPTY,
    customerId: Guid.EMPTY
  };
  constructor(
    private http: _HttpClient,

    private drawerRef: NzDrawerRef<string>,
    private msg: NzMessageService,
    private cd: ChangeDetectorRef
  ) { }
  schemadevice: SFSchema = {
    properties: {}
  };

  schemaserver: SFSchema = {
    properties: {
      // "field1": {
      //     "type": "string",
      //     "title": "参12222数5",
      //     "maxlength": 20
      // },
    }
  };

  schemaany: SFSchema = {
    properties: {
      // "field1": {
      //     "type": "string",
      //     "title": "参12222数5",
      //     "maxlength": 20
      // },
    }
  };

  ngOnInit(): void {
    this.http.get<appmessage<deviceattributeitem[]>>('api/Devices/' + this.params.id + '/AttributeLatest').subscribe(
      {
        next: next => {
          var propertiesserver: any = {};
          var propertiesany: any = {};
          var propertiesdevice: any = {};
          var serverSide = next.data.filter(x => x.dataSide == 'ServerSide');
          var anySide = next.data.filter(x => x.dataSide == 'AnySide');
          var devicveSide = next.data.filter(x => x.dataSide == 'ClientSide');
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
                    value: item.value
                  },
                  default: item.value
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
                    unCheckedChildren: 'False'
                  },
                  default: item.value
                };

                break;

              case 'String':
                propertiesserver[item.keyName] = {
                  type: 'string',
                  title: item.keyName,
                  // maxLength: item.maxLength,
                  //  pattern: item.pattern,

                  default: item.value
                };

                break;

              case 'Long':
                propertiesserver[item.keyName] = {
                  type: 'number',
                  title: item.keyName,
                  // maxLength: item.maxLength,
                  //  pattern: item.pattern,
                  ui: {
                    hideStep: true
                  } as SFNumberWidgetSchema,
                  default: item.value
                };

                break;

              case 'Double':
                propertiesserver[item.keyName] = {
                  type: 'number',
                  title: item.keyName,
                  // maxLength: item.maxLength,
                  //  pattern: item.pattern,
                  ui: {
                    hideStep: true
                  } as SFNumberWidgetSchema,
                  default: item.value
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
                    value: item.value
                  },
                  default: item.value
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
                    autosize: { minRows: 2, maxRows: 10 }
                  } as SFTextareaWidgetSchema,
                  default: item.value
                };

                break;
              case 'DateTime':
                propertiesserver[item.keyName] = {
                  type: 'string',
                  title: item.keyName,
                  format: 'date-time',
                  displayFormat: 'yyyy-MM-dd HH:mm:ss',
                  ui: {},
                  default: item.value
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
                    value: item.value
                  },
                  default: item.value
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
                    unCheckedChildren: 'False'
                  },
                  default: item.value
                };

                break;

              case 'String':
                propertiesany[item.keyName] = {
                  type: 'string',
                  title: item.keyName,
                  // maxLength: item.maxLength,
                  //  pattern: item.pattern,

                  default: item.value
                };

                break;

              case 'Long':
                propertiesany[item.keyName] = {
                  type: 'number',
                  title: item.keyName,
                  // maxLength: item.maxLength,
                  //  pattern: item.pattern,
                  ui: {
                    hideStep: true
                  } as SFNumberWidgetSchema,
                  default: item.value
                };

                break;

              case 'Double':
                propertiesany[item.keyName] = {
                  type: 'number',
                  title: item.keyName,
                  // maxLength: item.maxLength,
                  //  pattern: item.pattern,
                  ui: {
                    hideStep: true
                  } as SFNumberWidgetSchema,
                  default: item.value
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
                    value: item.value
                  },
                  default: item.value
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
                    autosize: { minRows: 2, maxRows: 10 }
                  } as SFTextareaWidgetSchema,
                  default: item.value
                };

                break;
              case 'DateTime':
                propertiesany[item.keyName] = {
                  type: 'string',
                  title: item.keyName,
                  format: 'date-time',
                  displayFormat: 'yyyy-MM-dd HH:mm:ss',
                  ui: {},
                  default: item.value
                };

                break;
            }
          }

          for (var item of devicveSide) {
            switch (item.dataType) {
              case 'XML':
                propertiesdevice[item.keyName] = {
                  type: 'string',
                  title: item.keyName,
                  // maxLength: item.maxLength,
                  //  pattern: item.pattern,
                  ui: {
                    widget: 'codefield',
                    loadingTip: 'loading...',
                    config: { theme: 'vs-dark', language: 'xml' },
                    value: item.value
                  },
                  default: item.value
                };

                break;

              case 'Boolean':
                propertiesdevice[item.keyName] = {
                  type: 'boolean',
                  title: item.keyName,
                  // maxLength: item.maxLength,
                  //  pattern: item.pattern,
                  ui: {
                    checkedChildren: 'True',
                    unCheckedChildren: 'False'
                  },
                  default: item.value
                };

                break;

              case 'String':
                propertiesdevice[item.keyName] = {
                  type: 'string',
                  title: item.keyName,
                  // maxLength: item.maxLength,
                  //  pattern: item.pattern,

                  default: item.value
                };

                break;

              case 'Long':
                propertiesdevice[item.keyName] = {
                  type: 'number',
                  title: item.keyName,
                  // maxLength: item.maxLength,
                  //  pattern: item.pattern,
                  ui: {
                    hideStep: true
                  } as SFNumberWidgetSchema,
                  default: item.value
                };

                break;

              case 'Double':
                propertiesdevice[item.keyName] = {
                  type: 'number',
                  title: item.keyName,
                  // maxLength: item.maxLength,
                  //  pattern: item.pattern,
                  ui: {
                    hideStep: true
                  } as SFNumberWidgetSchema,
                  default: item.value
                };

                break;

              case 'Json':
                propertiesdevice[item.keyName] = {
                  type: 'string',
                  title: item.keyName,
                  // maxLength: item.maxLength,
                  //  pattern: item.pattern,
                  ui: {
                    widget: 'codefield',
                    loadingTip: 'loading...',
                    config: { theme: 'vs-dark', language: 'json' },
                    value: item.value
                  },
                  default: item.value
                };

                break;

              case 'Binary':
                propertiesdevice[item.keyName] = {
                  type: 'string',
                  title: item.keyName,
                  // maxLength: item.maxLength,
                  //  pattern: item.pattern,
                  ui: {
                    widget: 'textarea',
                    autosize: { minRows: 2, maxRows: 10 }
                  } as SFTextareaWidgetSchema,
                  default: item.value
                };

                break;
              case 'DateTime':
                propertiesdevice[item.keyName] = {
                  type: 'string',
                  title: item.keyName,
                  format: 'date-time',
                  displayFormat: 'yyyy-MM-dd HH:mm:ss',
                  ui: {},
                  default: item.value
                };

                break;
            }
          }

          this.schemaserver.properties = propertiesserver;
          this.schemaany.properties = propertiesany;
          this.schemadevice.properties = propertiesdevice;
          this.sfserver.refreshSchema();
          this.sfany.refreshSchema();
          this.sfdevice.refreshSchema();
          this.cd.detectChanges();
        },
        error: error => { },
        complete: () => { }
      }
    );
  }

  save($event) {
    var val = {
      serverside: this.sfserver.value,
      anyside: this.sfany.value,
      clientside: this.sfdevice.value
    };
    this.http.post<appmessage<any>>('api/Devices/' + this.params.id + '/EditAttribute', val).subscribe(
      {
        next: next => {
          if (next.code === 10000) {
            this.msg.success('属性修改成功');
            this.drawerRef.close(this.params);
          } else {
            this.msg.error(next.msg);
          }
        },
        error: error => { },
        complete: () => { }
      }
    );
  }

  submit(value: any) {
    console.log(value);
    this.http.post<appmessage<any>>('api/Devices/' + this.params.id + '/EditAttribute', value).subscribe(
      {

        next: next => {
          if (next.code === 10000) {
            this.msg.success('属性修改成功');
            this.drawerRef.close(this.params);
          } else {
            this.msg.error(next.msg);
          }
        },
        error: error => { },
        complete: () => { }
      }
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