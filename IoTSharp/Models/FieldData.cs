using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using IoTSharp.Extensions;

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

        public static object GetValue(int type, JsonNode target)
        {

            switch (type)
            {
                case 1:

                    {
                        int v;
                        if (int.TryParse(target.GetStringValue(), out v))
                        {
                            return v;
                        }
                        return "";
                    }
                case 2:

                    {
                        double v;
                        if (double.TryParse(target.GetStringValue(), out v))
                        {
                            return v;
                        }

                        return "";
                    }

                case 3:
                    {
                        decimal v;
                        if (decimal.TryParse(target.GetStringValue(), out v))
                        {
                            return v;
                        }

                        return "";
                    }
                case 4:
                    return target.GetStringValue();
                case 5:
                    {
                        DateTime v;
                        if (DateTime.TryParse(target.GetStringValue(), out v))
                        {
                            return v;
                        }

                        return "";
                    }

                case 6:


                    if (target is JsonArray intArray && intArray.Count > 0)
                    {
                        return JsonObjectSerializer.Serialize(intArray.Select(c => c.ToObject<int>()));
                    }
                    return "";


                case 7:

                    if (target is JsonArray stringArray && stringArray.Count > 0)
                    {
                        return JsonObjectSerializer.Serialize(stringArray.Select(c => c.ToObject<string>()));
                    }
                    break;
                case 8:
                    {
                        var vl = target.GetStringValue();

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
                    return target.ToObject<float>();
                case 10:
                    {
                        var vl = target.GetStringValue();

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
                    if (target is JsonArray dateTimeArray && dateTimeArray.Count > 0)
                    {
                        return JsonObjectSerializer.Serialize(dateTimeArray.Select(c => c.ToObject<DateTime>()));
                    }
                    break;
                case 12:


                    {
                        var vl = target.GetStringValue();

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

                    var x = JsonObjectSerializer.Serialize(target as JsonArray ?? new JsonArray());

                    return JsonObjectSerializer.Serialize(x);
                case 15:
                    {
                        DateTimeOffset v;

                        if (DateTimeOffset.TryParse(target.GetStringValue(), out v))
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
