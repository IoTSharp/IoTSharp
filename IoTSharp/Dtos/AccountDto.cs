using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace IoTSharp.Dtos
{
    public class TokenEntity
    {
        public string access_token { get; set; }
        public int expires_in { get; set; }
    }

    public class LoginResult
    {
        public string UserName { get; set; }
        public Microsoft.AspNetCore.Identity.SignInResult SignIn { get; set; }
        public bool Succeeded { get; set; }
        public TokenEntity Token { get; set; }
        public IList<string> Roles { get; set; }
    }

    public class LoginDto
    {
        [Required]
        public string Password { get; set; }

        [Required]
        public string UserName { get; set; }
    }

    public class RegisterDto
    {
        [Required]
        public string Email { get; set; }
        [Required]
        public string PhoneNumber { get; set; }
        [Required]
        public Guid CustomerId { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "PASSWORD_MIN_LENGTH", MinimumLength = 6)]
        public string Password { get; set; }
    }

    public class UserItemDto
    {
        public string Email { get; set; }
        public List<string> Roles { get; set; } = new List<string>();
        public string PhoneNumber { get;  set; }
        public int AccessFailedCount { get;  set; }
        public string Id { get;  set; }
    }
}