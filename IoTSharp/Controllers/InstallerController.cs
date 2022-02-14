using IoTSharp.Data;
using IoTSharp.Dtos;
using IoTSharp.X509Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace IoTSharp.Controllers
{
    /// <summary>
    /// 安装
    /// </summary>
    [ApiController]
    [Route("api/[controller]/[action]")]
    public partial class InstallerController : ControllerBase
    {
        private ApplicationDbContext _context;
        private ILogger _logger;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ApplicationDBInitializer _dBInitializer;
        private readonly AppSettings _setting;

        public InstallerController(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            IConfiguration configuration, ILogger<AccountController> logger, ApplicationDbContext context
           , ApplicationDBInitializer dBInitializer,IOptions<AppSettings> options)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _logger = logger;
            _context = context;
            _dBInitializer = dBInitializer;
            _setting = options.Value;
        }

        /// <summary>
        /// 检查IoTSharp实例信息
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesDefaultResponseType]
        public ApiResult<InstanceDto> Instance()
        {
            try
            {
                return new ApiResult<InstanceDto>(ApiCode.Success, "Ok", GetInstanceDto());
            }
            catch (Exception ex)
            {
                return new ApiResult<InstanceDto>(ApiCode.Exception, ex.Message, null);
            }
        }

        private InstanceDto GetInstanceDto()
        {

            return new InstanceDto() { Installed = _context.Relationship.Any(), Domain = this.Request.Host.Value, Version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString(), CACertificate= _setting.MqttBroker.CACertificate != null };
        }

        /// <summary>
        /// 域名可以不配置， 默认会使用机器名  
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = nameof(UserRole.SystemAdmin))]
        [HttpPost]
        public ApiResult CreateRootCertificate(CreateRootCertificateDto m)
        {
            ApiResult result = new ApiResult(ApiCode.Success, "OK");
            if (_setting.MqttBroker.CACertificate != null)
            {
                result = new ApiResult(ApiCode.AlreadyExists, "CACertificate already exists.");
            }
            else if (string.IsNullOrEmpty(m.Domain))
            {
                result = new ApiResult(ApiCode.AlreadyExists, "ServerIPAddress     is required.");
            }
            else
            {
                var ip = IPAddress.Parse(m.Domain);
                var ten = _context.GetTenant(User.GetTenantId());
                var option = _setting.MqttBroker;
                var ca = ip.CreateCA(option.CACertificateFile, option.CAPrivateKeyFile);
                ca.CreateBrokerTlsCert(_setting.MqttBroker.DomainName?? Dns.GetHostName(), ip, option.CertificateFile, option.PrivateKeyFile, ten.EMail);
                ca.LoadCAToRoot();
                result = new ApiResult(ApiCode.Success, ca.Thumbprint);

            }
            return result;
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<ApiResult<InstanceDto>> Install([FromBody] InstallDto model)
        {
            try
            {
                if (!_context.Relationship.Any())
                {
                    await _dBInitializer.SeedRoleAsync();
                    await _dBInitializer.SeedUserAsync(model);
                    await _dBInitializer.SeedDictionary();
                    //     await _dBInitializer.SeedI18N();
                    //     actionResult = Ok(GetInstanceDto());

                    return new ApiResult<InstanceDto>(ApiCode.Success, "Ok", GetInstanceDto());
                }
                else
                {
                    return new ApiResult<InstanceDto>(ApiCode.AlreadyExists, "Already installed", GetInstanceDto());
                }
            }
            catch (Exception ex)
            {
                return new ApiResult<InstanceDto>(ApiCode.Exception, ex.Message, null);
            }
        }
    }
}