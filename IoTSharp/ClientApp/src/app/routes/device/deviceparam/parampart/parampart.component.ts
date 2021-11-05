import { Component, EventEmitter, OnInit, Output, Input } from '@angular/core';
import { ControlValueAccessor } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { SFSchema } from '@delon/form';
import { UIDataType } from 'src/app/routes/common/UIDataType';
//SFComponent is good ,but make things so complicated
@Component({
  selector: 'app-parampart',
  templateUrl: './parampart.component.html',
  styleUrls: ['./parampart.component.less'],
})
export class ParampartComponent implements ControlValueAccessor, OnInit {
  @Output() OnRemove = new EventEmitter<DeviceTypeParam>();
  @Input()
  DeviceTypeParam!: DeviceTypeParam;
  @Input()
  AllSuportType: UIDataType[] = [];
  @Input()
  AllControlType: any = [];

  SuportType: any = [];

  DSVisble: boolean = false;
  DefaultVisble: boolean = false;
  constructor(private _router: Router, private router: ActivatedRoute) {}
  writeValue(obj: any): void {
    this.DeviceTypeParam = obj;
  }

  registerOnChange(fn: any): void {}
  registerOnTouched(fn: any): void {}
  dtcChange($event: any) {}

  dtpChange($event: any) {}
  remove($event: any) {}
  DeviceTypeParamUITypeChange($event: any) {}

  submit() {}
  ngOnInit(): void {}
}

export class DeviceTypeParam {
  constructor(
    public Key: string,
    public TemplateParamId: Number,
    public TemplateParamName: string,
    public TemplateParamValue: string,
    public TemplateParamValueType: string,
    public TemplateParamValueDataSource: string, //组件读取远程数据地址，
    public TemplateParamUIElementSchema: string, //表单Schema，
    public TemplateParamUIElement: string,
    public schema: SFSchema,
    public formdata: any,

    public TemplateParamValueCode: string,
    public IsRequired: boolean,
  ) {}

  public TemplateParamNamenzValidateStatus: string = '';
  public TemplateParamNamenzValidatingTip: string = '';

  public TemplateParamValueCodenzValidateStatus: string = '';
  public TemplateParamValueCodenzValidatingTip: string = '';
}
