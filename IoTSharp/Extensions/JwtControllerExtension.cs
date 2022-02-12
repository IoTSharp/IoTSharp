using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using IoTSharp.Data;
using IoTSharp.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;


namespace IoTSharp.Extensions
{
    public static class JwtControllerExtension
    {
        public static UserProfile GetUserProfile(this ControllerBase @this)
        {
            return new UserProfile()
            {
                Id = @this.User.GetUserId(),
                Name = @this.User.Identity.Name,
                Roles = @this.User.Claims.Where(c => c.Type == ClaimTypes.Role).Select(x => x.Value).ToArray(),
                Email = @this.User.GetEmail(),
                Tenant = @this.User.GetTenantId(),
                Comstomer =@this.User.GetCustomerId()
            };
        }
    }
}
