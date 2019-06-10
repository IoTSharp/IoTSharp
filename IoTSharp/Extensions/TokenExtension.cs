using IoTSharp.Data;
using IoTSharp.Dtos;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace IoTSharp
{
    public static class TokenExtension
    {
        private static byte[] symmetricKeyBytes;
        private static SymmetricSecurityKey symmetricKey;
        private static SigningCredentials signingCredentials;
        private static TokenValidationParameters tokenValidationParams;
        private static TimeSpan _expire;
        private static string _issuer;
        private static string _audience;

        //Construct our JWT authentication paramaters then inject the parameters into the current TokenBuilder instance
        // If injecting an RSA key for signing use this method
        // Be weary of common jwt trips: https://trustfoundry.net/jwt-hacking-101/ and https://www.sjoerdlangkemper.nl/2016/09/28/attacking-jwt-authentication/
        //public static void ConfigureJwtAuthentication(this IServiceCollection services, RSAParameters rsaParams)
        public static void ConfigureJwtAuthentication(this IServiceCollection services, string issuer, string audience, string key, TimeSpan expire)
        {
            //   JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear(); // => remove default claims
            symmetricKeyBytes = Encoding.ASCII.GetBytes(key);
            symmetricKey = new SymmetricSecurityKey(symmetricKeyBytes);
            signingCredentials = new SigningCredentials(symmetricKey, SecurityAlgorithms.HmacSha256);
            _expire = expire;
            if (_expire.TotalMinutes < 60) expire = TimeSpan.FromMinutes(60);
            _issuer = issuer ?? "iotsharp.net";
            _audience = audience ?? _issuer;

            tokenValidationParams = new TokenValidationParameters()
            {
                ValidateIssuerSigningKey = true,
                ValidIssuer = issuer,
                ValidAudience = audience,
                ValidateLifetime = true,
                ValidateAudience = true,
                RequireSignedTokens = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                ClockSkew = TimeSpan.Zero
            };
            services.AddAuthentication(options =>
            {
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultForbidScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = tokenValidationParams;
#if PROD || UAT
                options.IncludeErrorDetails = false;
#elif DEBUG
                options.RequireHttpsMetadata = false;
#endif
            });
        }

        public static async Task<TokenEntity> GenerateJwtTokenAsync(this UserManager<IdentityUser> manager, IdentityUser user)
        {
            var roles = await manager.GetRolesAsync(user);
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id)
            };
            var lstclaims = await manager.GetClaimsAsync(user);
            if (roles != null)
            {
                foreach (var role in roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                }
            }
            claims.AddRange(lstclaims.Where(c => c.Type == IoTSharpClaimTypes.Tenant || c.Type == IoTSharpClaimTypes.Customer).ToList());
            var head = new JwtHeader();
            var jwt = new JwtSecurityToken(tokenValidationParams.ValidIssuer, tokenValidationParams.ValidAudience, claims, DateTime.UtcNow, DateTime.Now.AddMinutes(_expire.TotalMinutes), signingCredentials);
            jwt.Payload.AddClaims(claims.ToArray());
            var response = new TokenEntity
            {
                access_token = new JwtSecurityTokenHandler().WriteToken(jwt),
                expires_in = (int)_expire.TotalSeconds
            };
            return response;
        }

        public static JwtSecurityToken GetJwtSecurityToken(this ControllerBase controller)
        {
            return controller.Request.GetJwtSecurityToken();
        }

        public static Claim GetClaim(this JwtSecurityToken jwt, string key)
        {
            return jwt.Payload.Claims.FirstOrDefault(c => c.Type == key);
        }

        public static IEnumerable<Claim> GetClaims(this JwtSecurityToken jwt)
        {
            return jwt.Payload.Claims;
        }

        public static JwtSecurityToken GetJwtSecurityToken(this HttpRequest httpRequest)
        {
            JwtSecurityToken result = null;
            var authenticationHeaders = httpRequest.Headers["Authorization"].ToArray();
            if ((authenticationHeaders == null) || (authenticationHeaders.Length != 1))
            {
                result = null;
            }
            else
            {
                var jwToken = authenticationHeaders[0].Split(' ')[1];
                var jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
                ClaimsPrincipal principal = null;
                SecurityToken securityToken = null;
                try
                {
                    principal = jwtSecurityTokenHandler.ValidateToken(jwToken, tokenValidationParams, out securityToken);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                if ((principal != null) && (principal.Claims != null))
                {
                    result = securityToken as JwtSecurityToken;
                    Trace.WriteLine(result.Audiences.First());
                    Trace.WriteLine(result.Issuer);
                }
            }
            return result;
        }

        public static string GetUserId(this JwtSecurityToken jwtSecurityToken)
        {
            return jwtSecurityToken.Payload["userid"] as string;
        }

        public static string GetPayloadValue(this JwtSecurityToken jwtSecurityToken, string Key)
        {
            return jwtSecurityToken.Payload[Key] as string;
        }


        /// Hashes an email with MD5.  Suitable for use with Gravatar profile
        /// image urls
        public static string Gravatar( this IdentityUser user)
        {
            string email = user.Email;
            // Create a new instance of the MD5CryptoServiceProvider object.  
            MD5 md5Hasher = MD5.Create();

            // Convert the input string to a byte array and compute the hash.  
            byte[] data = md5Hasher.ComputeHash(Encoding.Default.GetBytes(email));

            // Create a new Stringbuilder to collect the bytes  
            // and create a string.  
            StringBuilder sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data  
            // and format each one as a hexadecimal string.  
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }
            return  string.Format("http://www.gravatar.com/avatar/{0}", sBuilder.ToString());  ;  // Return the hexadecimal string. 
        }
    }
}