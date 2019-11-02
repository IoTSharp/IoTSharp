using IoTSharp.Data;
using IoTSharp.Dtos;
using IoTSharp.Extensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using IoTSharp.Extensions.AspNetCore;

namespace IoTSharp.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]/[action]")]
    public class AccountController : ControllerBase
    {
        private ApplicationDbContext _context;
        private readonly AppSettings _settings;
        private readonly ILogger _logger;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly SignInManager<IdentityUser> _signInManager;

        public AccountController(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            IConfiguration configuration, ILogger<AccountController> logger, ApplicationDbContext context,
            IOptions<AppSettings> options  
            )
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _logger = logger;
            _context = context;
            _settings = options.Value;
        }
        [HttpGet, Authorize(Roles = nameof(UserRole.NormalUser))]
        public async Task<ActionResult<ApiResult<UserInfoDto>>> MyInfo()
        {
            var user = await _userManager.GetUserAsync(User);
            var rooles = await _userManager.GetRolesAsync(user);
            string custid =  User.FindFirstValue(IoTSharpClaimTypes.Customer);
            var Customer = _context.Customer.Include(c=>c.Tenant).FirstOrDefault(c => c.Id.ToString() == custid);
            var uidto = new UserInfoDto()
            {
                Code = ApiCode.Success,
                Roles = string.Join(',',rooles).ToLower().Contains("admin")?"admin": "admin",//TODO: Permission control
                Name = user.UserName,
                Email = user.Email,
                Avatar = user.Gravatar(),
                Introduction =  user.NormalizedUserName,
                Customer = Customer,
                Tenant = Customer?.Tenant
            };
            return new ApiResult<UserInfoDto>(ApiCode.Success, "OK", uidto);
        }

        [AllowAnonymous]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesDefaultResponseType]
        public async Task<ActionResult<LoginResult>> Login([FromBody] LoginDto model)
        {
            try
            {
                var result = await _signInManager.PasswordSignInAsync(model.UserName, model.Password, false, false);
                if (result.Succeeded)
                {
                    var appUser = _userManager.Users.SingleOrDefault(r => r.Email == model.UserName);
                    var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtKey"]));
                    var signinCredentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256Signature);
                    var claims = new List<Claim>
                                    {
                                        new Claim(ClaimTypes.Email, appUser.Email),
                                        new Claim(ClaimTypes.NameIdentifier, appUser.Id),
                                        new Claim(ClaimTypes.Name,  appUser.UserName),
                                    };
                    var lstclaims = await _userManager.GetClaimsAsync(appUser);
                    claims.AddRange(lstclaims);
                    var roles = await _userManager.GetRolesAsync(appUser);
                    if (roles != null)
                    {
                        claims.AddRange(from role in roles
                                        select new Claim(ClaimTypes.Role, role));
                    }
                    var tokeOptions = new JwtSecurityToken(
                                issuer: _configuration["JwtIssuer"],
                                audience: _configuration["JwtAudience"],
                                claims: claims,
                                expires: DateTime.Now.AddHours(_settings.JwtExpireHours * 24),
                                signingCredentials: signinCredentials);
                    var token = new TokenEntity
                    {
                        access_token = new JwtSecurityTokenHandler().WriteToken(tokeOptions),
                        expires_in = (int)DateTime.UtcNow.AddHours(_settings.JwtExpireHours * 24).Subtract(new DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalSeconds
                    };
                    return Ok(new LoginResult()
                    {
                        Code = ApiCode.Success,
                        Succeeded = result.Succeeded,
                        Token = token,
                        UserName = appUser.UserName,
                        SignIn = result,
                        Roles = roles
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

        [AllowAnonymous]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> Logout( )
        {
            try
            {
                await _signInManager.SignOutAsync();
                return new  OkResult();
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
                    Email = model.Email,
                    UserName = model.Email,
                    PhoneNumber = model.PhoneNumber
                };
                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    await _signInManager.SignInAsync(user, false);
                    await _signInManager.UserManager.AddClaimAsync(user, new Claim(ClaimTypes.Email, model.Email));
                    var customer = await _context.Customer.Include(c=>c.Tenant).FirstOrDefaultAsync(c=>c.Id== model.CustomerId);
                    if (customer != null)
                    {
                        await _signInManager.UserManager.AddClaimAsync(user, new Claim(ClaimTypes.Email, model.Email));
                        await _signInManager.UserManager.AddClaimAsync(user, new Claim(IoTSharpClaimTypes.Customer, customer.Id.ToString()));
                        await _signInManager.UserManager.AddClaimAsync(user, new Claim(IoTSharpClaimTypes.Tenant, customer.Tenant.Id.ToString()));
                        await _signInManager.UserManager.AddToRolesAsync(user, new[] { nameof(UserRole.NormalUser) });
                        actionResult = CreatedAtAction(nameof(this.Login), new LoginDto() { UserName = model.Email,  Password = model.Password });
                    }
                }
                else
                {
                    var msg = from e in result.Errors select $"{e.Code}:{e.Description}\r\n";
                    actionResult = BadRequest(new ApiResult(ApiCode.CreateUserFailed, string.Join(';', msg.ToArray())));
                }
            }
            catch (Exception ex)
            {
                actionResult = this.ExceptionRequest(ex);
                _logger.LogError(ex, ex.Message);
            }
            return actionResult;
        }

        [HttpGet("{customerId}")]
        public async Task<ActionResult<List<UserItemDto>>> All(Guid customerId)
        {
            List<UserItemDto> dtos = new List<UserItemDto>();
            var users = await _userManager.GetUsersForClaimAsync (_signInManager.Context.User.FindFirst( m=> m.Type==  IoTSharpClaimTypes.Customer && m.Value==customerId.ToString()));
            users.ToList().ForEach(async c =>
            {
                var uid = new UserItemDto()
                {
                    Id =c.Id,
                    Email = c.Email,
                    Roles = new List<string>(await _userManager.GetRolesAsync(c)),
                    PhoneNumber = c.PhoneNumber,
                    AccessFailedCount = c.AccessFailedCount 
                };
                dtos.Add(uid);
            });
            return dtos;
        }

    }
}