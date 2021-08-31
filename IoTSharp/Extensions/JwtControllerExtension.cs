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
        public static async Task<UserProfile> GetUserProfile(this ControllerBase c)
        {
            string token = await c.HttpContext.GetTokenAsync(JwtBearerDefaults.AuthenticationScheme, "access_token");
            if (!string.IsNullOrEmpty(token))
            {
                var _token = token.Split(new[] { "." }, StringSplitOptions.RemoveEmptyEntries);
                if (_token.Length == 3)
                {
                    var payload = _token[1];
                    var claims = JwtHeader.Base64UrlDeserialize(payload);
                    return new UserProfile()
                    {
                        Id = Guid.Parse(claims[ClaimTypes.NameIdentifier].ToString() ?? string.Empty),
                        Name = claims[ClaimTypes.Name].ToString() ?? string.Empty,
                        Roles = claims[ClaimTypes.Role]?.ToString().Split(';') ?? new string[] { },
                        Email = claims[ClaimTypes.Email]?.ToString().Split(';') ?? new string[] { },
                        Tenant = Guid.Parse(claims[IoTSharpClaimTypes.Tenant]?.ToString() ?? string.Empty),
                        Comstomer = Guid.Parse(claims[IoTSharpClaimTypes.Customer]?.ToString() ?? string.Empty),
                    };
                }
            }
            return default;
        }
    }
}
