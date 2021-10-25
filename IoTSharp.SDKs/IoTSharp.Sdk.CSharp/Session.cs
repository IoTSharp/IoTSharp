using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoTSharp.Sdk.Http
{
    public enum UserRole
    {
        Anonymous,
        NormalUser,
        CustomerAdmin,
        TenantAdmin,
        SystemAdmin,
    }
    public class Session
    {

        private readonly LoginResult _result;
        public Session(LoginResult result)
        {
            _result = result;
        }
        public static implicit operator Session(LoginResult result  )
        {
            return new Session(result);
        }
        public static implicit operator Session(ApiResultOfLoginResult result)
        {
            return new Session(result.Data);
        }
        public static explicit operator LoginResult(Session result)
        {
            return (LoginResult) result;
        }
        public bool Anonymous => (_result?.Roles?.Contains(nameof(UserRole.Anonymous))).GetValueOrDefault();
        public bool CustomerAdmin => (_result?.Roles?.Contains(nameof(UserRole.CustomerAdmin))).GetValueOrDefault();
        public bool NormalUser => (_result?.Roles?.Contains(nameof(UserRole.NormalUser))).GetValueOrDefault();
        public bool SystemAdmin => (_result?.Roles?.Contains(nameof(UserRole.SystemAdmin))).GetValueOrDefault();
        public bool TenantAdmin => (_result?.Roles?.Contains(nameof(UserRole.TenantAdmin))).GetValueOrDefault();
        public TokenEntity Token => _result.Token;
        public bool IsLogin => (_result?.Succeeded).GetValueOrDefault();
        public bool CanLogout => IsLogin;

        public bool CanLogin => !IsLogin;

    }
}
