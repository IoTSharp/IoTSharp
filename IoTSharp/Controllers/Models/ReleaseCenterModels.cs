using IoTSharp.Contracts;

namespace IoTSharp.Controllers.Models
{
    /// <summary>
    /// Release Center 发布计划查询条件。
    /// </summary>
    public class ReleasePlanQueryDto : QueryDto
    {
        /// <summary>
        /// 发布计划类型。
        /// </summary>
        public ReleasePlanType? PlanType { get; set; }

        /// <summary>
        /// 发布计划状态。
        /// </summary>
        public ReleasePlanStatus? Status { get; set; }
    }
}
