using IoTSharp.Contracts;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataType = IoTSharp.Contracts.DataType;

#nullable enable

namespace IoTSharp.Data
{
    public class ProductDictionary
    {
        [Key]
        public Guid Id { get; set; }

        /// <summary>
        /// 字段名称
        /// </summary>
        public string KeyName { get; set; } = string.Empty;

        /// <summary>
        /// 字段显示名称
        /// </summary>
        public string DisplayName { get; set; } = string.Empty;



        /// <summary>
        /// 单位
        /// </summary>
        public string Unit { get; set; } = string.Empty;

        /// <summary>
        /// 单位转换表达式
        /// </summary>
        public string UnitExpression { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        public bool UnitConvert { get; set; }
        /// <summary>
        /// 字段备注
        /// </summary>
        public string? KeyDesc { get; set; }
        /// <summary>
        /// 默认值
        /// </summary>
        public string? DefaultValue { get; set; }

        /// <summary>
        /// 是否显示
        /// </summary>
        public bool Display { get; set; }

        /// <summary>
        /// 位置名称
        /// </summary>
        public string Place0 { get; set; } = string.Empty;
        /// <summary>
        /// 此位置顺序
        /// </summary>
        public string PlaceOrder0 { get; set; } = string.Empty;
        public string Place1 { get; set; } = string.Empty;
        public string PlaceOrder1 { get; set; } = string.Empty;
        public string Place2 { get; set; } = string.Empty;
        public string PlaceOrder2 { get; set; } = string.Empty;
        public string Place3 { get; set; } = string.Empty;
        public string PlaceOrder3 { get; set; } = string.Empty;
        public string Place4 { get; set; } = string.Empty;
        public string PlaceOrder4 { get; set; } = string.Empty;
        public string Place5 { get; set; } = string.Empty;
        public string PlaceOrder5 { get; set; } = string.Empty;
        /// <summary>
        /// 数据类型 
        /// </summary>
        public DataType DataType { get; set; }


        public string? Tag { get; set; }


        public Guid? Customer { get; set; }

        public bool Deleted { get; set; }


    }
}
