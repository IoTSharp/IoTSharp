using IoTSharp.Contracts;
using IoTSharp.Data;
using IoTSharp.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IoTSharp.Controllers
{
    /// <summary>
    /// 全局设备认证KEY管理
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class AuthorizedKeysController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ILogger _logger;

        public AuthorizedKeysController(UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager, ILogger<AuthorizedKeysController> logger, ApplicationDbContext context)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        /// <summary>
        /// 获取当前已登录用户所属客户的全局认证KEY
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AuthorizedKey>>> GetAuthorizedKeys()
        {
            return await _context.AuthorizedKeys.JustCustomer(User.GetCustomerId()).Where(ak => ak.Deleted == false).ToListAsync();
        }

        /// <summary>
        /// 根据ID获取KEY
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public async Task<ApiResult<AuthorizedKey>> GetAuthorizedKey(Guid id)
        {
            var authorizedKey = await _context.AuthorizedKeys.JustCustomer(User.GetCustomerId()).FirstOrDefaultAsync(ak => ak.Id == id);

            if (authorizedKey == null)
            {
                return new ApiResult<AuthorizedKey>(ApiCode.InValidData, "can't find this object", null);
            }
            return new ApiResult<AuthorizedKey>(ApiCode.Success, "Ok", authorizedKey);
        }

        // PUT: api/AuthorizedKeys/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ApiResult<bool>> PutAuthorizedKey(Guid id, AuthorizedKey authorizedKey)
        {
            if (id != authorizedKey.Id)
            {
                return new ApiResult<bool>(ApiCode.InValidData, "can't find this object", false);
            }
            if (!AuthorizedKeyExists(id))
            {
                return new ApiResult<bool>(ApiCode.CantFindObject, "can't find this object", false);
            }

            _context.Entry(authorizedKey).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                return new ApiResult<bool>(ApiCode.Success, "Ok", true);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AuthorizedKeyExists(id))
                {
                    return new ApiResult<bool>(ApiCode.CantFindObject, "can't find this object", false);
                }
                else
                {
                    throw;
                }
            }
        }

        // POST: api/AuthorizedKeys
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesDefaultResponseType]
        public async Task<ApiResult<AuthorizedKey>> PostAuthorizedKey(AuthorizedKeyDto dto)
        {
            var authorizedKey = new AuthorizedKey() { Name = dto.Name, AuthToken = dto.AuthToken };
            authorizedKey.Id = Guid.NewGuid();
            _context.JustFill(this, authorizedKey);
            if (authorizedKey.Tenant == null || authorizedKey.Customer == null)
            {
                return new ApiResult<AuthorizedKey>(ApiCode.CantFindObject, "can't find this object", null);
            }
            _context.AuthorizedKeys.Add(authorizedKey);
            await _context.SaveChangesAsync();
            return await GetAuthorizedKey(authorizedKey.Id);
        }

        // DELETE: api/AuthorizedKeys/5
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public async Task<ApiResult<AuthorizedKey>> DeleteAuthorizedKey(Guid id)
        {
            var authorizedKey = await _context.AuthorizedKeys.JustCustomer(User.GetCustomerId()).FirstOrDefaultAsync(ak => ak.Id == id);
            if (authorizedKey == null)
            {
                return new ApiResult<AuthorizedKey>(ApiCode.CantFindObject, "can't find this object", null);
            }
            authorizedKey.Deleted = false;
            _context.AuthorizedKeys.Update(authorizedKey);
            await _context.SaveChangesAsync();

            return new ApiResult<AuthorizedKey>(ApiCode.Success, "Ok", authorizedKey); ;
        }

        private bool AuthorizedKeyExists(Guid id)
        {
            return _context.AuthorizedKeys.JustCustomer(User.GetCustomerId()).Any(e => e.Id == id && e.Deleted == false);
        }
    }
}