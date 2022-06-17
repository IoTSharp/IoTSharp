import { SFSchema } from "@delon/form";

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
  