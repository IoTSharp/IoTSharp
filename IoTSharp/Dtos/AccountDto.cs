using IoTSharp.Contracts;
using IoTSharp.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace IoTSharp.Dtos
{
    public class TokenEntity
    {
        /// <summary>
        /// token
        /// </summary>
        public string access_token { get; set; }
        /// <summary>
        /// 过期时间
        /// </summary>
        public long expires_in { get; set; }


        public string refresh_token { get; set; }
        public DateTime expires { get; set; }
    }

    public class LoginResult
    {
        /// <summary>
        /// 登录结果
        /// </summary>
        public ApiCode Code { get; set; }
        /// <summary>
        /// 用户名
        /// </summary>
        public string UserName { get; set; }
        /// <summary>
        /// 登录结果
        /// </summary>
        public Microsoft.AspNetCore.Identity.SignInResult SignIn { get; set; }
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool Succeeded { get; set; }
        /// <summary>
        /// Token
        /// </summary>
        public TokenEntity Token { get; set; }
        /// <summary>
        /// 用户所具备权限
        /// </summary>
        public IList<string> Roles { get; set; }
        public string Avatar { get; internal set; }
    }

    public class LoginDto
    {
        /// <summary>
        /// 密码
        /// </summary>
        [Required]
        public string Password { get; set; }
        /// <summary>
        /// 用户名
        /// </summary>
        [Required]
        public string UserName { get; set; }

    }

    public class RegisterDto
    {
        /// <summary>
        /// 邮箱地址， 也是用户名，一个邮箱只能注册平台的一个客户，如果你在平台有两个租户都有账号，则需要两个邮箱地址。 
        /// </summary>
        [Required]
        public string Email { get; set; }
        /// <summary>
        /// 电话号码
        /// </summary>
        [Required]
        public string PhoneNumber { get; set; }
        /// <summary>
        /// 用户隶属客户邮箱地址
        /// </summary>
        [Required]
        public string Customer { get; set; }
        /// <summary>
        /// 用户名密码
        /// </summary>
        [Required]
        [StringLength(100,  MinimumLength = 6)]
        public string Password { get; set; }
    }

    public class UserItemDto
    {
        /// <summary>
        /// 用户邮箱地址
        /// </summary>
        public string Email { get; set; }
        /// <summary>
        /// 权限标识
        /// </summary>
        public List<string> Roles { get; set; } = new List<string>();
        /// <summary>
        /// 电话号码
        /// </summary>
        public string PhoneNumber { get; set; }
        /// <summary>
        /// 登录失败次数
        /// </summary>
        public int AccessFailedCount { get; set; }
        /// <summary>
        /// 标识
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// 是否被锁定
        /// </summary>
        public bool LockoutEnabled { get; set; }
        /// <summary>
        /// 锁定时长
        /// </summary>
        public DateTimeOffset? LockoutEnd { get; set; }
        /// <summary>
        /// 用户名
        /// </summary>
        public string UserName { get;  set; }
        public string CustomerName { get; set; }
        public string TenantName { get; set; }
    }

    public class UserPassword
    {
        /// <summary>
        /// 旧密码
        /// </summary>
        public string Pass { get; set; }
        /// <summary>
        /// 新密码
        /// </summary>
        public string PassNew { get; set; }
        /// <summary>
        /// 第二次验证密码
        /// </summary>
        public string PassNewSecond { get; set; }
    }


}