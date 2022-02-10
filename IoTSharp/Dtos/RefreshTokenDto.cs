using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace IoTSharp.Dtos
{
    public class RefreshTokenDto
    {

        [Required]
        public string Token { get; set; }

        [Required]
        public string RefreshToken { get; set; }




    }
}
