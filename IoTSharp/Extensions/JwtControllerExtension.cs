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
                Customer =@this.User.GetCustomerId()
            };
        }
        public static string GetStorageRoot(this ControllerBase @this)
        {
            return $"/{@this.User.GetTenantId()}/{@this.User.GetCustomerId()}/";
        }
        public static string GetStorageRoot(this ControllerBase @this,Produce produce)
        {
            return $"/{@this.User.GetTenantId()}/{@this.User.GetCustomerId()}/{produce.Id}";
        }
        public static string GetStorageRoot(this ControllerBase @this, Device device)
        {
            return $"/{@this.User.GetTenantId()}/{@this.User.GetCustomerId()}/{device.Id}";
        }
        public static string GetStorageRoot(this ControllerBase @this, Gateway device)
        {
            return $"/{@this.User.GetTenantId()}/{@this.User.GetCustomerId()}/{device.Id}";
        }
    }
}
