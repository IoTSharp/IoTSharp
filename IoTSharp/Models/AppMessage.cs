using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IoTSharp.Models
{
    public class AppMessage<T>
    {
        public ErrType ErrType { get; set; }
        public string ErrMessage { get; set; }
        public T Result { get; set; }
        public string ErrLevel { get; set; }
        public bool IsVisble { get; set; }
    }
    public class AppMessage
    {
        public ErrType ErrType { get; set; }
        public string ErrMessage { get; set; }
        public dynamic Result { get; set; }
        public string ErrLevel { get; set; }
        public bool IsVisble { get; set; }
    }
    public enum ErrType
    {
        正常返回 = 1,
        找不到对象 = 2,
        会话过期 = 200,
        参数错误 = 3,
        权限不足 = 10,
        网络异常 = 8,
        程序异常 = 9
    }

    public struct ErrLevel
    {
        public static readonly string Error = "error";
        public static readonly string Success = "success";
        public static readonly string Info = "info";
        public static readonly string Warning = "warning";

    }
}
