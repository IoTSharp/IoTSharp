import { ChangeDetectorRef, Component, EventEmitter, Input, OnInit, Output, ViewChild } from '@angular/core';
import { FormBuilder } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { SFComponent, SFObjectWidgetSchema, SFRadioWidgetSchema, SFDateWidgetSchema, SFSliderWidgetSchema, SFSelectWidgetSchema, SFTransferWidgetSchema, SFTreeSelectWidgetSchema, SFUploadWidgetSchema, SFSchema } from '@delon/form';
import { _HttpClient } from '@delon/theme';
import { NzDrawerRef } from 'ng-zorro-antd/drawer';
import { NzMessageService } from 'ng-zorro-antd/message';
import { map } from 'rxjs/operators';


@Component({
  selector: 'app-searchformgenerator',
  templateUrl: './searchformgenerator.component.html',
  styleUrls: ['./searchformgenerator.component.less']
})
export class SearchformgeneratorComponent implements OnInit {

  @ViewChild('sf', { static: false }) sf: SFComponent;
  @Input() _id: Number = -1;
  @Output() onsubmit = new EventEmitter();
  options: {};
  AllControlType: any = [];
  AllSuportType: any = [];
  SuportType: any = [];

  get id() {
    return this._id;
  }
  set id(val) {
    this._id = val;
    this.CreatForm(val);
  }

  constructor(
    private _router: ActivatedRoute,
    private router: Router,
    private _formBuilder: FormBuilder,
    private _httpClient: _HttpClient,
    private fb: FormBuilder,
    private msg: NzMessageService,
    private drawerRef: NzDrawerRef<string>,
    private cd: ChangeDetectorRef,
  ) { }

  private CreatForm(id) {
    this._httpClient.get('api/DynamicFormInfo/GetFormFieldValue?FormId=' + this.id + '&BizId=0').subscribe(
      (x) => {
        var properties = {};
        x.data = x.data.map((x) => { return {
          Creator: x.creator,
          FieldCode: x.fieldCode,
          FieldCreateDate: x.fieldCreateDate,
          FieldEditDate: x.fieldEditDate,
          FieldI18nKey: x.fieldI18nKey,
          FieldId: x.fieldId,
          FieldMaxLength: x.fieldMaxLength,
          FieldName: x.fieldName,
          FieldPattern: x.fieldPattern,
          FieldPocoTypeName: x.fieldPocoTypeName,
          FieldStatus: x.fieldStatus,
          FieldUIElement: x.fieldUIElement,
          FieldUIElementSchema: x.fieldUIElementSchema,
          FieldUnit: x.fieldUnit,
          FieldValue: x.fieldValue,
          FieldValueDataSource: x.fieldValueDataSource,
          FieldValueLocalDataSource: x.fieldValueLocalDataSource,
          FieldValueType: x.fieldValueType,
          FieldValueTypeName: x.fieldValueTypeName,
          FormId: x.formId,
          IsRequired: x.isRequired,
        };    });
        for (let a of x.data) {
          var Schame = JSON.parse(a.FieldUIElementSchema);
          switch (a.FieldUIElement) {
            case 1:

              properties[a.FieldCode] = {
                title: a.FieldName,
                type: 'object',

                properties: {

                

                  operator: {
                    title: a.FieldName,
                    type: 'string',
                    
                    enum: [
                      { label: '=', value: '='},
                      { label: '>', value: '>' },
                      { label: '<', value: '<' },
                    ]
                  },
                  value: {
                    type: a.FieldValueTypeName,
                    maximum: Schame.maximum,
                    minimum: Schame.minimum,
                    title: '',
                    ui: {
                      spanLabelFixed: 100,


                    },
                    default: a.FieldValue,
                  },
                  connector: {
                    type: 'boolean',
                    title: '',
                    ui: {
                      checkedChildren: 'And',
                      unCheckedChildren: 'Or',

                    }
                  },


                },
                ui: {
                  grid: { sm: 24, md: 12, xl: 12 },
                } as SFObjectWidgetSchema,

              }


              // properties[a.FieldCode] = {
              //   type: a.FieldValueTypeName,
              //   title: a.FieldName,
              //   // exclusiveMinimum: Schame.exclusiveMinimum,
              //   // exclusiveMaximum: Schame.exclusiveMaximum,
              //   maximum: Schame.maximum,
              //   minimum: Schame.minimum,
              //   ui: {
              //     widgetWidth: Schame.ui.widgetWidth,
              //   },
              //   default: a.FieldValue,
              // };
              break;
            case 2:
              properties[a.FieldCode] = {
                title: '',
                type: 'object',

                properties: {
                  value: {
                    type: a.FieldValueTypeName,
                    title: a.FieldName,
                    maxLength: Schame.maxLength,
                    pattern: Schame.pattern,
                    ui: {
                      addOnAfter: Schame.ui.addOnAfter,
                      placeholder: Schame.ui.placeholder,
                    },
                    default: a.FieldValue,
                  },
                  connector: {
                    type: 'boolean',
                    title: '',
                    ui: {
                      checkedChildren: 'And',
                      unCheckedChildren: 'Or',
                    }
                  },

                },
                ui: {

                } as SFObjectWidgetSchema,

              };
              break;
            case 3:
              break;
            case 4:
              properties[a.FieldCode] = {
                title: '',
                type: 'object',

                properties: {
                  value: {
                    type: a.FieldValueTypeName,
                    title: a.FieldName,
                    ui: {
                      widget: 'md',
                      options: {
                        toolbar: ['bold', 'italic', 'heading', '|', 'quote'],
                      },
                    },
                    default: a.FieldValue,
                  },

                  connector: {
                    type: 'boolean',
                    title: '',
                    ui: {
                      checkedChildren: 'And',
                      unCheckedChildren: 'Or',
                    }
                  },

                },
                ui: {
                  spanLabelFixed: 150,
                  grid: { span: 5, gutter: 1 },
                } as SFObjectWidgetSchema,
              };
              break;
            case 5:
              properties[a.FieldCode] = {
                title: '',
                type: 'object',

                properties: {
                  value: {
                    type: a.FieldValueTypeName,
                    title: a.FieldName,
                    ui: {
                      checkedChildren: Schame.ui.checkedChildren,
                      unCheckedChildren: Schame.ui.unCheckedChildren,
                    },
                    default: a.FieldValue,
                  }, connector: {
                    type: 'boolean',
                    title: '',
                    ui: {
                      checkedChildren: 'And',
                      unCheckedChildren: 'Or',
                    }
                  },

                },
                ui: {
                  spanLabelFixed: 150,
                  grid: { span: 5, gutter: 1 },
                } as SFObjectWidgetSchema,
              };

              break;
            case 6:
              properties[a.FieldCode] = {
                title: '',
                type: 'object',

                properties: {
                  value: {
                    type: a.FieldValueTypeName,
                    title: a.FieldName,
                    default: a.FieldValue ? JSON.parse(a.FieldValue) : '',
                    ui: {
                      widget: 'checkbox',
                      span: Schame.ui.span,
                      styleType: Schame.ui.styleType,
                      checkAll: Schame.ui.checkAll,
                      asyncData: () =>
                        this._httpClient.get(a.DataSource).pipe(
                          map((x) => {
                            return x.Result;
                          }),
                        ),
                      // this._httpClient.get(a.DataSource).pipe(
                      //   map((x) => {
                      //     return x.Result.map((y) => {
                      //       return {
                      //         value: y[Schame.IotSharp.key],
                      //         label: y[Schame.IotSharp.value],
                      //       };
                      //     });
                      //   }),
                      // ),
                    },

                  },
                   connector: {
                    type: 'boolean',
                    title: '',
                    ui: {
                      checkedChildren: 'And',
                      unCheckedChildren: 'Or',
                    }
                  },
                },
                ui: {
                  spanLabelFixed: 150,
                  grid: { span: 5, gutter: 1 },
                } as SFObjectWidgetSchema,
              };

              break;
            case 7:
              properties[a.FieldCode] = {
                type: a.FieldValueTypeName,
                title: a.FieldName,
                default: a.FieldValue,
                ui: {
                  checkedChildren: Schame.ui.checkedChildren,
                  unCheckedChildren: Schame.ui.unCheckedChildren,
                },
              };

              break;
            case 8:
              properties[a.FieldCode] = {
                type: a.FieldValueTypeName,
                title: a.FieldName,
                default: a.FieldValue,
                ui: {
                  widget: 'radio',
                  styleType: Schame.ui.styleType,
                  buttonStyle: Schame.ui.buttonStyle,
                  asyncData: () =>
                    this._httpClient.get(a.DataSource).pipe(
                      map((x) => {
                        return x.Result;
                      }),
                    ),
                  // this._httpClient.get(a.DataSource).pipe(
                  //   map((x) => {
                  //     return x.Result.map((y) => {
                  //       return {
                  //         value: y[Schame.IotSharp.key],
                  //         label: y[Schame.IotSharp.value],
                  //       };
                  //     });
                  //   }),
                  // ),
                } as SFRadioWidgetSchema,
              };

              break;
            case 9:
              switch (Schame.IotSharp.format) {
                case 'date-time':
                  properties[a.FieldCode] = {
                    type: 'string',
                    format: 'date-time',
                    default: '2015-3-1 11:11:11',
                    displayFormat: 'yyyy-MM-dd HH:mm:ss',
                    title: a.FieldName,
                  };
                  break;
                case 'date':
                  properties[a.FieldCode] = {
                    type: 'string',
                    title: a.FieldName,
                    default: a.FieldValue,
                    format: 'date',
                  };
                  break;
                case 'month':
                  properties[a.FieldCode] = {
                    title: a.FieldName,
                    default: a.FieldValue,
                    type: 'string',
                    format: 'month',
                  };
                  break;
                case 'yyyy':
                  properties[a.FieldCode] = {
                    type: a.FieldValueTypeName,
                    title: a.FieldName,
                    default: a.FieldValue,
                    ui: {
                      widget: 'date',
                      format: Schame.IotSharp.format,
                      mode: 'year',
                    } as SFDateWidgetSchema,
                  };
                  break;
                case 'week':
                  properties[a.FieldCode] = {
                    type: 'string',
                    format: 'week',
                    title: a.FieldName,
                    default: a.FieldValue,
                  };
                  break;
                case 'range':
                  var dpv = [];
                  try {
                    dpv = JSON.parse(a.FieldValue);
                  } catch (error) { }

                  properties[a.FieldCode] = {
                    type: a.FieldValueTypeName,
                    title: a.FieldName,
                    default: dpv,
                    ui: {
                      widget: 'date',
                      mode: 'range',
                    } as SFDateWidgetSchema,
                  };
                  break;
                case 'Inline':
                  properties[a.FieldCode] = {
                    type: a.FieldValueTypeName,
                    title: a.FieldName,
                    default: a.FieldValue,
                    ui: {
                      widget: 'date',
                      inline: true,
                    } as SFDateWidgetSchema,
                  };
                  break;
              }

              break;
            case 10:
              properties[a.FieldCode] = {
                type: a.FieldValueTypeName,
                title: a.FieldName,
                default: a.FieldValue,
                ui: {
                  widget: 'time',
                  placeholder: Schame.ui.placeholder,
                  format: Schame.ui.format,
                  utcEpoch: Schame.ui.utcEpoch,
                  allowEmpty: Schame.ui.allowEmpty,
                  use12Hours: Schame.ui.use12Hours,
                },
              };

              break;

            case 11:
              if (!Schame.ui) {
                Schame.ui = {};
              } else {
              }
              properties[a.FieldCode] = {
                type: a.FieldValueTypeName,
                title: a.FieldName,
                minimum: Schame.minimum,
                maximum: Schame.maximum,
                multipleOf: Schame.multipleOf,
                default: a.FieldValue ? JSON.parse(a.FieldValue) : '',
                ui: {
                  widget: 'slider',
                  range: Schame.ui.range,
                } as SFSliderWidgetSchema,
              };

              break;
            case 12:
              if (Schame.IotSharp && Schame.IotSharp.allowsearch) {
                properties[a.FieldCode] = {
                  type: a.FieldValueTypeName,
                  title: a.FieldName,
                  default: a.FieldValue,

                  ui: {
                    widget: 'select',
                    serverSearch: true,
                    searchDebounceTime: 300,
                    searchLoadingText: '搜索中...',
                    onSearch: (q) => {
                      return this._httpClient
                        .get(a.DataSource + '?q=' + q)
                        .pipe(
                          map((x) => {
                            return x.Result;
                          }),
                        )
                        .toPromise();
                      // this._httpClient.get(a.DataSource).pipe(
                      //   map((x) => {
                      //     return x.Result.map((y) => {
                      //       return { value: y[Schame.IotSharp.key], label: y[Schame.IotSharp.value] };
                      //     });
                      //   }),
                      // );
                    },
                  } as SFSelectWidgetSchema,
                };
              } else {
                properties[a.FieldCode] = {
                  type: a.FieldValueTypeName,
                  title: a.FieldName,
                  default: a.FieldValue,
                  ui: {
                    widget: 'select',
                    asyncData: () =>
                      this._httpClient.get(a.DataSource).pipe(
                        map((x) => {
                          return x.Result;
                        }),
                      ),
                    // this._httpClient.get(a.DataSource).pipe(
                    //   map((x) => {
                    //     return x.Result.map((y) => {
                    //       return { value: y[Schame.IotSharp.key], label: y[Schame.IotSharp.value] };
                    //     });
                    //   }),
                    // ),
                  } as SFSelectWidgetSchema,
                };
              }

              break;
            case 13:
              properties[a.FieldCode] = {
                type: a.FieldValueTypeName,
                title: a.FieldName,
                default: a.FieldValue ? JSON.parse(a.FieldValue) : '',
                ui: {
                  widget: 'cascader',
                  titles: ['未拥有', '已拥有'],
                  asyncData: (node, index) => {
                    return this._httpClient
                      .get(a.DataSource)
                      .toPromise()
                      .then((res) => {
                        node.children = res;
                      });
                  },
                } as unknown as SFTransferWidgetSchema,
              };
              break;

            case 14:
              properties[a.FieldCode] = {
                type: a.FieldValueTypeName,
                title: a.FieldName,
                default: a.FieldValue ? JSON.parse(a.FieldValue) : '',

                ui: {
                  widget: 'tree-select',
                  checkable: true,
                  asyncData: () =>
                    this._httpClient.get(a.DataSource).pipe(
                      map((x) => {
                        return x;
                      }),
                    ),
                  //暂时取消异步支持，值可以绑定，但显示
                  // expandChange: (e) => {
                  //   if (e.eventName === 'expand') {
                  //     return this._httpClient.get<SFSchemaEnumType>(a.DataSource + '?id=' + e.node.key).pipe(
                  //       map((x) => {
                  //         return { title: x[Schame.IotSharp.value], value: x[Schame.IotSharp.key] };
                  //       }),
                  //     );
                  //   }
                  //   return of([]);
                  // }    ,
                } as SFTreeSelectWidgetSchema,
              };
              break;
            case 15:
              properties[a.FieldCode] = {
                type: a.FieldValueTypeName,
                title: a.FieldName,
                default: a.FieldValue ? JSON.parse(a.FieldValue) : '',
                ui: {
                  widget: 'transfer',
                  asyncData: () =>
                    this._httpClient.get(a.DataSource).pipe(
                      map((x) => {
                        return x.Result;
                      }),
                    ),
                  // this._httpClient.get(a.DataSource).pipe(
                  //   map((x) => {
                  //     return x.Result.map((y) => {
                  //       return { value: y[Schame.IotSharp.key], title: y[Schame.IotSharp.value] };
                  //     });
                  //   }),
                  // ),
                } as SFTransferWidgetSchema,
              };

              break;
            case 16:
              if (a.FieldValue && a.FieldValue !== '[]') {
                let filelist = JSON.parse(JSON.parse(a.FieldValue));
                if (filelist.length > 0) {
                  let _filelist = filelist.map((x) => {
                    return { name: x.fileName, url: x.url };
                  });

                  properties[a.FieldCode] = {
                    type: a.FieldValueTypeName,
                    title: a.FieldName,
                    default: a.FieldValue,
                    ui: {
                      widget: 'upload',
                      action: Schame.ui.action ? Schame.ui.action : 'api/common/attachment/upLoaderFile',
                      format: Schame.ui.format,
                      text: Schame.ui.text,
                      fileSize: Schame.ui.fileSize,
                      multiple: Schame.ui.multiple,
                      fileType: Schame.ui.fileType,
                      fileList: _filelist,
                    } as SFUploadWidgetSchema,
                  };
                }
              } else {
                properties[a.FieldCode] = {
                  type: a.FieldValueTypeName,
                  title: a.FieldName,
                  default: a.FieldValue,
                  ui: {
                    widget: 'upload',
                    action: Schame.ui.action ? Schame.ui.action : 'api/common/attachment/upLoaderFile',
                    format: Schame.ui.format,
                    text: Schame.ui.text,
                    fileSize: Schame.ui.fileSize,
                    multiple: Schame.ui.multiple,
                    fileType: Schame.ui.fileType,
                  } as SFUploadWidgetSchema,
                };
              }

              break;
            case 17:
              if (!Schame.ui) {
                Schame.ui = {
                  autosize: {},
                };
              }
              properties[a.FieldCode] = {
                type: 'string',
                title: a.FieldName,
                maxLength: Schame.maxLength,
                default: a.FieldValue,
                ui: {
                  borderless: Schame.ui?.borderless ?? false,
                  placeholder: Schame.ui?.placeholder,
                  widget: 'textarea',
                  autosize: {
                    minRows: Schame.ui?.autosize.minRows ?? 2,

                    maxRows: Schame.ui?.autosize.maxRows ?? 4,
                  },
                },
              };
              console.log(properties[a.FieldCode]);
              break;

            case 18:
              properties[a.FieldCode] = {
                type: 'string',
                title: a.FieldName,
                default: a.FieldValue,
                ui: {
                  widget: 'ueditor',
                },
              };

              break;

            case 19:
              //Schame.IotSharp.key
              properties[a.FieldCode] = {
                type: 'number',
                title: a.FieldName,
                maximum: Schame.maximum,
                multipleOf: Schame.multipleOf,
                default: a.FieldValue,
                ui: {
                  widget: 'rate',
                },
              };

              break;
          }
        }

        this.schema.properties = properties;
        this.sf.refreshSchema();
        this.cd.detectChanges();
      },
      (y) => { },
      () => { },
    );
  }

  ngOnInit(): void { }
  schema: SFSchema = {
    mode: 'search',
    properties: {},
  };

  submit(value: any) {
    console.log(value)
    this.onsubmit.emit(value);
    return;
    this._httpClient.post('api/dynamicforminfo/saveparams', { form: value, id: this.id }).subscribe(
      (x) => { },
      (y) => { },
      () => { },
    );
  }
}
