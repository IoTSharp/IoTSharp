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

        public static  string JWTKEY="";
        public static async Task<UserProfile> GetUserProfile(this ControllerBase c)
        {
            string token = await c.HttpContext.GetTokenAsync(JwtBearerDefaults.AuthenticationScheme, "access_token");
            if (!string.IsNullOrEmpty(token))
            {

                var _token = token.Split(new[] { "." }, StringSplitOptions.RemoveEmptyEntries);
                if (_token.Length == 3)
                {
                    //var header = _token[0];
                    var payload = _token[1];
                //    var sign = _token[2];
                ////    var YOURJWTKEY = "iotsharpiotsharpiotsharpiotsharpiotsharp"; 
                //    var hs256 = new HMACSHA256(Encoding.UTF8.GetBytes(JWTKEY));
                //    var target = Base64UrlEncoder.Encode(
                //        hs256.ComputeHash(Encoding.UTF8.GetBytes(string.Concat(header, ".", payload))));
                //    //验签
                //    if (string.Equals(sign, target))
                //    {
                        var claims = JwtHeader.Base64UrlDeserialize(payload);
                        return new UserProfile()
                        {
                            Id = Guid.Parse(claims[ClaimTypes.NameIdentifier].ToString() ?? string.Empty),
                            Name = claims[ClaimTypes.Name].ToString() ?? string.Empty,
                            Roles = JsonConvert.DeserializeObject<string[]>(claims[ClaimTypes.Role]?.ToString() ?? string.Empty),
                            Email = JsonConvert.DeserializeObject<string[]>(claims[ClaimTypes.Email]?.ToString() ?? string.Empty),
                            Tenant = Guid.Parse(claims[IoTSharpClaimTypes.Tenant]?.ToString() ?? string.Empty),
                            Comstomer = Guid.Parse(claims[IoTSharpClaimTypes.Customer]?.ToString() ?? string.Empty),
                        };

                    //}


                }
            }

            return default;
        }
    }
}
