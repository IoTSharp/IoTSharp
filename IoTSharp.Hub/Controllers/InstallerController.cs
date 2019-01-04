using IoTSharp.Hub.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace IoTSharp.Hub.Controllers
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

        public InstallerController(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            IConfiguration configuration, ILogger<AccountController> logger, ApplicationDbContext context
            )
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _logger = logger;
            _context = context;
        }

        [AllowAnonymous]
        [HttpPost]
        public IActionResult CheckInstallation()
        {
            int code = -1;
            string msg;
            bool installed = false;
            try
            {
                installed = !(_context.Tenant.Count() == 0 && _context.Customer.Count() == 0);
                msg = installed ? "Already installed" : "Not Install";
                code = 0;
            }
            catch (Exception ex)
            {
                msg = ex.Message;
            }
            return Ok(new
            {
                code,
                msg,
                data = new { installed }
            });
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Install([FromBody] InstallDto model)
        {
            IActionResult actionResult = NoContent();
            var tran = _context.Database.BeginTransaction();
            try
            {
                if (_context.Tenant.Count() == 0 && _context.Customer.Count() == 0)
                {
                    var tenant = new Tenant() { Id = Guid.NewGuid(), Name = model.TenantName, EMail = model.TenantEMail };
                    var customer = new Customer() { Id = Guid.NewGuid(), Name = model.CustomerName, Email = model.CustomerEMail };
                    customer.Tenant = tenant;
                    tenant.Customers = new List<Customer>();
                    tenant.Customers.Add(customer);
                    var user = new IdentityUser
                    {
                        Email = model.Email,
                        UserName = model.Email,
                        PhoneNumber = model.PhoneNumber
                    };

                    var result = await _userManager.CreateAsync(user, model.Password);

                    if (result.Succeeded)
                    {
                        _context.Tenant.Add(tenant);
                        _context.Customer.Add(customer);
                        await _signInManager.SignInAsync(user, false);
                        await _signInManager.UserManager.AddClaimAsync(user, new Claim(ClaimTypes.Email, model.Email));
                        await _signInManager.UserManager.AddClaimAsync(user, new Claim(ClaimTypes.GroupSid, customer.Id.ToString()));
                        var rship = new Relationship();
                        rship.IdentityUser = _context.Users.Find(user.Id);
                        rship.Customer = customer;
                        rship.Tenant = tenant;
                        _context.Add(rship);
                        int savechangesresult = _context.SaveChanges();
                        tran.Commit();
                        actionResult = Ok(new { code = 0, msg = "OK", data = new { result = savechangesresult >= 0, count = savechangesresult } });
                    }
                    else
                    {
                        tran.Rollback();
                        var msg = from e in result.Errors select $"{e.Code}:{e.Description}\r\n";
                        actionResult = BadRequest(new { code = -3, msg = string.Join(';', msg.ToArray()) });
                    }
                }
                else
                {
                    tran.Rollback();
                    actionResult = Ok(new { code = 1, msg = "Already installed" });
                }
            }
            catch (Exception ex)
            {
                tran.Rollback();
                actionResult = BadRequest(new { code = -2, msg = ex.Message, data = ex });
                _logger.LogError(ex, ex.Message);
            }
            return actionResult;
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