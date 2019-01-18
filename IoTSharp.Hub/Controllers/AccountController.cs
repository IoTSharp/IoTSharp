using IoTSharp.Hub.Data;
using IoTSharp.Hub.Dtos;
using IoTSharp.Hub.Extensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace IoTSharp.Hub.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class AccountController : ControllerBase
    {
        private ApplicationDbContext _context;
        private ILogger _logger;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly SignInManager<IdentityUser> _signInManager;

        public AccountController(
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
        public async Task<ActionResult<LoginResult>> Login([FromBody] LoginDto model)
        {
            try
            {
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, false, false);
                if (result.Succeeded)
                {
                    var appUser = _userManager.Users.SingleOrDefault(r => r.Email == model.Email);
                    var token = await _userManager.GenerateJwtTokenAsync(appUser);
                    return Ok(new LoginResult()
                    {
                        Succeeded = result.Succeeded,
                        Token = token,
                        UserName = appUser.UserName,
                        SignIn = result
                    });
                }
                else
                {
                    return Unauthorized(new { code = ApiCode.LoginError, msg = "Unauthorized", data = result });
                }
            }
            catch (Exception ex)
            {
                return this.ExceptionRequest(ex);
            }
        }

        /// <summary>
        /// Register a user
        /// </summary>
        /// <param name="model"></param>
        /// <returns ></returns>
        [AllowAnonymous]
        [HttpPost]
        public async Task<ActionResult<LoginResult>> Register([FromBody] RegisterDto model)
        {
            ActionResult<LoginResult> actionResult = NoContent();
            try
            {
                var user = new IdentityUser
                {
                    Email = model.Email
                };
                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    await _signInManager.SignInAsync(user, false);
                    await _signInManager.UserManager.AddClaimAsync(user, new Claim(ClaimTypes.Email, model.Email));
                    var customer = _context.Customer.FirstOrDefault(c => c.Name == model.CustomerName);
                    if (customer != null)
                    {
                        await _signInManager.UserManager.AddClaimAsync(user, new Claim(ClaimTypes.Email, model.Email));
                        await _signInManager.UserManager.AddClaimAsync(user, new Claim(IoTSharpClaimTypes.Customer, customer.Id.ToString()));
                        await _signInManager.UserManager.AddClaimAsync(user, new Claim(IoTSharpClaimTypes.Tenant, customer.Tenant.Id.ToString()));
                        await _signInManager.UserManager.AddToRolesAsync(user, new[] { nameof(UserRole.NormalUser) });
                        actionResult = CreatedAtAction(nameof(this.Login), new LoginDto() { Email = model.Email, Password = model.Password });
                    }
                }
                else
                {
                    var msg = from e in result.Errors select $"{e.Code}:{e.Description}\r\n";
                    actionResult = BadRequest(new { code = -3, msg = string.Join(';', msg.ToArray()) });
                }
            }
            catch (Exception ex)
            {
                actionResult = BadRequest(new { code = -2, msg = ex.Message, data = ex });
                _logger.LogError(ex, ex.Message);
            }

            return actionResult;
        }
    }
}