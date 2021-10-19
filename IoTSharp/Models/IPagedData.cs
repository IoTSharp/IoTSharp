using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IoTSharp.Models
{
    public class PagedData<T>
    {
        public long total { get; set; }
        public List<T> rows { get; set; }
        public  bool HasNextPageData { get; set; }

    }
}
