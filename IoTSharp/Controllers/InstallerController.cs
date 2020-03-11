using IoTSharp.Data;
using IoTSharp.Dtos;
using IoTSharp.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Silkier.AspNetCore;

namespace IoTSharp.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class InstallerController : ControllerBase
    {
        private ApplicationDbContext _context;
        private ILogger _logger;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ApplicationDBInitializer _dBInitializer;

        public InstallerController(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            IConfiguration configuration, ILogger<AccountController> logger, ApplicationDbContext context
           , ApplicationDBInitializer dBInitializer)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _logger = logger;
            _context = context;
            _dBInitializer = dBInitializer;
        }

        [AllowAnonymous]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesDefaultResponseType]
        public ActionResult<InstanceDto> Instance()
        {
            try
            {
                return base.Ok(GetInstanceDto());
            }
            catch (Exception ex)
            {
                return this.ExceptionRequest(ApiCode.Exception,ex.Message, ex);
            }
        }

        private InstanceDto GetInstanceDto()
        {
            return new InstanceDto() { Installed = _context.Relationship.Any(), Version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString() };
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<ActionResult<InstanceDto>> Install([FromBody] InstallDto model)
        {
            ActionResult<InstanceDto> actionResult = NoContent();
            try
            {
                if (!_context.Relationship.Any())
                {
                    await _dBInitializer.SeedRoleAsync();
                    await _dBInitializer.SeedUserAsync(model);
                    actionResult = Ok(GetInstanceDto());
                }
                else
                {
                    actionResult = BadRequest(new { code = ApiCode.AlreadyExists, msg = "Already installed", data = GetInstanceDto() });
                }
            }
            catch (Exception ex)
            {
                actionResult =this.ExceptionRequest(ApiCode.Exception, ex.Message, ex);
            }
            return actionResult;
        }
   
        public class InstanceDto
        {
            public string Version { get; internal set; }
            public bool Installed { get; internal set; }
        }

        public class InstallDto
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }

            [Required]
            public string CustomerName { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "PASSWORD_MIN_LENGTH", MinimumLength = 6)]
            public string Password { get; set; }

            public string TenantName { get; set; }

            [EmailAddress]
            public string TenantEMail { get; set; }

            [EmailAddress]
            public string CustomerEMail { get; set; }

            [Phone]
            public string PhoneNumber { get; set; }
        }
    }
}