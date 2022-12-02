using System;
using IoTSharp.Contracts;

namespace IoTSharp.Dtos
{
    public class LockDto
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        public Guid Id { get; set; }
        /// <summary>
        /// 用户锁定操作
        /// </summary>
        public LockOpt Opt { get; set; }
    }
}