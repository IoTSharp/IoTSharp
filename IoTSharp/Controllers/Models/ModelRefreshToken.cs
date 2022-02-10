using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace IoTSharp.Controllers.Models
{
    public class ModelRefreshToken
    {

        public string Token { get; set; }

        public string RefreshToken { get; set; }

        public   long ExpiresIn { get; set; }

        public IdentityUser AppUser { get; set; }
        public IList<string> Roles { get; set; }
        public DateTime Expires { get; set; }
    }
}
