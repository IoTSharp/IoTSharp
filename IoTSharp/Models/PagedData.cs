using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IoTSharp.Models
{
    /// <summary>
    /// 返回一页类型为<typeparamref name="T"/>的数据
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PagedData<T>
    {
        /// <summary>
        /// 数据总量
        /// </summary>
        public long total { get; set; }
        /// <summary>
        /// 一页数据
        /// </summary>
        public List<T> rows { get; set; }

    }
}
