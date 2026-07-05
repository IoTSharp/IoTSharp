using System;

namespace IoTSharp.Dtos
{
    /// <summary>
    /// 产品命令定义，用于描述同一 Product 下设备实例可执行的命令模板。
    /// </summary>
    public class ProductCommandDto
    {
        /// <summary>
        /// 命令定义ID。
        /// </summary>
        public Guid CommandId { get; set; }

        /// <summary>
        /// 所属产品ID。
        /// </summary>
        public Guid ProductId { get; set; }

        /// <summary>
        /// 命令显示标题。
        /// </summary>
        public string CommandTitle { get; set; }

        /// <summary>
        /// 命令类型，用于兼容现有 RPC 和命令执行分类。
        /// </summary>
        public int CommandType { get; set; }

        /// <summary>
        /// 命令参数定义。
        /// </summary>
        public string CommandParams { get; set; }

        /// <summary>
        /// 命令名称，通常对应设备侧 RPC 方法名。
        /// </summary>
        public string CommandName { get; set; }

        /// <summary>
        /// 命令负载模板。
        /// </summary>
        public string CommandTemplate { get; set; }
    }
}
