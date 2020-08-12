using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IoTSharp.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using IoTSharp.Dtos;

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
        private readonly string _customerId;

        public AuthorizedKeysController(UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager, ILogger<AuthorizedKeysController> logger, ApplicationDbContext context)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _customerId = this.GetCustomerId();
        }


      /// <summary>
      /// 获取当前已登录用户所属客户的全局认证KEY
      /// </summary>
      /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AuthorizedKey>>> GetAuthorizedKeys()
        {
            return await _context.AuthorizedKeys.JustCustomer(_customerId).ToListAsync();
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
        public async Task<ActionResult<AuthorizedKey>> GetAuthorizedKey(Guid id)
        {
            var authorizedKey = await _context.AuthorizedKeys.JustCustomer(_customerId).FirstOrDefaultAsync(ak=>ak.Id== id);

            if (authorizedKey == null)
            {
                return NotFound();
            }

            return authorizedKey;
        }

        // PUT: api/AuthorizedKeys/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> PutAuthorizedKey(Guid id, AuthorizedKey authorizedKey)
        {
            if (id != authorizedKey.Id)
            {
                return BadRequest();
            }
            if (!AuthorizedKeyExists(id))
            {
                return NotFound();
            }

            _context.Entry(authorizedKey).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AuthorizedKeyExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/AuthorizedKeys
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesDefaultResponseType]
        public async Task<ActionResult<AuthorizedKey>> PostAuthorizedKey(AuthorizedKeyDto    dto)
        {
            var authorizedKey = new AuthorizedKey() { Name = dto.Name, AuthToken = dto.AuthToken };
            authorizedKey.Id = Guid.NewGuid();
            _context.JustFill(this, authorizedKey);
            if (authorizedKey.Tenant == null || authorizedKey.Customer == null)
            {
                return NotFound(new ApiResult<AuthorizedKey>(ApiCode.NotFoundTenantOrCustomer, $"Not found Tenant or Customer ", authorizedKey));
            }
            _context.AuthorizedKeys.Add(authorizedKey);
            await _context.SaveChangesAsync();
            return CreatedAtAction("GetAuthorizedKey", new { id = authorizedKey.Id }, authorizedKey);
        }

        // DELETE: api/AuthorizedKeys/5
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public async Task<ActionResult<AuthorizedKey>> DeleteAuthorizedKey(Guid id)
        {

            var authorizedKey = await _context.AuthorizedKeys.JustCustomer(_customerId).FirstOrDefaultAsync(ak=>ak.Id== id);
            if (authorizedKey == null)
            {
                return NotFound();
            }

            _context.AuthorizedKeys.Remove(authorizedKey);
            await _context.SaveChangesAsync();

            return authorizedKey;
        }

        private bool AuthorizedKeyExists(Guid id)
        {
            return _context.AuthorizedKeys.JustCustomer(_customerId).Any(e => e.Id == id);
        }
    }
}
