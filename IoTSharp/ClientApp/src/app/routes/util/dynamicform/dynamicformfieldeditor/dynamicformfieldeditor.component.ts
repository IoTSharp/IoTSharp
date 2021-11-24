import {
  ChangeDetectorRef,
  Component,
  ComponentFactory,
  ComponentFactoryResolver,
  ComponentRef,
  Input,
  OnInit,
  ViewChild,
  ViewContainerRef,
} from '@angular/core';
import { FormGroup } from '@angular/forms';
import { SFObjectWidgetSchema, SFSelectWidgetSchema } from '@delon/form';
import { _HttpClient } from '@delon/theme';
import { NzDrawerRef } from 'ng-zorro-antd/drawer';
import { concat } from 'rxjs';
import { map } from 'rxjs/operators';
import { UIDataType } from 'src/app/routes/common/UIDataType';
import { FieldpartComponent, FormField } from '../fieldpart/fieldpart.component';
import { fielddirective } from '../fieldpartdirective';

@Component({
  selector: 'app-dynamicformfieldeditor',
  templateUrl: './dynamicformfieldeditor.component.html',
  styleUrls: ['./dynamicformfieldeditor.component.less'],
})
export class DynamicformfieldeditorComponent implements OnInit {
  @Input() id: Number = -1;
  @ViewChild(fielddirective, { static: true })
  propertitycontainer: fielddirective;
  form!: FormGroup;
  componentFactory: ComponentFactory<FieldpartComponent>;
  viewContainerRef: ViewContainerRef;
  FieldDatas: FieldData[] = [];
  SuportType: UIDataType[] = [];
  AllControlType: any = [];
  AllSuportType: any = [];
  constructor(private componentFactoryResolver: ComponentFactoryResolver, private http: _HttpClient, private cd: ChangeDetectorRef, private drawerRef: NzDrawerRef<string>) {}
  ngOnDestroy(): void {}
  ngOnInit(): void {
    this.componentFactory = this.componentFactoryResolver.resolveComponentFactory(FieldpartComponent);
    this.viewContainerRef = this.propertitycontainer.viewContainerRef;

    //一定要顺序发射指令
    concat(
      this.http.post('api/dictionary/index', { DictionaryGroupId: 2, pi: 0, ps: 20, limit: 20, offset: 0 }).pipe(
        map((x) => {
          this.AllSuportType = x.data.rows.map((x) => {
            return { label: x.dictionaryName, value: x.dictionaryValue };
          });
        }),
      ),
      this.http.post('api/dictionary/index', { DictionaryGroupId: 1, pi: 0, ps: 20, limit: 20, offset: 0 }).pipe(
        map((x) => {
          this.AllControlType = x.data.rows.map((x) => {
            return { label: x.dictionaryName, value: x.dictionaryValue };
          });
        }),
      ),
      this.http.get('api/dynamicforminfo/getParams?id=' + this.id).pipe(
        map((x) => {
          if (x.data.propdata) {
            x.data.propdata = x.data.propdata.map((x) => {
              return {
                FieldId: x.fieldId,
                FieldName: x.fieldName,
                FieldValue: x.fieldValue,
                FieldValueType: x.fieldValueType,
                FieldValueDataSource: x.fieldValueDataSource,
                FieldUIElementSchema: x.fieldUIElementSchema,
                FieldUIElement: x.fieldUIElement,
                FieldCode: x.fieldCode,
                FieldUnit: x.fieldUnit,
                IsRequired: x.isRequired,
              };
            });

            for (var i = 0; i < x.data.propdata.length; i++) {
              //始终从头插入
              const componentRef = this.viewContainerRef.createComponent<FieldpartComponent>(this.componentFactory, 0);
              let key = this.makeString();
              componentRef.instance.AllSuportType = this.AllSuportType;
              componentRef.instance.AllControlType = this.AllControlType;
              let field = new FormField(
                key,
                x.data.propdata[i].FieldId,
                x.data.propdata[i].FieldName,
                x.data.propdata[i].FieldValue,
                x.data.propdata[i].FieldValueType,
                x.data.propdata[i].FieldValueDataSource,
                x.data.propdata[i].FieldUIElementSchema,
                x.data.propdata[i].FieldUIElement + '',
                x.data.propdata[i].FieldUnit,
                {
                  properties: this.createschema(x.data.propdata[i].FieldUIElement, x.data.propdata[i].FieldUIElementSchema),
                },
                {},
                //  () => {},
                x.data.propdata[i].FieldCode,
                x.data.propdata[i].IsRequired,
              );

            
              componentRef.instance.FormField = field;
              componentRef.instance.OnRemove.subscribe((x) => {
                var index = this.FieldDatas.findIndex((c) => c.Key == x.Key);
                if (index > -1) {
                  this.viewContainerRef.remove(index);
                  this.FieldDatas.splice(index, 1);
                  this.cd.detectChanges();
                }
              });
              this.FieldDatas = [new FieldData(field, componentRef, key), ...this.FieldDatas];
            }
          }

          this.cd.detectChanges();
        }),
      ),
    ).subscribe();
  }

  creat() {
    let key = this.makeString();
    const componentRef = this.viewContainerRef.createComponent<FieldpartComponent>(this.componentFactory, 0);
    let e = new FormField(
      key,
      0,
      '',
      '',
      '0',
      '',
      '',
      '0',
      '',
      {
        properties: {},
      },
      {},
      //  () => {},
      '',
      false,
    );
    componentRef.instance.AllSuportType = this.AllSuportType;
    componentRef.instance.FormField = e;
    componentRef.instance.AllControlType = this.AllControlType;
    componentRef.instance.OnRemove.subscribe((x) => {
      var index = this.FieldDatas.findIndex((c) => c.Key == x.Key);
      if (index > -1) {
        this.viewContainerRef.remove(index);
        this.FieldDatas.splice(index, 1);
        this.cd.detectChanges();
      }
    });

    this.FieldDatas = [new FieldData(e, componentRef, key), ...this.FieldDatas];
  }

  saveData() {
    var keys: string[] = [];
    for (let item of this.FieldDatas) {
      if (!item.prop.FieldName) {
        item.prop.FieldCodenzValidatingTip = '参数名称不能为空';
        item.prop.FieldCodenzValidateStatus = 'error';
        item.componentRef.location.nativeElement.scrollIntoView();
        return;
      }
      if (!item.prop.FieldCode) {
        item.prop.FieldCodenzValidatingTip = '索引不能为空';
        item.prop.FieldCodenzValidateStatus = 'error';
        item.componentRef.location.nativeElement.scrollIntoView();
        return;
      }


      if (item.prop.FieldValueType==='0') {
        item.prop.FieldValueTypenzValidatingTip = '数据类型不能为空';
        item.prop.FieldValueTypenzValidateStatus = 'error';
        item.componentRef.location.nativeElement.scrollIntoView();
        return;
      }
      if (keys.filter((x) => x == item.prop.FieldCode).length > 0) {
        item.prop.FieldCodenzValidatingTip = '索引重复';
        item.prop.FieldCodenzValidateStatus = 'error';
        item.componentRef.location.nativeElement.scrollIntoView();
        return;
      } else {
        keys = [...keys, item.prop.FieldCode];
      }

      item.componentRef.instance.submit();
    }

    // of(...this.ParamDatas)
    //   .pipe(
    //     groupBy((x) => x.prop.DeviceTypeParamCode),
    //     mergeMap((group$) => group$.pipe(reduce((acc, cur) => [...acc, cur], []))),
    //   )
    //   .subscribe((x) => {

    //   });

    this.http
      .post('api/dynamicforminfo/saveparams', {
        Id: this.id,
        propdata: this.FieldDatas.map((x) => {
          return {
            FieldId: x.prop.FieldId,
            FieldName: x.prop.FieldName,
            FieldUIElementSchema: JSON.stringify(x.prop.formdata),
            FieldValue: x.prop.FieldValue,
            FieldValueDataSource: x.prop.FieldValueDataSource,
            FieldValueType: x.prop.FieldValueType,
            FieldUIElement: x.prop.FieldUIElement,
            FieldCode: x.prop.FieldCode,
            FieldUnit: x.prop.FieldUnit,
            IsRequired: x.prop.IsRequired,
          };
        }),
      })
      .subscribe(
        (x) => {},
        (y) => {},
        () => { this.drawerRef.close(this.id);},
      );
  }

  makeString(): string {
    let outString: string = '';
    let inOptions: string = 'abcdefghijklmnopqrstuvwxyz0123456789';
    for (let i = 0; i < 32; i++) {
      outString += inOptions.charAt(Math.floor(Math.random() * inOptions.length));
    }
    return outString;
  }

  private createschema(type: Number, schema: string): any {
    let Schema = JSON.parse(schema);

    switch (type) {
      case 1:
        return {
          exclusiveMinimum: {
            type: 'boolean',
            title: '是否有最小值限制',
            default: Schema.exclusiveMinimum,
          },
          exclusiveMaximum: {
            type: 'boolean',
            title: '是否有最大值限制',
            default: Schema.exclusiveMaximum,
          },

          minimum: {
            type: 'number',
            title: '最小值',
            maxLength: 20,
            minLength: 1,
            default: Schema.minimum,
          },
          maximum: {
            type: 'number',
            title: '最大值',
            maxLength: 20,
            minLength: 1,
            default: Schema.maximum,
          },
          ui: {
            title: 'UI属性',
            type: 'object',
            properties: {
              widgetWidth: {
                type: 'number',
                title: '宽度',
                default: Schema.ui.widgetWidth,
              },
            },
            ui: {
              type: 'card',
            } as SFObjectWidgetSchema,
          },
        };

        break;
      case 2:
        return {
          maxLength: {
            type: 'number',
            title: '字符长度限制',
            default: Schema.maxLength,
          },

          pattern: {
            type: 'string',
            title: '校验正则表达式',
            maxLength: 50,
            minLength: 1,
            default: Schema.pattern,
          },

          ui: {
            title: 'UI属性',
            type: 'object',
            properties: {
              addOnAfter: {
                type: 'string',
                title: '后缀',
                maxLength: 20,
                minLength: 1,
                default: Schema.ui.addOnAfter,
              },
              placeholder: {
                type: 'string',
                title: 'placeholder',
                maxLength: 20,
                minLength: 1,
                default: Schema.ui.placeholder,
              },
            },
            ui: {
              type: 'card',
            } as SFObjectWidgetSchema,
          },
        };
        break;
      case 3:
        return {};
        break;
      case 4:
        return {};
        break;
      case 5:
        return {
          ui: {
            title: 'UI属性',
            type: 'object',
            properties: {
              checkedChildren: {
                type: 'string',
                title: '打开值',
                maxLength: 20,
                minLength: 1,
                default: Schema.ui.checkedChildren,
              },
              unCheckedChildren: {
                type: 'string',
                title: '关闭值',
                maxLength: 20,
                minLength: 1,
                default: Schema.ui.unCheckedChildren,
              },
            },
            ui: {
              type: 'card',
            } as SFObjectWidgetSchema,
          },
        };
        break;
      case 6:
        return {
          ui: {
            title: 'UI属性',
            type: 'object',
            properties: {
              span: {
                type: 'number',
                title: '每个选框单元格数量',
                default: Schema.ui.span,
              },
              styleType: {
                type: 'string',
                title: 'radio的样式',
                default: Schema.ui.styleType,
                enum: [
                  { label: '默认', value: 'default' },
                  { label: '按钮', value: 'button' },
                ],
              },
              checkAll: {
                type: 'boolean',
                title: '是否需要全选',
                default: Schema.ui.checkAll,
              },
            },
            ui: {
              type: 'card',
            } as SFObjectWidgetSchema,
          },
          // IotSharp: {
          //   title: '扩展属性',
          //   type: 'object',

          //   ui: {
          //     type: 'card',
          //   } as SFObjectWidgetSchema,
          //   properties: {
          //     key: {
          //       type: 'string',
          //       title: '值',
          //       default: Schema.IotSharp.key,
          //     },
          //     value: {
          //       type: 'string',
          //       title: '名称',
          //       default: Schema.IotSharp.value,
          //     },
          //   },
          // },
        };
        break;
      case 7:
        return {
          ui: {
            title: 'UI属性',
            type: 'object',
            properties: {
              format: {
                type: 'string',
                title: '时间格式',
                default: Schema.ui.format,
              },
              use12Hours: {
                type: 'boolean',
                title: '是否使用12小时',
                default: Schema.ui.use12Hours,
              },
              size: {
                type: 'number',
                title: '尺寸',
                default: Schema.ui.size,
              },
            },
            ui: {
              type: 'card',
            } as SFObjectWidgetSchema,
          },
        };
        break;
      case 8:
        return {
          ui: {
            title: 'UI属性',
            type: 'object',
            properties: {
              styleType: {
                type: 'string',
                title: 'radio 的样式',
                default: Schema.ui.styleType,
                enum: [
                  { label: '默认', value: 'default' },
                  { label: '按钮', value: 'button' },
                ],
              },
              buttonStyle: {
                type: 'boolean',
                title: 'Button风格样式',
                default: Schema.ui.buttonStyle,
                enum: [
                  { label: 'outline', value: 'outline' },
                  { label: 'solid', value: 'solid' },
                ],
              },
            },
            ui: {
              type: 'card',
            } as SFObjectWidgetSchema,
          },
          // IotSharp: {
          //   title: '扩展属性',
          //   type: 'object',

          //   ui: {
          //     type: 'card',
          //   } as SFObjectWidgetSchema,
          //   properties: {
          //     key: {
          //       default: Schema.IotSharp.key,
          //       type: 'string',
          //       title: '值',
          //     },
          //     value: {
          //       default: Schema.IotSharp.value,
          //       type: 'string',
          //       title: '名称',
          //     },
          //   },
          // },
        };
        break;
      case 9:
        return {
          IotSharp: {
            title: '扩展属性',
            type: 'object',

            ui: {
              change: (item) => {
                this.GetTargetType('9', item);
              },
              type: 'card',
            } as SFObjectWidgetSchema,
            properties: {
              format: {
                default: Schema.IotSharp.format,
                type: 'string',
                title: '格式',
                enum: [
                  { label: '时间日期', value: 'date-time' },
                  { label: '日期', value: 'date' },
                  { label: '年月', value: 'month' },
                  { label: '年', value: 'yyyy' },
                  { label: '周', value: 'week' },
                  { label: '区间', value: 'range' },
                  { label: 'Inline显示', value: 'Inline' },
                ],
              },
            },
          },
        };
        break;
      case 10:
        return {
          ui: {
            title: 'UI属性',
            type: 'object',
            properties: {
              placeholder: {
                default: Schema.ui.placeholder,
                type: 'string',
                title: 'placeholder',
              },
              format: {
                default: Schema.ui.format,
                type: 'string',
                title: '数据格式化',
              },
              utcEpoch: { default: Schema.ui.utcEpoch, type: 'boolean', title: '是否UTC' },
              allowEmpty: { default: Schema.ui.allowEmpty, type: 'boolean', title: '否展示清除按钮' },
              use12Hours: { default: Schema.ui.use12Hours, type: 'boolean', title: '使用12小时' },
            },
            ui: {
              type: 'card',
            } as SFObjectWidgetSchema,
          },
        };
        break;

      case 11:
        if (!Schema.ui) {
          Schema.ui = {};
        }
        return {
          minimum: {
            type: 'number',
            title: '最小值',
            default: Schema.minimum,
          },
          maximum: {
            type: 'number',
            title: '最大值',
            default: Schema.maximum,
          },

          multipleOf: {
            type: 'number',
            title: '值',
            default: Schema.multipleOf ? Schema.multipleOf : 1,
          },

          ui: {
            title: 'UI属性',
            type: 'object',

            properties: {
              range: {
                //布尔值开关没有Change事件
                type: 'boolean',
                title: '开启双滑块',
                default: Schema.ui.range ? Schema.ui.range : false,
                enum: [
                  { label: '是', value: true },
                  { label: '否', value: false },
                ],
                ui: {
                  widget: 'select',
                  change: (value, orgData) => {
                    if (value) {
                    } else {
                    }
                  },
                } as SFSelectWidgetSchema,
              },
              vertical: {
                type: 'boolean',
                title: '是否垂直',
                default: Schema.ui.vertical ? Schema.ui.vertical : false,
              },
            },
            ui: {
              type: 'card',
            } as SFObjectWidgetSchema,
          },
        };
        break;
      case 12:
        return {
          IotSharp: {
            title: '扩展属性',
            type: 'object',
            ui: {
              type: 'card',
            } as SFObjectWidgetSchema,
            properties: {
              // key: {
              //   type: 'string',
              //   title: '值',
              //   default: Schema.IotSharp.key,
              // },
              // value: {
              //   type: 'string',
              //   title: '名称',
              //   default: Schema.IotSharp.value,
              // },
              allowsearch: {
                type: 'boolean',
                title: '是否支持搜索',
                default: Schema.IotSharp ? Schema.IotSharp.allowsearch : false,
              },
            },
          },
        };
        break;
      case 13:
        return {};
        break;
      case 14:
        return {
          // IotSharp: {
          //   title: '扩展属性',
          //   type: 'object',
          //   ui: {
          //     type: 'card',
          //   } as SFObjectWidgetSchema,
          //   properties: {
          //     key: {
          //       type: 'string',
          //       title: '值',
          //       default: Schema.IotSharp.key,
          //     },
          //     value: {
          //       type: 'string',
          //       title: '名称',
          //       default: Schema.IotSharp.value,
          //     },
          //     parent: {
          //       type: 'string',
          //       title: '父级Id',
          //       default: Schema.IotSharp.parent,
          //     },
          //   },
          // },
        };
        break;
      case 15:
        return {
          // IotSharp: {
          //   title: '扩展属性',
          //   type: 'object',
          //   ui: {
          //     type: 'card',
          //   } as SFObjectWidgetSchema,
          //   properties: {
          //     key: {
          //       type: 'string',
          //       title: '值',
          //       default: Schema.IotSharp.key,
          //     },
          //     value: {
          //       type: 'string',
          //       title: '名称',
          //       default: Schema.IotSharp.value,
          //     },
          //     parent: {
          //       type: 'string',
          //       title: '名称',
          //       default: Schema.IotSharp.parent,
          //     },
          //   },
          // },
        };
        break;

      case 16:
        return {
          ui: {
            title: 'UI属性',
            type: 'object',
            properties: {
              format: {
                type: 'string',
                title: '上传方式',
                default: Schema.ui.format,
                enum: [
                  { label: '选择', value: 'select' },
                  { label: '拖放', value: 'drag' },
                ],
              },

              action: {
                type: 'string',
                title: '上传地址',
                default: Schema.ui.action ? Schema.ui.action : 'api/common/attachment/upLoaderFile',
              },
              text: {
                type: 'string',
                title: '按钮文本',
                default: Schema.ui.text,
              },
              fileSize: {
                type: 'number',
                title: '文件大小限制（kb）',
                default: Schema.ui.fileSize,
              },
              fileType: {
                type: 'string',
                title: '文件类型',
                default: Schema.ui.fileType,
                enum: [
                  { label: '不限', value: '' },
                  { label: 'png', value: 'image/png' },
                  { label: 'jpeg', value: 'image/jpeg' },
                  { label: 'gif', value: 'image/gif' },
                  { label: 'bmp', value: 'image/bmp' },
                ],
              },
              multiple: {
                type: 'boolean',
                title: '允许多上传',
                default: Schema.ui.multiple,
              },
            },
            ui: {
              type: 'card',
            } as SFObjectWidgetSchema,
          },
        };
        break;

      case 17:
        return {
          maxLength: {
            title: '表单最大长度',
            type: 'number',
            default: Schema.maxLength,
          },
          ui: {
            title: 'UI属性',
            type: 'object',
            properties: {
              borderless: {
                title: '隐藏边框',
                type: 'boolean',
                default: Schema.ui.borderless,
              },
              placeholder: {
                title: 'placeholder',
                type: 'string',
                default: Schema.ui.placeholder,
              },

              autosize: {
                type: 'object',
                title: '自适应内容高度',
                properties: {
                  minRows: {
                    type: 'number',
                    title: '最小行',
                    default: Schema.ui.autosize.minRows,
                  },
                  maxRows: {
                    type: 'number',
                    title: '最大行',
                    default: Schema.ui.autosize.maxRows,
                  },
                },
              },
            },
            ui: {
              type: 'card',
            } as SFObjectWidgetSchema,
          },
        };
        break;
      case 18:
        return {};
        break;
      case 19:
        return {
          maximum: {
            type: 'number',
            title: '最大值',
            default: Schema.maximum,
          },
          multipleOf: {
            type: 'number',
            title: '允许半星',
            default: Schema.multipleOf,
            enum: [
              { label: '是', value: 0.5 },
              { label: '否', value: 1 },
            ],
          },
        };
        break;


        case 20:
          return {}; 
         

    }
  }

  GetTargetType(type: string, format: string) {
    switch (type) {
      case '1':
        this.SuportType = this.AllSuportType.filter((c) => c.value === '1' || c.value === '2' || c.value === '3' || c.value === '9');
        break;
      case '2':
        this.SuportType = this.AllSuportType.filter((c) => c.value === '4');
        break;
      case '3':
        this.SuportType = this.AllSuportType.filter((c) => c.value === '4');
        break;
      case '4':
        this.SuportType = this.AllSuportType.filter((c) => c.value === '4');
        break;

      case '5':
        this.SuportType = this.AllSuportType.filter((c) => c.value === '13');
        break;
      case '6':
        this.SuportType = this.AllSuportType.filter(
          (c) => c.value === '6' || c.value === '7' || c.value === '8' || c.value === '10' || c.value === '12',
        );
        break;
      case '7':
        this.SuportType = this.AllSuportType.filter((c) => c.value === '4');
        break;
      case '8':
        this.SuportType = this.AllSuportType.filter(
          (c) => c.value === '1' || c.value === '2' || c.value === '3' || c.value === '4' || c.value === '9',
        );
        break;

      case '9':
        switch (format) {
          case 'date-time':
            this.SuportType = this.AllSuportType.filter((c) => c.value === '5' || c.value === '15');
            break;
          case 'date':
            this.SuportType = this.AllSuportType.filter((c) => c.value === '5' || c.value === '4' || c.value === '15');
            break;
          case 'month':
            this.SuportType = this.AllSuportType.filter((c) => c.value === '5' || c.value === '4' || c.value === '15');
            break;
          case 'yyyy':
            this.SuportType = this.AllSuportType.filter((c) => c.value === '1');
            break;
          case 'week':
            this.SuportType = this.AllSuportType.filter((c) => c.value === '4');
            break;
          case 'range':
            this.SuportType = this.AllSuportType.filter((c) => c.value === '11' || c.value === '7'); //ranger暂时支持 DataTime和String，
            break;
          case 'Inline':
            this.SuportType = this.AllSuportType.filter((c) => c.value === '5' || c.value === '4' || c.value === '15');
            break;
        }

        break;
      case '10':
        this.SuportType = this.AllSuportType.filter((c) => c.value === '4');
        break;
      case '11':
        switch (format) {
          case 'true':
            this.SuportType = this.AllSuportType.filter((c) => c.value === '6' || c.value === '8' || c.value === '10' || c.value === '12');

            break;
          case 'false':
            this.SuportType = this.AllSuportType.filter((c) => c.value === '1' || c.value === '2' || c.value === '3' || c.value === '9');

            break;
        }

        break;

      case '12':
        this.SuportType = this.AllSuportType.filter(
          (c) => c.value === '1' || c.value === '2' || c.value === '3' || c.value === '4' || c.value === '9',
        );
        break;
      case '13':
        this.SuportType = this.AllSuportType.filter((c) => c.value === '6' || c.value === '8' || c.value === '7' || c.value === '12');
        break;
      case '14':
        this.SuportType = this.AllSuportType.filter((c) => c.value === '6' || c.value === '8' || c.value === '7' || c.value === '12');
        break;
      case '15':
        this.SuportType = this.AllSuportType.filter((c) => c.value === '6' || c.value === '8' || c.value === '7' || c.value === '12');
        break;
      case '16':
        this.SuportType = this.AllSuportType.filter((c) => c.value === '14');
        break;
      case '17':
        this.SuportType = this.AllSuportType.filter((c) => c.value === '4');
        break;
      case '18':
        this.SuportType = this.AllSuportType.filter((c) => c.value === '4');
        break;
      case '19':
        this.SuportType = this.AllSuportType.filter((c) => c.value === '1' || c.value === '2' || c.value === '3' || c.value === '9');
        break;
        case '20':
          this.SuportType = this.AllSuportType.filter((c) => c.value === '4'); 
               break;
    }

    //  this.AllSuportType.filter(c=>c.value===)
    // this.http.get('api/common/dictionaryservice/gettargettype?id=' + type + '&format=' + format).subscribe(
    //   (x) => {
    //     this.AllSuportType = x.data;
    //   },
    //   (y) => {},
    //   () => {},
    // );
  }
}
export class FieldData {
  constructor(public prop: FormField, public componentRef: ComponentRef<FieldpartComponent>, public Key: string) {}
}
