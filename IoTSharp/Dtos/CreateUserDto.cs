using System;
using System.ComponentModel.DataAnnotations;

namespace IoTSharp.Dtos
{
    public class CreateUserDto
    {
        /// <summary>
        /// 用户邮箱
        /// </summary>
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        /// <summary>
        /// 系统管理员密码
        /// </summary>
        [Required]
        [StringLength(100, ErrorMessage = "PASSWORD_MIN_LENGTH", MinimumLength = 6)]
        public string Password { get; set; }

        /// <summary>
        /// 电话号码
        /// </summary>
        [Phone]
        public string PhoneNumber { get; set; }
        /// <summary>
        /// 客户的ID
        /// </summary>
        public Guid Customer { get; set; }

    }
}