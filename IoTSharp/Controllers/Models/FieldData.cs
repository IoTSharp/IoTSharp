using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace IoTSharp.Controllers.Models
{
    public class FormFieldData
    {
        public int Id { get; set; }

        public FieldData[] propdata { get; set; }

    }


    public class FieldData
    {
        public int FieldId { get; set; }
        public string FieldName { get; set; }
        public string FieldValue { get; set; }
        public int FieldValueType { get; set; }
        public int FieldMaxLength { get; set; }
        public string FieldValueDataSource { get; set; }
        public string FieldPattern { get; set; }
        public string FieldI18nKey { get; set; }
        public string FieldUIElementSchema { get; set; }
        public string FieldCode { get; set; }
        public int FieldUIElement { get; set; }
        public bool IsRequired { get; set; }
        public string FieldUnit { get; set; }
    }


    public class DynamicProp
    {

        public static object GetValue(int type, JToken target)
        {

            switch (type)
            {
                case 1:

                    {
                        int v;
                        if (int.TryParse(target.Value<string>(), out v))
                        {
                            return v;
                        }
                        return "";
                    }
                case 2:

                    {
                        double v;
                        if (double.TryParse(target.Value<string>(), out v))
                        {
                            return v;
                        }

                        return "";
                    }

                case 3:
                    {
                        decimal v;
                        if (decimal.TryParse(target.Value<string>(), out v))
                        {
                            return v;
                        }

                        return "";
                    }
                case 4:
                    return target.Value<string>();
                case 5:
                    {
                        DateTime v;
                        if (DateTime.TryParse(target.Value<string>(), out v))
                        {
                            return v;
                        }

                        return "";
                    }

                case 6:


                    if (target.HasValues)
                    {

                        return JsonConvert.SerializeObject(target.Value<JArray>().ToArray().Select(c => c.Value<int>()));
                    }
                    return "";


                case 7:

                    if (target.HasValues)
                    {
                        return JsonConvert.SerializeObject(target.Value<JArray>().ToArray().Select(c => c.Value<string>()));
                    }
                    break;
                case 8:
                    {
                        var vl = target.Value<string>();

                        if (!string.IsNullOrEmpty(vl))
                        {
                            if (Regex.IsMatch(vl, "^\\[(((\\d+(\\.\\d+)|\\d),)*(\\d+(\\.\\d+)|\\d))*\\]$"))
                            {
                                return vl;
                            }
                        }

                        return "";
                    }



                case 9:
                    return target.Value<float>();
                case 10:
                    {
                        var vl = target.Value<string>();

                        if (!string.IsNullOrEmpty(vl))
                        {
                            if (Regex.IsMatch(vl, "^\\[((\\d,)*\\d)*\\]$"))
                            {
                                return vl;
                            }

                        }

                        return "";
                    }
                   
              
                case 11:
                    if (target.HasValues)
                    {
                        return JsonConvert.SerializeObject(target.Value<JArray>().ToArray()
                            .Select(c => c.Value<DateTime>()));
                    }
                    break;
                case 12:


                    {
                        var vl = target.Value<string>();

                        if (!string.IsNullOrEmpty(vl))
                        {
                            if (Regex.IsMatch(vl, "^\\[(((\\d+(\\.\\d+)|\\d),)*(\\d+(\\.\\d+)|\\d))*\\]$"))
                            {
                                return vl;
                            }
                        }

                        return "";
                    }

           
                case 14:

                    var x = JsonConvert.SerializeObject(target.Value<JArray>());

                    return JsonConvert.SerializeObject(x);
                case 15:
                    {
                        DateTimeOffset v;

                        if (DateTimeOffset.TryParse(target.Value<string>(), out v))
                        {
                            return v;
                        }

                        return "";
                    }




                case 13:
                    break;
                    //  return target.Value<bool>();
            }




            return "";


        }
    }
}
