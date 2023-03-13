using static CLanguage.Report;
using System.ComponentModel.DataAnnotations;
using System;

namespace IoTSharp.Dtos
{
    public class CreateUserInput
    {
        /// <summary>
        /// 邮箱地址， 也是用户名，一个邮箱只能注册平台的一个客户，如果你在平台有两个租户都有账号，则需要两个邮箱地址。 
        /// </summary>
        [Required]
        public string Email { get; set; }
        /// <summary>
        /// 电话号码
        /// </summary>
        public string PhoneNumber { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Guid CustomerId { get; set; }

        /// <summary>
        /// 用户名密码
        /// </summary>
        [Required]
        [StringLength(100, MinimumLength = 6)]
        public string Password { get; set; } = "1q2w3E*";
    }
}
