using IoTSharp.Hub.Data;
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
        public async Task<IActionResult> Login([FromBody] LoginDto model)
        {
            IActionResult actionResult = NoContent();
            try
            {
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, false, false);
                if (result.Succeeded)
                {
                    var appUser = _userManager.Users.SingleOrDefault(r => r.Email == model.Email);
                    actionResult = Ok(new { code = 0, msg = "OK", data = GenerateJwtToken(model.Email, appUser) });
                }
                else
                {
                    actionResult = BadRequest(new { code = -3, msg = "Login Error", data = result });
                }
            }
            catch (Exception ex)
            {
                actionResult = BadRequest(new { code = -1, msg = ex.Message, data = ex });
            }
            return actionResult;
        }

        /// <summary>
        /// Register a user
        /// </summary>
        /// <param name="model"></param>
        /// <returns ></returns>
        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Register([FromBody] RegisterDto model)
        {
            IActionResult actionResult = NoContent();
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
                    var cust = _context.Customer.FirstOrDefault(c => c.Name == model.CustomerName);

                    if (cust != null)
                    {
                        await _signInManager.UserManager.AddClaimAsync(user, new Claim(ClaimTypes.GroupSid, cust.Id.ToString()));

                        actionResult = Ok(new { code = 0, msg = "OK", data = GenerateJwtToken(model.Email, user) });
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

        private object GenerateJwtToken(string email, IdentityUser user)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.Now.AddDays(Convert.ToDouble(_configuration["JwtExpireDays"]));

            var token = new JwtSecurityToken(
                _configuration["JwtIssuer"],
                _configuration["JwtIssuer"],
                claims,
                expires: expires,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public class LoginDto
        {
            [Required]
            public string Password { get; set; }

            [Required]
            public string Email { get; set; }
        }

        public class RegisterDto
        {
            [Required]
            public string Email { get; set; }

            [Required]
            public string CustomerName { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "PASSWORD_MIN_LENGTH", MinimumLength = 6)]
            public string Password { get; set; }
        }
    }
}