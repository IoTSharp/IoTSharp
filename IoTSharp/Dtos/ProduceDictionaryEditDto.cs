using System;
using IoTSharp.Contracts;

namespace IoTSharp.Dtos
{
   



    public class ProduceDictionaryEditDto
    {
        public Guid produceId { get; set; }
        public ProduceDictionaryItemDto[] ProduceDictionaryData { get; set; }
    }



    public class ProduceDictionaryItemDto
    {

        public Guid Id { get; set; }

        /// <summary>
        /// 字段名称
        /// </summary>
        public string KeyName { get; set; }

        /// <summary>
        /// 字段显示名称
        /// </summary>
        public string DisplayName { get; set; }



        /// <summary>
        /// 单位
        /// </summary>
        public string Unit { get; set; }

        /// <summary>
        /// 单位转换表达式
        /// </summary>
        public string UnitExpression { get; set; }

        /// <summary>
        /// 单位转换
        /// </summary>
        public bool UnitConvert { get; set; }
        /// <summary>
        /// 字段备注
        /// </summary>
        public string KeyDesc { get; set; }
        /// <summary>
        /// 默认值
        /// </summary>
        public string DefaultValue { get; set; }

        /// <summary>
        /// 是否显示
        /// </summary>
        public bool Display { get; set; }

        /// <summary>
        /// 位置名称
        /// </summary>
        public string Place0 { get; set; }
        /// <summary>
        /// 此位置顺序
        /// </summary>
        public string PlaceOrder0 { get; set; }
        public string Place1 { get; set; }
        public string PlaceOrder1 { get; set; }
        public string Place2 { get; set; }
        public string PlaceOrder2 { get; set; }
        public string Place3 { get; set; }
        public string PlaceOrder3 { get; set; }
        public string Place4 { get; set; }
        public string PlaceOrder4 { get; set; }
        public string Place5 { get; set; }
        public string PlaceOrder5 { get; set; }
        /// <summary>
        /// 数据类型 
        /// </summary>
        public DataType DataType { get; set; }


        public string Tag { get; set; }

    }

}
