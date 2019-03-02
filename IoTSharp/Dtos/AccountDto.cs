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
    }

    public class LoginDto
    {
        [Required]
        public string Password { get; set; }

        [Required]
        public string Email { get; set; }
    }

    public class RegisterDto
    {
        [Required]
        public string Email { get; set; }

        [Required]
        public string CustomerName { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "PASSWORD_MIN_LENGTH", MinimumLength = 6)]
        public string Password { get; set; }
    }
}