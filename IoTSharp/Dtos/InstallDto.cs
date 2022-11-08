using System.ComponentModel.DataAnnotations;

namespace IoTSharp.Dtos
{

    public class InstallDto
    {
        /// <summary>
        /// 系统管理员用户名
        /// </summary>
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        /// <summary>
        /// 客户名称
        /// </summary>
        [Required]
        public string CustomerName { get; set; }
        /// <summary>
        /// 系统管理员密码
        /// </summary>
        [Required]
        [StringLength(100, ErrorMessage = "PASSWORD_MIN_LENGTH", MinimumLength = 6)]
        public string Password { get; set; }
        /// <summary>
        /// 租户姓名
        /// </summary>
        public string TenantName { get; set; }
        /// <summary>
        /// 租户邮箱
        /// </summary>
        [EmailAddress]
        public string TenantEMail { get; set; }
        /// <summary>
        /// 客户邮箱
        /// </summary>
        [EmailAddress]
        public string CustomerEMail { get; set; }
        /// <summary>
        /// 电话号码
        /// </summary>
        [Phone]
        public string PhoneNumber { get; set; }
    }
}