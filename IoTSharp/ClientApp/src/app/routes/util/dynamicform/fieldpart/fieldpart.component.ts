import { ChangeDetectorRef, Component, EventEmitter, Input, OnInit, Output, SimpleChanges, ViewChild } from '@angular/core';
import { SFComponent, SFObjectWidgetSchema, SFSelectWidgetSchema, SFSchema } from '@delon/form';
import { _HttpClient } from '@delon/theme';
import { UIDataType } from 'src/app/routes/common/UIDataType';

@Component({
  selector: 'app-fieldpart',
  templateUrl: './fieldpart.component.html',
  styleUrls: ['./fieldpart.component.less'],
})
export class FieldpartComponent implements OnInit {
  @ViewChild('sf', { static: false }) sf: SFComponent;
  @Output() OnRemove = new EventEmitter<FormField>();
  @Input()
  FormField: FormField;
  @Input()
  AllSuportType: UIDataType[] = [];
  @Input()
  AllControlType: any = [];

  SuportType: any = [];

  DSVisble: boolean = false;
  DefaultVisble: boolean = false;
  constructor(private http: _HttpClient, private cd: ChangeDetectorRef) {}

  ngOnChanges(changes: SimpleChanges): void {}

  writeValue(obj: any): void {
    this.FormField = obj;

    //
  }

  registerOnChange(fn: any): void {
    //  this.onChange = fn;
  }
  registerOnTouched(fn: any): void {}
  setDisabledState?(isDisabled: boolean): void {}

  ngOnInit(): void {

    switch (this.FormField.FieldUIElement) {
      case '1':
        this.DSVisble = false;
        this.GetTargetType(this.FormField.FieldUIElement, '');
        break;
      case '2':
        this.DSVisble = false;
        this.GetTargetType(this.FormField.FieldUIElement, '');
        break;
      case '3':
        this.DSVisble = false;
        this.GetTargetType(this.FormField.FieldUIElement, '');
        break;
      case '4':
        this.DSVisble = false;
        this.GetTargetType(this.FormField.FieldUIElement, '');
        break;
      case '5':
        this.DSVisble = false;
        this.GetTargetType(this.FormField.FieldUIElement, '');
        break;
      case '6':
        this.DSVisble = true;
        this.GetTargetType(this.FormField.FieldUIElement, '');
        break;
      case '7':
        this.DSVisble = false;
        this.GetTargetType(this.FormField.FieldUIElement, '');
        break;
      case '8':
        this.DSVisble = true;
        this.GetTargetType(this.FormField.FieldUIElement, '');
        break;
      case '9':
        this.DSVisble = false;
        this.FormField.schema.properties.IotSharp.properties.format.ui = {
          widget: 'select',
          change: (x, y) => {
            this.GetTargetType('9', x);
          },
        };
        console.log(this.FormField.schema.properties.IotSharp.properties.format.ui);
        this.GetTargetType(this.FormField.FieldUIElement, this.FormField.schema.properties.IotSharp.properties.format.default);

        break;
      case '10':
        this.DSVisble = false;
        this.GetTargetType(this.FormField.FieldUIElement, '');
        break;
      case '11':
        this.DSVisble = false;

        if (this.FormField.schema.properties.ui.properties.range) {
          this.GetTargetType(this.FormField.FieldUIElement, 'true');
        } else {
          this.GetTargetType(this.FormField.FieldUIElement, 'false');
        }

        break;
      case '12':
        this.DSVisble = true;
        this.GetTargetType(this.FormField.FieldUIElement, '');
        break;
      case '13':
        this.DSVisble = true;
        this.GetTargetType(this.FormField.FieldUIElement, '');
        break;
      case '14':
        this.DSVisble = true;
        this.GetTargetType(this.FormField.FieldUIElement, '');
        break;
      case '15':
        this.DSVisble = true;
        this.GetTargetType(this.FormField.FieldUIElement, '');
        break;
      case '16':
        this.DSVisble = false;
        this.GetTargetType(this.FormField.FieldUIElement, '');
        break;
      case '17':
        this.DSVisble = false;
        this.GetTargetType(this.FormField.FieldUIElement, '');
        break;
      case '18':
        this.DSVisble = false;
        this.GetTargetType(this.FormField.FieldUIElement, '');
        break;
      case '19':
        this.DSVisble = false;
        this.GetTargetType(this.FormField.FieldUIElement, '');
        break;
    }
    this.cd.detectChanges();
  }

  getData() {}

  remove(e) {
    this.OnRemove.emit(this.FormField);
  }

  submit(): void {
    this.FormField.formdata = this.sf.value;
  }
  dtpChange(value: string) {
    this.FormField.FieldNamenzValidateStatus = '';
  }
  dtcChange(value: string) {
    this.FormField.FieldCodenzValidateStatus = '';
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
            console.log(format);
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
    }

    //  this.AllSuportType.filter(c=>c.value===)
    // this.http.get('api/common/dictionaryservice/gettargettype?id=' + type + '&format=' + format).subscribe(
    //   (x) => {
    //     this.AllSuportType = x.Result;
    //   },
    //   (y) => {},
    //   () => {},
    // );
  }

  FormFieldUITypeChange(value: string) {
    switch (value) {
      case '1':
        this.FormField.schema.properties = {
          // exclusiveMinimum: {
          //   type: 'boolean',
          //   title: '是否有最小值限制',
          // },
          // exclusiveMaximum: {
          //   type: 'boolean',
          //   title: '是否有最大值限制',
          // },

          minimum: {
            type: 'number',
            title: '最小值',
            maxLength: 20,
            minLength: 1,
          },
          maximum: {
            type: 'number',
            title: '最大值',
            maxLength: 20,
            minLength: 1,
          },
          ui: {
            title: 'UI属性',
            type: 'object',
            properties: {
              widgetWidth: {
                type: 'number',
                title: '宽度',
                default: 400,
              },
            },
            ui: {
              type: 'card',
            } as SFObjectWidgetSchema,
          },
        };
        this.sf.refreshSchema();
        this.GetTargetType(value, '');
        this.DSVisble = false;
        break;
      case '2':
        this.FormField.schema.properties = {
          maxLength: {
            type: 'number',
            title: '字符长度限制',
          },

          pattern: {
            type: 'string',
            title: '校验正则表达式',
            maxLength: 100,
            minLength: 1,
            default: '',
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
                default: '',
              },
              placeholder: {
                type: 'string',
                title: 'placeholder',
                maxLength: 20,
                minLength: 1,
                default: '',
              },
            },
            ui: {
              type: 'card',
            } as SFObjectWidgetSchema,
          },
        };
        this.sf.refreshSchema();
        this.DSVisble = false;
        this.GetTargetType(value, '');
        break;
      case '3':
        this.FormField.schema.properties = {};
        this.DSVisble = false;
        this.sf.refreshSchema();
        break;
      case '4':
        this.FormField.schema.properties = {};
        this.sf.refreshSchema();
        this.DSVisble = false;
        this.GetTargetType(value, '');
        break;
      case '5':
        this.FormField.schema.properties = {
          ui: {
            title: 'UI属性',
            type: 'object',
            properties: {
              checkedChildren: {
                type: 'string',
                title: '打开值',
                maxLength: 20,
                minLength: 1,
              },
              unCheckedChildren: {
                type: 'string',
                title: '关闭值',
                maxLength: 20,
                minLength: 1,
              },
            },
            ui: {
              type: 'card',
            } as SFObjectWidgetSchema,
          },
        };
        this.sf.refreshSchema();
        this.DSVisble = false;
        this.GetTargetType(value, '');
        this.DSVisble = false;
        break;
      case '6':
        this.FormField.schema.properties = {
          ui: {
            title: 'UI属性',
            type: 'object',
            properties: {
              span: {
                type: 'number',
                title: '每个选框单元格数量',
              },
              styleType: {
                type: 'string',
                title: 'radio的样式',
                enum: [
                  { label: '默认', value: 'default' },
                  { label: '按钮', value: 'button' },
                ],
              },
              checkAll: {
                type: 'boolean',
                title: '是否需要全选',
              },
            },
            ui: {
              type: 'card',
            } as SFObjectWidgetSchema,
          },
        };

        this.sf.refreshSchema();
        this.GetTargetType(value, '');
        this.DSVisble = true;
        break;
      case '7':
        this.FormField.schema.properties = {
          // format: {
          //   type: 'string',
          //   title: '时间格式',

          //   maxLength: 20,
          //   minLength: 1,
          // },

          ui: {
            title: 'UI属性',
            type: 'object',
            properties: {
              format: {
                type: 'string',
                title: '时间格式',
              },
              use12Hours: {
                type: 'boolean',
                title: '是否使用12小时',
              },
              size: {
                type: 'number',
                title: '尺寸',
              },
            },
            ui: {
              type: 'card',
            } as SFObjectWidgetSchema,
          },
        };
        this.GetTargetType(value, '');
        this.sf.refreshSchema();
        this.DSVisble = false;
        break;
      case '8':
        this.FormField.schema.properties = {
          ui: {
            title: 'UI属性',
            type: 'object',
            properties: {
              styleType: {
                type: 'string',
                title: 'radio 的样式',
                enum: [
                  { label: '默认', value: 'default' },
                  { label: '按钮', value: 'button' },
                ],
              },
              buttonStyle: {
                type: 'boolean',
                title: 'Button风格样式',
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
        };
        this.GetTargetType(value, '');
        this.sf.refreshSchema();
        this.DSVisble = true;
        break;
      case '9':
        this.FormField.schema.properties = {
          IotSharp: {
            title: '扩展属性',
            type: 'object',

            ui: {
              change: (item) => {
                console.log(item);
                this.GetTargetType(value, item);
              },
              type: 'card',
            } as SFObjectWidgetSchema,
            properties: {
              format: {
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
                ui: {
                  widget: 'select',
                  change: (x, y) => {
                    console.log(x);
                    this.GetTargetType('9', x);
                  },
                },
              },
            },
          },
        };
        this.DSVisble = false;
        this.sf.refreshSchema();
        this.GetTargetType(value, '');
        break;
      case '10':
        this.FormField.schema.properties = {
          ui: {
            title: 'UI属性',
            type: 'object',
            properties: {
              placeholder: {
                type: 'string',
                title: 'placeholder',
                default: '',
              },
              format: {
                type: 'string',
                title: '数据格式化',
                default: 'HH:mm:ss',
              },
              utcEpoch: {
                type: 'boolean',
                title: '是否UTC',
                default: false,
              },
              allowEmpty: {
                type: 'boolean',
                title: '否展示清除按钮',
                default: true,
              },
              use12Hours: {
                type: 'boolean',
                title: '使用12小时',
                default: false,
              },
            },
            ui: {
              type: 'card',
            } as SFObjectWidgetSchema,
          },
        };
        this.GetTargetType(value, '');
        this.sf.refreshSchema();
        this.DSVisble = false;
        break;
      case '11':
        this.FormField.schema.properties = {
          minimum: {
            type: 'number',
            title: '最小值',
          },
          maximum: {
            type: 'number',
            title: '最大值',
          },

          multipleOf: {
            type: 'number',
            title: '值',
          },

          ui: {
            title: 'UI属性',
            type: 'object',
            properties: {
              range: {
                //布尔值开关没有Change事件
                type: 'boolean',
                title: '开启双滑块',
                enum: [
                  { label: '是', value: true },
                  { label: '否', value: false },
                ],
                ui: {
                  widget: 'select',
                  change: (v, orgData) => {
                    if (v) {
                      this.GetTargetType('11', 'true');
                    } else {
                      this.GetTargetType('11', 'false');
                    }
                  },
                } as SFSelectWidgetSchema,
              },
              vertical: {
                type: 'boolean',
                title: '是否垂直',
              },
            },
            ui: {
              type: 'card',
            } as SFObjectWidgetSchema,
          },
        };

        this.sf.refreshSchema();
        this.DSVisble = false;
        break;
      case '12':
        this.FormField.schema.properties = {
          IotSharp: {
            title: '扩展属性',
            type: 'object',

            ui: {
              type: 'card',
            } as SFObjectWidgetSchema,
            properties: {
              allowsearch: {
                type: 'boolean',
                title: '是否支持搜索',
              },
            },
          },
        };
        this.GetTargetType(value, '');
        this.sf.refreshSchema();
        this.DSVisble = true;
        break;
      case '13':
        this.FormField.schema.properties = {};
        this.GetTargetType(value, '');
        this.sf.refreshSchema();
        this.DSVisble = true;
        break;
      case '14':
        this.FormField.schema.properties = {};
        this.GetTargetType(value, '');
        this.sf.refreshSchema();
        this.DSVisble = true;
        break;
      case '15':
        this.FormField.schema.properties = {};
        this.GetTargetType(value, '');
        this.sf.refreshSchema();
        this.DSVisble = true;
        break;
      case '16':
        this.FormField.schema.properties = {
          // format: {
          //   type: 'string',
          //   title: '时间格式',

          //   maxLength: 20,
          //   minLength: 1,
          // },

          ui: {
            title: 'UI属性',
            type: 'object',
            properties: {
              format: {
                type: 'string',
                title: '上传方式',
                enum: [
                  { label: '选择', value: 'select' },
                  { label: '拖放', value: 'drag' },
                ],
              },
              text: {
                type: 'string',
                title: '按钮文本',
                default: '文件上传',
              },
              action: {
                type: 'string',
                title: '上传地址',
                default: 'api/attachment/upLoaderFile',
              },
              fileSize: {
                type: 'number',
                title: '文件大小限制（kb）',
              },
              fileType: {
                type: 'string',
                title: '文件类型',
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
              },
            },
            ui: {
              type: 'card',
            } as SFObjectWidgetSchema,
          },
        };
        this.GetTargetType(value, '');
        this.sf.refreshSchema();
        this.DSVisble = false;
        break;
      case '17':
        this.FormField.schema.properties = {
          maxLength: {
            title: '表单最大长度',
            type: 'number',
          },
          ui: {
            title: 'UI属性',
            type: 'object',
            properties: {
              borderless: {
                title: '隐藏边框',
                type: 'boolean',
                default: false,
              },
              placeholder: {
                title: 'placeholder',
                type: 'string',
                default: '',
              },

              autosize: {
                type: 'object',
                title: '自适应内容高度',
                properties: {
                  minRows: {
                    type: 'number',
                    title: '最小行',
                    default: 1,
                  },
                  maxRows: {
                    type: 'number',
                    title: '最大行',
                    default: 3,
                  },
                },
              },
            },
            ui: {
              type: 'card',
            } as SFObjectWidgetSchema,
          },
        };
        this.GetTargetType(value, '');
        this.sf.refreshSchema();
        this.DSVisble = false;
        break;

      case '18':
        this.FormField.schema.properties = {};
        this.sf.refreshSchema();
        this.GetTargetType(value, '');
        this.DSVisble = false;
        break;

      case '19':
        this.FormField.schema.properties = {
          maximum: {
            type: 'number',
            title: '最大值',
          },
          multipleOf: {
            type: 'number',
            title: '允许半星',
            enum: [
              { label: '是', value: 0.5 },
              { label: '否', value: 1 },
            ],
          },
        };
        this.GetTargetType(value, '');
        this.sf.refreshSchema();
        this.DSVisble = false;
        break;
    }
  }
}
export class FormField {
  constructor(
    public Key: string,
    public FieldId: Number,
    public FieldName: string,
    public FieldValue: string,
    public FieldValueType: string,
    public FieldValueDataSource: string, //组件读取远程数据地址，
    public FieldUIElementSchema: string, //表单Schema，
    public FieldUIElement: string,
    public FieldUnit: string,
    public schema: SFSchema, //酌情移除
    public formdata: any, //Schema 数据
    //  public callback: any, //明确移除
    public FieldCode: string,
    public IsRequired: boolean,
  ) {}

  FieldNamenzValidateStatus: string;
  FieldNamenzValidatingTip: string;

  FieldCodenzValidateStatus: string;
  FieldCodenzValidatingTip: string;


  FieldValueTypenzValidateStatus: string;
  FieldValueTypenzValidatingTip: string;
}
