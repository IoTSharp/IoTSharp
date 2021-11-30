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
using Esprima.Ast;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Silkier.AspNetCore;
using Silkier.Extensions;
using Jdenticon.AspNetCore;

namespace IoTSharp.Controllers
{
    /// <summary>
    /// 用户管理
    /// </summary>
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

        [HttpGet,AllowAnonymous ]
        public async Task<IActionResult> Avatar()
        {
            var user = await _userManager.GetUserAsync(User);
            return IdenticonResult.FromValue(user.Email+user.Id, 64);
        }
        /// <summary>
        /// 获取当前登录用户信息
        /// </summary>
        /// <returns></returns>
        [HttpGet, Authorize(Roles = nameof(UserRole.NormalUser))]
        public async Task<ActionResult<ApiResult<UserInfoDto>>> MyInfo()
        {
            var user = await _userManager.GetUserAsync(User);
            var rooles = await _userManager.GetRolesAsync(user);
            var Customer = _context.GetCustomer(this.GetNowUserCustomerId());


    
            var uidto = new UserInfoDto()
            {
                Code = ApiCode.Success,
                Roles = string.Join(',', rooles).ToLower().Contains("admin") ? "admin" : "admin",//TODO: Permission control
                Name = user.UserName,
                Email = user.Email,
                Avatar = user.Gravatar(),
                PhoneNumber= user.PhoneNumber,
                Introduction = user.NormalizedUserName,
                Customer = Customer, 
                Tenant = Customer?.Tenant
            };
            return new ApiResult<UserInfoDto>(ApiCode.Success, "OK", uidto);
        }
        /// <summary>
        /// 登录用户
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesDefaultResponseType]
        public async Task<ApiResult<LoginResult>> Login([FromBody] LoginDto model)
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
                    var expires = DateTime.Now.AddHours(_settings.JwtExpireHours);
                    var tokeOptions = new JwtSecurityToken(
                                issuer: _configuration["JwtIssuer"],
                                audience: _configuration["JwtAudience"],
                                claims: claims,
                                expires: expires,
                                signingCredentials: signinCredentials);




                    var t = (expires.Ticks - TimeZoneInfo.ConvertTimeFromUtc(new System.DateTime(1970, 1, 1, 0, 0, 0, 0), TimeZoneInfo.Local).Ticks) / 10000;
                    var token = new TokenEntity
                    {
                        access_token = new JwtSecurityTokenHandler().WriteToken(tokeOptions),
                        expires_in = t
                    };



                    return new ApiResult<LoginResult>(ApiCode.Success, "Ok", new LoginResult()
                    {
                        Code = ApiCode.Success,
                        Succeeded = result.Succeeded,
                        Token = token,
                        UserName = appUser.UserName,
                        SignIn = result,
                        Roles = roles,
                        Avatar = appUser.Gravatar()
                    });
                    //return Ok(new LoginResult()
                    //{
                    //    Code = ApiCode.Success,
                    //    Succeeded = result.Succeeded,
                    //    Token = token,
                    //    UserName = appUser.UserName,
                    //    SignIn = result,
                    //    Roles = roles
                    //});
                }
                else
                {

                    return new ApiResult<LoginResult>(ApiCode.InValidData, "Unauthorized", null);

                    //    return Unauthorized(new { code = ApiCode.LoginError, msg = "Unauthorized", data = result });
                }
            }
            catch (Exception ex)
            {

                return new ApiResult<LoginResult>(ApiCode.InValidData, ex.Message, null);
                //      return this.ExceptionRequest(ex);
            }
        }
        /// <summary>
        /// 退出账号
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesDefaultResponseType]
        public async Task<ApiResult<bool>> Logout()
        {
            try
            {
                await _signInManager.SignOutAsync();

                return new ApiResult<bool>(ApiCode.InValidData, "Ok", true);
                //  return new  OkResult();
            }
            catch (Exception ex)
            {
                return new ApiResult<bool>(ApiCode.InValidData, ex.Message, true);
                //    return this.ExceptionRequest(ex);
            }

        }

        /// <summary>
        /// 注册用户
        /// </summary>
        /// <param name="model"></param>
        /// <returns >返回登录结果</returns>
        [AllowAnonymous]
        [HttpPost]
        public async Task<ApiResult<LoginResult>> Register([FromBody] RegisterDto model)
        {

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
                    var customer = await _context.Customer.Include(c => c.Tenant).FirstOrDefaultAsync(c => c.Email == model.Customer);
                    if (customer != null)
                    {
                        await _signInManager.UserManager.AddClaimAsync(user, new Claim(ClaimTypes.Email, model.Email));
                        await _signInManager.UserManager.AddClaimAsync(user, new Claim(IoTSharpClaimTypes.Customer, customer.Id.ToString()));
                        await _signInManager.UserManager.AddClaimAsync(user, new Claim(IoTSharpClaimTypes.Tenant, customer.Tenant.Id.ToString()));
                        await _signInManager.UserManager.AddToRolesAsync(user, new[] { nameof(UserRole.NormalUser) });
                        return new ApiResult<LoginResult>(ApiCode.Success, "Ok", new LoginResult()
                        {
                            Code = ApiCode.Success,
                            Succeeded = result.Succeeded,
                            UserName = model.Email,

                        });
                        //    actionResult = CreatedAtAction(nameof(this.Login), new LoginDto() { UserName = model.Email,  Password = model.Password });
                    }
                }
                else
                {
                    var msg = from e in result.Errors select $"{e.Code}:{e.Description}\r\n";
                    return new ApiResult<LoginResult>(ApiCode.InValidData, string.Join(';', msg.ToArray()), null);
                }
            }
            catch (Exception ex)
            {
                return new ApiResult<LoginResult>(ApiCode.InValidData, ex.Message, null);
            }
            return new ApiResult<LoginResult>(ApiCode.InValidData, "", null);
        }
        /// <summary>
        /// 返回客户所属用户列表
        /// </summary>
        /// <param name="customerId"></param>
        /// <returns></returns>
        [HttpGet("{customerId}")]
        public async Task<ApiResult<List<UserItemDto>>> All(Guid customerId)
        {
            List<UserItemDto> dtos = new List<UserItemDto>();
            var users = await _userManager.GetUsersForClaimAsync(_signInManager.Context.User.FindFirst(m => m.Type == IoTSharpClaimTypes.Customer && m.Value == customerId.ToString()));
            users.ToList().ForEach(c =>
            {
                var uid = new UserItemDto()
                {
                    Id = c.Id,
                    Email = c.Email,
                    Roles = new List<string>(_userManager.GetRolesAsync(c).Result),
                    PhoneNumber = c.PhoneNumber,
                    AccessFailedCount = c.AccessFailedCount
                };
                dtos.Add(uid);
            });

            return new ApiResult<List<UserItemDto>>(ApiCode.InValidData, "", dtos);

        }


        /// <summary>
        /// 返回用户信息
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<ApiResult<UserItemDto>> Get(String Id)
        {


            var user = await _userManager.FindByIdAsync(Id);
            if (user != null)
            {
                return new ApiResult<UserItemDto>(ApiCode.Success, "", new UserItemDto { PhoneNumber = user.PhoneNumber ,Id = Id, Email = user.Email});
            }

            return new ApiResult<UserItemDto>(ApiCode.CantFindObject, "can't find that user",null );

        }




        /// <summary>
        /// 修改用户信息
        /// </summary>
        /// <returns></returns>
        [HttpPut]
        public async Task<ApiResult<bool>> Modify(UserItemDto user)
        {
            var idu = await _userManager.FindByIdAsync(user.Id);
            idu.PhoneNumber = user.PhoneNumber;
            var result = await _userManager.UpdateAsync(idu);
            return new ApiResult<bool>(ApiCode.Success, "Ok", result.Succeeded);

        }



        /// <summary>
        /// 修改当前用户信息
        /// </summary>
        /// <returns></returns>
        [HttpPut]
        public async Task<ApiResult<bool>> ModifyMyInfo(UserItemDto user)
        {

            var cuser = await _userManager.GetUserAsync(User);

            cuser.PhoneNumber = user.PhoneNumber;
            var result = await _userManager.UpdateAsync(cuser);
            return new ApiResult<bool>(ApiCode.Success, "Ok", result.Succeeded);

        }



        /// <summary>
        /// 修改当前用户信息
        /// </summary>
        /// <returns></returns>
        [HttpPut]
        public async Task<ApiResult<bool>> ModifyMyPassword(UserPassword password)
        {

            if (password.PassNew.Length > 6)
            {

                if (password.PassNew == password.PassNewSecond)
                {
                    var cuser = await _userManager.GetUserAsync(User);
                    var result = await _signInManager.UserManager.ChangePasswordAsync(cuser, password.Pass, password.PassNew);
                    if (result.Succeeded)
                    {
                        return new ApiResult<bool>(ApiCode.Success, "Ok", result.Succeeded);
                    }
                    return new ApiResult<bool>(ApiCode.InValidData, result.Errors.Aggregate("",(x,y)=>x+y.Description+"\n\r"), false);
                }
                return new ApiResult<bool>(ApiCode.InValidData, "Repeat password must be equal new password", false);


            }
            return new ApiResult<bool>(ApiCode.InValidData, "password length must great than six character", false);
        }


    }
}