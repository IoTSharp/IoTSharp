using IoTSharp.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IoTSharp.Data
{
    /// <summary>
    /// 平台审计日志，记录用户或运行时对关键对象执行的可追溯动作。
    /// </summary>
    public class AuditLog
    {
        /// <summary>
        /// 审计记录 ID。
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// 租户 ID，用于审计查询隔离；历史迁移中已经存在同名列。
        /// </summary>
        public Guid? TenantId { get; set; }

        /// <summary>
        /// 客户 ID，用于审计查询隔离；历史迁移中已经存在同名列。
        /// </summary>
        public Guid? CustomerId { get; set; }

        /// <summary>
        /// 租户导航属性。
        /// </summary>
        public Tenant Tenant { get; set; }

        /// <summary>
        /// 客户导航属性。
        /// </summary>
        public Customer Customer { get; set; }

        /// <summary>
        /// 操作者用户 ID；运行时动作可写入运行时身份。
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// 操作者显示名；运行时动作可写入运行时来源。
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// 被操作对象 ID。
        /// </summary>
        public Guid ObjectID { get; set; }

        /// <summary>
        /// 被操作对象名称。
        /// </summary>
        public string ObjectName { get; set; }

        /// <summary>
        /// 被操作对象类型。
        /// </summary>
        public ObjectType ObjectType { get; set; } = ObjectType.Unknow;

        /// <summary>
        /// 动作名称。
        /// </summary>
        public string ActionName { get; set; }

        /// <summary>
        /// 动作输入或上下文 JSON。
        /// </summary>
        public string ActionData { get; set; }

        /// <summary>
        /// 动作结果摘要。
        /// </summary>
        public string ActionResult { get; set; }

        /// <summary>
        /// 动作发生时间。
        /// </summary>
        public DateTime ActiveDateTime { get; set; } = DateTime.UtcNow;

    }
}
