using IoTSharp.Contracts;
using IoTSharp.Controllers.Models;
using IoTSharp.Data;
using IoTSharp.Dtos;
using IoTSharp.Extensions;
using IoTSharp.Models;
using Jdenticon.AspNetCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using static IronPython.Modules._ast;

namespace IoTSharp.Controllers
{
    /// <summary>
    /// 用户管理
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/[controller]/[action]")]
    public partial class AccountController : ControllerBase
    {
        private ApplicationDbContext _context;
        private readonly AppSettings _settings;
        private readonly ILogger _logger;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly SignInManager<IdentityUser> _signInManager;

        /// <summary>
        /// 用户管理
        /// </summary>
        /// <param name="userManager"></param>
        /// <param name="signInManager"></param>
        /// <param name="configuration"></param>
        /// <param name="logger"></param>
        /// <param name="context"></param>
        /// <param name="options"></param>
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

        /// <summary>
        /// 获取当前用户的头像， 基于邮箱生成
        /// </summary>
        /// <returns></returns>
        [HttpGet, AllowAnonymous]
        public async Task<IActionResult> Avatar()
        {
            IdenticonResult result = IdenticonResult.FromValue("IoTSharp", 64);
            try
            {
                if (_signInManager.IsSignedIn(this.User))
                {
                    var user = await _userManager.GetUserAsync(User);
                    result = IdenticonResult.FromValue($"iotsharp_{user?.Email}_{user?.Id}", 64);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"生成头像失败:{ex.Message}");
            }
            return result;
        }

        /// <summary>
        /// 获取当前登录用户信息
        /// </summary>
        /// <returns></returns>
        [HttpGet, Authorize(Roles = nameof(UserRole.NormalUser))]
        public async Task<ActionResult<ApiResult<UserInfoDto>>> MyInfo()
        {
            ApiResult<UserInfoDto> result;
            var user = await _userManager.GetUserAsync(User);
            var custid = User.GetCustomerId();
            if (user == null || custid == Guid.Empty)
            {
                result = new ApiResult<UserInfoDto>(ApiCode.UserTokenNotAvailable, "用户的登录信息已经不可用", new UserInfoDto() { Code = ApiCode.UserTokenNotAvailable });
            }
            else
            {
                try
                {
                    var rooles = await _userManager.GetRolesAsync(user);
                    var Customer = _context.GetCustomer(custid);
                    var uidto = new UserInfoDto()
                    {
                        Code = ApiCode.Success,
                        Roles = string.Join(',', rooles).ToLower(),
                        Name = user.UserName,
                        Email = user.Email,
                        Avatar = user.Gravatar(),
                        PhoneNumber = user.PhoneNumber,
                        Introduction = user.NormalizedUserName,
                        Customer = Customer,
                        Tenant = Customer?.Tenant
                    };
                    result = new ApiResult<UserInfoDto>(ApiCode.Success, "OK", uidto);
                }
                catch (Exception ex)
                {
                    result = new ApiResult<UserInfoDto>(ApiCode.UserTokenNotAvailable, ex.Message, new UserInfoDto() { Code = ApiCode.UserTokenNotAvailable });
                }
            }
            return result;
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
                    var SignInResult = await CreateToken(model.UserName);
                    return new ApiResult<LoginResult>(ApiCode.Success, "Ok", new LoginResult()
                    {
                        Code = ApiCode.Success,
                        Succeeded = result.Succeeded,
                        Token = new TokenEntity
                        {
                            access_token = SignInResult.Token,
                            expires_in = SignInResult.ExpiresIn,
                            refresh_token = SignInResult.RefreshToken,
                            expires = SignInResult.Expires
                        },
                        UserName = model.UserName,
                        SignIn = result,
                        Roles = SignInResult.Roles,
                        Avatar = SignInResult.AppUser.Gravatar()
                    });
                }
                else
                {
                    return new ApiResult<LoginResult>(ApiCode.LoginError, "用户名或密码错误。", new LoginResult() { Code = ApiCode.LoginError, Succeeded = false, SignIn = result });
                }
            }
            catch (Exception ex)
            {
                return new ApiResult<LoginResult>(ApiCode.InValidData, ex.Message, null);
            }
        }


        private async Task<ModelRefreshToken> CreateToken(string name)
        {
            var appUser = _userManager.Users.SingleOrDefault(r => r.Email == name);
            var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtKey"]));
            var signinCredentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256Signature);
            var claims = new List<Claim>
            {
                new(ClaimTypes.Email, appUser.Email),
                new(ClaimTypes.NameIdentifier, appUser.Id),
                new(ClaimTypes.Name,  appUser.UserName),
            };
            var lstclaims = await _userManager.GetClaimsAsync(appUser);
            claims.AddRange(lstclaims);
            var roles = await _userManager.GetRolesAsync(appUser);
            if (roles != null)
            {
                claims.AddRange(from role in roles
                                select new Claim(ClaimTypes.Role, role));
            }
            var expires = DateTime.UtcNow.AddHours(_settings.JwtExpireHours);
            var jwtSecurityTokenHandler = new JwtSecurityTokenHandler();

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Issuer = _configuration["JwtIssuer"],
                Audience = _configuration["JwtAudience"],
                Subject = new ClaimsIdentity(claims),
                Expires = expires,
                SigningCredentials = signinCredentials
            };
            var _token = jwtSecurityTokenHandler.CreateToken(tokenDescriptor);
            var jwtToken = jwtSecurityTokenHandler.WriteToken(_token);
            var refreshToken = new RefreshToken()
            {
                JwtId = _token.Id,
                IsUsed = false,
                IsRevorked = false,
                UserId = appUser.Id,
                AddedDate = DateTime.UtcNow,
                ExpiryDate = DateTime.UtcNow.AddHours(_settings.JwtExpireHours),
                Token = Guid.NewGuid().ToString()
            };
            await _context.RefreshTokens.AddAsync(refreshToken);
            await _context.SaveChangesAsync();
            return new ModelRefreshToken() { RefreshToken = refreshToken.Token, Token = jwtToken, ExpiresIn = (long)(_settings.JwtExpireHours * 3600), AppUser = appUser, Roles = roles, Expires = expires };
        }

        /// <summary>
        /// 刷新JWT Token
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ApiResult<LoginResult>> RefreshToken([FromBody] RefreshTokenDto model)
        {
            var profile = this.GetUserProfile();
            try
            {
                var storedRefreshToken = await _context.RefreshTokens.FirstOrDefaultAsync(x => x.Token == model.RefreshToken);
                if (storedRefreshToken == null)
                {
                    return new ApiResult<LoginResult>(ApiCode.InValidData, "RefreshToken does not exist", null);
                }

                if (storedRefreshToken.IsRevorked)

                {
                    return new ApiResult<LoginResult>(ApiCode.InValidData, "RefreshToken is revorked", null);
                }

                storedRefreshToken.IsUsed = true;
                _context.RefreshTokens.Update(storedRefreshToken);
                await _context.SaveChangesAsync();
                var signInResult = await CreateToken(profile.Name);
                return new ApiResult<LoginResult>(ApiCode.Success, "Ok", new LoginResult()
                {
                    Code = ApiCode.Success,
                    Succeeded = true,
                    UserName = profile.Name,
                    Roles = profile.Roles,
                    Token = new TokenEntity
                    {
                        access_token = signInResult.Token,
                        expires_in = signInResult.ExpiresIn,
                        refresh_token = signInResult.RefreshToken,
                        expires = signInResult.Expires
                    },
                });
            }
            catch (Exception ex)
            {
                return new ApiResult<LoginResult>(ApiCode.Exception, ex.Message, null);
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
                return new ApiResult<bool>(ApiCode.Success, "Ok", true);
            }
            catch (Exception ex)
            {
                return new ApiResult<bool>(ApiCode.Exception, ex.Message, true);
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
        /// 注册新的租户，客户，以及用户
        /// </summary>
        /// <param name="model"></param>
        /// <returns >返回登录结果</returns>
        [AllowAnonymous]
        [HttpPost]
        public async Task<ApiResult<LoginResult>> Create([FromBody] InstallDto model)
        {
            var tenant = _context.Tenant.FirstOrDefault(t => t.Email == model.TenantEMail && t.Deleted == false);
            var customer = _context.Customer.FirstOrDefault(t => t.Email == model.CustomerEMail && t.Deleted == false);
            if (tenant == null && customer == null)
            {
                tenant = new Tenant() { Id = Guid.NewGuid(), Name = model.TenantName, Email = model.TenantEMail };
                customer = new Customer() { Id = Guid.NewGuid(), Name = model.CustomerName, Email = model.CustomerEMail };
                customer.Tenant = tenant;
                tenant.Customers = new List<Customer>();
                tenant.Customers.Add(customer);
                _context.Tenant.Add(tenant);
                _context.Customer.Add(customer);
                await _context.SaveChangesAsync();
            }
            IdentityUser user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                user = new IdentityUser
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
                    await _signInManager.UserManager.AddClaimAsync(user, new Claim(IoTSharpClaimTypes.Customer, customer.Id.ToString()));
                    await _signInManager.UserManager.AddClaimAsync(user, new Claim(IoTSharpClaimTypes.Tenant, tenant.Id.ToString()));
                    await _signInManager.UserManager.AddToRoleAsync(user, nameof(UserRole.Anonymous));
                    await _signInManager.UserManager.AddToRoleAsync(user, nameof(UserRole.NormalUser));
                    await _signInManager.UserManager.AddToRoleAsync(user, nameof(UserRole.CustomerAdmin));
                    await _signInManager.UserManager.AddToRoleAsync(user, nameof(UserRole.TenantAdmin));
                    _context.Relationship.Add(new Relationship
                    {
                        IdentityUser = _context.Users.Find(user.Id),
                        Customer = _context.Customer.Find(customer.Id),
                        Tenant = _context.Tenant.Find(tenant.Id)
                    });
                    await _context.SaveChangesAsync();
                    return new ApiResult<LoginResult>(ApiCode.Success, "OK", null);
                }
                else
                {
                    throw new Exception(string.Join(',', result.Errors.ToList().Select(ie => $"code={ie.Code},msg={ie.Description}")));
                }
            }
            else
            {
                return new ApiResult<LoginResult>(ApiCode.UserAlreadyExists, "The user already exists", null);
            }
        }

        /// <summary>
        /// 注册新的租户，客户，以及用户
        /// </summary>
        /// <param name="model"></param>
        /// <returns >返回登录结果</returns>
        [AllowAnonymous]
        [HttpPost]
        public async Task<ApiResult<LoginResult>> CreateUser([FromBody] CreateUserDto model)
        {
            var customer = await _context.Customer.Include(c => c.Tenant).FirstOrDefaultAsync(c => c.Id == model.Customer);
            if (customer != null && customer.Tenant != null)
            {
                var tid = customer.Tenant.Id;
                IdentityUser user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null)
                {
                    user = new IdentityUser
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
                        await _signInManager.UserManager.AddClaimAsync(user, new Claim(IoTSharpClaimTypes.Customer, model.Customer.ToString()));
                        await _signInManager.UserManager.AddClaimAsync(user, new Claim(IoTSharpClaimTypes.Tenant, tid.ToString()));
                        await _signInManager.UserManager.AddToRoleAsync(user, nameof(UserRole.NormalUser));
                        var rship = new Relationship
                        {
                            IdentityUser = _context.Users.Find(user.Id),
                            Customer = _context.Customer.Find(model.Customer),
                            Tenant = _context.Tenant.Find(tid)
                        };
                        _context.Add(rship);
                        await _context.SaveChangesAsync();
                        return new ApiResult<LoginResult>(ApiCode.Success, "OK", null);
                    }
                    else
                    {
                        throw new Exception(string.Join(',', result.Errors.ToList().Select(ie => $"code={ie.Code},msg={ie.Description}")));
                    }
                }
                else
                {
                    return new ApiResult<LoginResult>(ApiCode.UserAlreadyExists, "用户已存在", null);
                }
            }
            else
            {
                return new ApiResult<LoginResult>(ApiCode.NotFoundCustomer, "未找到客户", null);
            }
        }
        /// <summary>
        /// 为当前客户所在的租户新增用户  zhangjie 20230308
        /// </summary>
        /// <param name="model">前端传参</param>
        /// <returns></returns>
        [Authorize(Roles = nameof(UserRole.CustomerAdmin))]
        [HttpPost]
        public async Task<ApiResult<LoginResult>> PostAccount(CreateUserInput model)
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
                    if (model.CustomerId != Guid.Empty)
                    {
                        var customer = await _context.Customer.Include(c => c.Tenant).FirstOrDefaultAsync(c => c.Id == model.CustomerId);
                        if (customer != null)
                        {
                            await _signInManager.UserManager.AddClaimAsync(user, new Claim(ClaimTypes.Email, model.Email));
                            await _signInManager.UserManager.AddClaimAsync(user, new Claim(IoTSharpClaimTypes.Customer, customer.Id.ToString()));
                            await _signInManager.UserManager.AddClaimAsync(user, new Claim(IoTSharpClaimTypes.Tenant, customer.Tenant.Id.ToString()));
                            await _signInManager.UserManager.AddToRolesAsync(user, new[] { nameof(UserRole.NormalUser) });

                            await _signInManager.UserManager.AddToRoleAsync(user, nameof(UserRole.Anonymous));
                            await _signInManager.UserManager.AddToRoleAsync(user, nameof(UserRole.NormalUser));
                            await _signInManager.UserManager.AddToRoleAsync(user, nameof(UserRole.CustomerAdmin));
                            await _signInManager.UserManager.AddToRoleAsync(user, nameof(UserRole.TenantAdmin));
                            await _signInManager.UserManager.AddToRoleAsync(user, nameof(UserRole.SystemAdmin));
                            var rship = new Relationship
                            {
                                IdentityUser = _context.Users.Find(user.Id),
                                Customer = customer,
                                Tenant = customer.Tenant
                            };
                            _context.Add(rship);
                            await _context.SaveChangesAsync();

                            return new ApiResult<LoginResult>(ApiCode.Success, "Ok", new LoginResult()
                            {
                                Code = ApiCode.Success,
                                Succeeded = result.Succeeded,
                                UserName = model.Email,
                            });
                        }
                    }
                    else
                    {
                        return new ApiResult<LoginResult>(ApiCode.Success, "Ok", new LoginResult()
                        {
                            Code = ApiCode.Success,
                            Succeeded = result.Succeeded,
                            UserName = model.Email,
                        });
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
        /// 列出指定租户的所有用户。
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        public async Task<ApiResult<PagedData<UserItemDto>>> List([FromQuery] UserQueryDto m)
        {
            var mx = m.CustomerId.ToString();
            //如果前端传参有 客户Id，则查询， 为空暂时查全部
            var srcDb = _context.Users.Join<IdentityUser, Relationship, string, UserItemDto>(_context.Relationship.Include(p => p.Tenant).ThenInclude(p => p.Customers).Where(x => x.Customer.Id == m.CustomerId).AsEnumerable(),
                 t => t.Id, s => s.IdentityUser.Id, (t, s) => new UserItemDto
                 {
                     Id = t.Id,
                     UserName = t.UserName,
                     Email = t.Email,
                     PhoneNumber = t.PhoneNumber,
                     AccessFailedCount = t.AccessFailedCount
                 ,
                     LockoutEnabled = t.LockoutEnabled,
                     LockoutEnd = t.LockoutEnd,
                     CustomerName = s.Customer.Name,
                     TenantName = s.Tenant.Name
                 });


            if (m.CustomerId != Guid.Empty)
            {
                var data = await m.Query(srcDb, c => c.Id != "", c => c.UserName, c => new UserItemDto()
                {
                    Id = c.Id,
                    UserName = c.UserName,
                    Email = c.Email,
                    PhoneNumber = c.PhoneNumber,
                    AccessFailedCount = c.AccessFailedCount,
                    LockoutEnabled = c.LockoutEnabled,
                    LockoutEnd = c.LockoutEnd,
                    CustomerName = c.CustomerName,
                    TenantName = c.TenantName

                });
                return new ApiResult<PagedData<UserItemDto>>(ApiCode.Success, "OK", data);

            }
            else
            {
                var data = await m.Query(_context.Users, c => c.Id != "", c => c.UserName, c => new UserItemDto()
                {
                    Id = c.Id,
                    UserName = c.UserName,
                    Email = c.Email,
                    PhoneNumber = c.PhoneNumber,
                    AccessFailedCount = c.AccessFailedCount,
                    LockoutEnabled = c.LockoutEnabled,
                    LockoutEnd = c.LockoutEnd,

                });
                return new ApiResult<PagedData<UserItemDto>>(ApiCode.Success, "OK", data);

            }
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
                return new ApiResult<UserItemDto>(ApiCode.Success, "", new UserItemDto { PhoneNumber = user.PhoneNumber, Id = Id, Email = user.Email });
            }

            return new ApiResult<UserItemDto>(ApiCode.CantFindObject, "can't find that user", null);
        }

        /// <summary>
        /// 锁定用户
        /// </summary>
        /// <param name="dto"></param>
        /// <returns>
        /// UserAlreadyExists = 10020,
        /// NotFoundUser = 10021,
        /// CanNotLockUser = 10022,
        ///LockUserHaveError = 10023
        ///CanNotLockYourself =10028 
        ///CanNotUnlockYourself =10029 
        ///</returns>
        [HttpPut]
        public async Task<ApiResult> Lock(LockDto dto)
        {
            var result = new ApiResult();
            var user = await _userManager.FindByIdAsync(dto.Id.ToString());
            if (user != null)
            {
                var les = await _userManager.GetLockoutEnabledAsync(user);
                switch (dto.Opt)
                {
                    case LockOpt.Status:
                        result = new ApiResult(ApiCode.Success, $"{les}");
                        break;

                    case LockOpt.Lock:
                        if (User.GetUserId() == dto.Id)
                        {
                            result = new ApiResult(ApiCode.CanNotLockYourself, "不能自己锁定自己的账号");
                        }
                        else
                        {
                            var lce = await _userManager.SetLockoutEnabledAsync(user, true);
                            if (lce.Succeeded)
                            {
                                /**20230309 zhangjie modify
                                 * 此处修改Now为UtcNow，否则报错：Cannot write DateTimeOffset with Offset=08:00:00 to PostgreSQL type 'timestamp with time zone'
                                 * 如果此处不改，网上给出的另一个方式是在Main方法中添加：
                                 * AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
                                 * AppContext.SetSwitch("Npgsql.DisableDateTimeInfinityConversions", true);
                                 */
                                var led = await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddYears(100));
                                if (led.Succeeded)
                                {
                                    result = new ApiResult(ApiCode.Success, "OK");
                                }
                                else
                                {
                                    result = new ApiResult(ApiCode.LockUserHaveError, "锁定用户时遇到错误" + string.Join(';', led.Errors.Select(c => $"{c.Code}-{c.Description}")));
                                }
                            }
                            else
                            {
                                result = new ApiResult(ApiCode.CanNotLockUser, "无法锁定此用户" + string.Join(';', lce.Errors.Select(c => $"{c.Code}-{c.Description}")));
                            }
                        }
                        break;

                    case LockOpt.Unlock:
                        if (User.GetUserId() == dto.Id)
                        {
                            result = new ApiResult(ApiCode.CanNotUnlockYourself, "不能自己解锁自己账号");
                        }
                        else
                        {
                            if (les)
                            {
                                var led = await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow);

                                if (led.Succeeded)
                                {
                                    var uld = await _userManager.SetLockoutEnabledAsync(user, false);
                                    if (uld.Succeeded)
                                    {
                                        result = new ApiResult(ApiCode.Success, "OK");
                                    }
                                    else
                                    {
                                        result = new ApiResult(ApiCode.LockUserHaveError, "解锁用户时遇到错误" + string.Join(';', uld.Errors.Select(c => $"{c.Code}-{c.Description}")));
                                    }
                                }
                                else
                                {
                                    result = new ApiResult(ApiCode.UnLockUserHaveError, "解锁用户时遇到错误" + string.Join(';', led.Errors.Select(c => $"{c.Code}-{c.Description}")));
                                }
                            }
                            else
                            {
                                result = new ApiResult(ApiCode.CanNotUnLockUser, "无法解锁此用户");
                            }
                        }
                        break;

                    default:
                        break;
                }
            }
            else
            {
                result = new ApiResult(ApiCode.NotFoundUser, "未找到此用户");
            }

            return result;
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
                    return new ApiResult<bool>(ApiCode.InValidData, result.Errors.Aggregate("", (x, y) => x + y.Description + "\n\r"), false);
                }
                return new ApiResult<bool>(ApiCode.InValidData, "Repeat password must be equal new password", false);
            }
            return new ApiResult<bool>(ApiCode.InValidData, "password length must great than six character", false);
        }

        [AllowAnonymous]
        [HttpGet]
        public ApiResult<bool> CheckExist(string email, int type)
        {
            if (string.IsNullOrEmpty(email))
            {
                return new ApiResult<bool>(ApiCode.Success, "OK", false);
            }
            else
            {
                switch (type)
                {
                    case 1:
                        return new ApiResult<bool>(ApiCode.Success, "OK", _context.Tenant.Any(c => c.Email.ToLower() == email.ToLower() && c.Deleted == false));

                    case 2:
                        return new ApiResult<bool>(ApiCode.Success, "OK", _context.Customer.Any(c => c.Email.ToLower() == email.ToLower() && c.Deleted == false));

                    case 3:
                        return new ApiResult<bool>(ApiCode.Success, "OK", _context.Users.Any(c => c.Email.ToLower() == email.ToLower()));
                }
                return new ApiResult<bool>(ApiCode.Success, "OK", false);
            }
        }
        [AllowAnonymous]
        [HttpGet]
        public async Task<ApiResult> ResetPasswordAsync(string email, string rootkey, string newpassword)
        {
            IdentityUser user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return new ApiResult(ApiCode.NotFoundUser, "用户为空");
            }
            else
            {
                if (!string.IsNullOrEmpty(rootkey) && rootkey == _settings.RootKey)
                {
                    if (user.LockoutEnabled)
                    {
                        var ck = await Lock(new LockDto() { Id = new Guid(user.Id), Opt = LockOpt.Unlock });
                    }
                    await _userManager.ResetAccessFailedCountAsync(user);
                    var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                    var rest = await _userManager.ResetPasswordAsync(user, token, newpassword);
                    if (rest.Succeeded)
                    {
                        return new ApiResult(ApiCode.Success, "OK");
                    }
                    else
                    {
                        var errinfo = string.Join(',', rest.Errors.Select(c => $"{c.Code}-{c.Description}").ToList());
                        return new ApiResult(ApiCode.Exception, errinfo);
                    }
                }
                else
                {
                    return new ApiResult(ApiCode.InValidData, "根密码不对，请修改服务器配置文件。");
                }

            }
        }
    }
}