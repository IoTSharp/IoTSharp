using IoTSharp.Contracts;
using IoTSharp.Controllers.Models;
using IoTSharp.Data;
using IoTSharp.Dtos;
using IoTSharp.Extensions;
using IoTSharp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Customer = IoTSharp.Data.Customer;

namespace IoTSharp.Controllers
{
    /// <summary>
    /// 客户管理
    /// </summary>
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class CustomersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CustomersController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// 获取指定租户下的所有客户
        /// </summary>
        /// <returns></returns>
        [HttpPost("Tenant/{tenantId}/All")]
        [Authorize(Roles = nameof(UserRole.NormalUser))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public async Task<ApiResult<List<Customer>>> GetAllCustomers([FromRoute] Guid tenantId)
        {
            return new ApiResult<List<Customer>>(ApiCode.Success, "OK", await _context.Customer.Where(c => c.Tenant.Id == tenantId && c.Deleted == false).ToListAsync());
        }

        /// <summary>
        /// 获取指定租户下的所有客户
        /// </summary>
        /// <returns></returns>
        [HttpPost("Tenant")]
        [Authorize(Roles = nameof(UserRole.TenantAdmin))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public async Task<ApiResult<PagedData<Customer>>> GetCustomers([FromBody] CustomerParam m)
        {
            var profile = this.GetUserProfile();
            if (m.tenantId != Guid.Empty)
            {
                var querym = _context.Customer.Include(c => c.Tenant).Where(c => !c.Deleted && c.Tenant.Id == m.tenantId);
                var data = await m.Query(querym, c => c.Name);
                return new ApiResult<PagedData<Customer>>(ApiCode.Success, "OK", data);
            }
            else
            {

                return new ApiResult<PagedData<Customer>>(ApiCode.NotFoundCustomer, "没有指定客户ID", new PagedData<Customer>());
            }

        }

        /// <summary>
        /// 获取指定用户下的AI设置
        /// </summary>
        /// <returns></returns>
        [HttpPost("{customerId}/ai")]
        [Authorize(Roles = nameof(UserRole.CustomerAdmin))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public async Task<ApiResult<AISettingsDto>> SetAISettings([FromRoute] Guid customerId, [FromBody] AISettingsSetDto dto)
        {
            var profile = this.GetUserProfile();
            if (customerId != Guid.Empty)
            {
                var querym = _context.Customer.Include(c => c.Tenant).Include(s => s.AISettings).Where(c => !c.Deleted && c.Tenant.Id == profile.Tenant && c.Id == customerId);
                if (querym.Any())
                {
                    var d = await querym.FirstOrDefaultAsync();

                    if (d.AISettings == null)
                    {
                        var dx = new AISettings();
                        dx.Id = Guid.NewGuid();
                        dx.Role = UserRole.CustomerAdmin;
                        _context.AISettings.Add(dx);
                        d.AISettings = dx;
                    }
                    d.AISettings.Enable = dto.Enable;
                    d.AISettings.MCP_API_KEY = Guid.NewGuid().ToString();
                    d.AISettings.Name = dto.Name;
                    int ret = await _context.SaveChangesAsync();
                    var dtor = new AISettingsDto() { Enable = d.AISettings.Enable, MCP_API_KEY = d.AISettings.MCP_API_KEY, Name = d.AISettings.Name };
                    return new ApiResult<AISettingsDto>(ApiCode.Success, "OK", dtor);
                }
                else
                {
                    return new ApiResult<AISettingsDto>(ApiCode.NotFoundCustomer, "没有指定客户", new AISettingsDto());
                }
            }
            else
            {

                return new ApiResult<AISettingsDto>(ApiCode.InValidData, "传入的客户ID为空", new AISettingsDto());
            }

        }

        /// <summary>
        /// 获取指定用户下的AI设置
        /// </summary>
        /// <returns></returns>
        [HttpGet("{customerId}/ai")]
        [Authorize(Roles = nameof(UserRole.CustomerAdmin))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public async Task<ApiResult<AISettingsDto>> GetAISettings([FromRoute] Guid customerId)
        {
            var profile = this.GetUserProfile();
            if (customerId != Guid.Empty)
            {
                var querym = _context.Customer.Include(c => c.Tenant).Include(s => s.AISettings).Where(c => !c.Deleted && c.Tenant.Id == profile.Tenant && c.Id == customerId);
                if (querym.Any())
                {
                    var d = await querym.FirstOrDefaultAsync();
                    var s = d?.AISettings ?? new AISettings() { Name = "None" };
                    var data = new AISettingsDto() { Enable = s.Enable, MCP_API_KEY = s.MCP_API_KEY, Name = s.Name };
                    return new ApiResult<AISettingsDto>(ApiCode.Success, "OK", data);
                }
                else
                {
                    return new ApiResult<AISettingsDto>(ApiCode.NotFoundCustomer, "没有指定客户", new AISettingsDto());
                }
            }
            else
            {

                return new ApiResult<AISettingsDto>(ApiCode.InValidData, "传入的客户ID为空", new AISettingsDto());
            }

        }


        /// <summary>
        /// 返回当前用户权限范围内的未删除客户信息。
        /// </summary>
        /// <param name="id">客户标识。</param>
        /// <returns>可访问的客户；不存在或超出权限范围时不返回客户数据。</returns>
        [Authorize(Roles = nameof(UserRole.NormalUser))]
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public async Task<ApiResult<Customer>> GetCustomer(Guid id)
        {
            if (id == Guid.Empty)
            {
                return new ApiResult<Customer>(ApiCode.NotFoundCustomer, "This customer was not found", null);
            }

            var customer = await _context.Customer
                .Include(c => c.Tenant)
                .FirstOrDefaultAsync(c => c.Id == id
                    && !c.Deleted
                    && c.Tenant != null
                    && !c.Tenant.Deleted);
            if (customer == null || !CanAccessCustomer(customer))
            {
                return new ApiResult<Customer>(ApiCode.NotFoundCustomer, "This customer was not found", null);
            }
            return new ApiResult<Customer>(ApiCode.Success, "OK", customer);
        }

        /// <summary>
        /// 修改当前用户权限范围内的客户资料，保留数据库中的租户归属和管理状态。
        /// </summary>
        /// <param name="id">客户标识，必须与请求中的标识一致。</param>
        /// <param name="customer">客户资料；空 TenantId 兼容旧客户端，非空值必须等于持久化归属。</param>
        /// <returns>更新后的客户；非法请求或超出权限范围时不返回客户数据。</returns>
        [Authorize(Roles = nameof(UserRole.CustomerAdmin))]
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ApiResult<Customer>> PutCustomer(Guid id, CustomerDto customer)
        {
            if (customer is null || id == Guid.Empty || id != customer.Id)
            {
                return new ApiResult<Customer>(ApiCode.InValidData, "InValidData", null);
            }

            var persisted = await _context.Customer
                .Include(c => c.Tenant)
                .FirstOrDefaultAsync(c => c.Id == id
                    && !c.Deleted
                    && c.Tenant != null
                    && !c.Tenant.Deleted);
            if (persisted == null || !CanAccessCustomer(persisted))
            {
                return new ApiResult<Customer>(ApiCode.NotFoundCustomer, "This customer was not found", null);
            }

            // 空 TenantId 兼容旧客户端；导航对象和管理字段由下方资料字段白名单忽略。
            if (customer.TenantId != Guid.Empty && customer.TenantId != persisted.Tenant.Id)
            {
                return new ApiResult<Customer>(ApiCode.InValidData, "InValidData", null);
            }

            persisted.Name = customer.Name;
            persisted.Email = customer.Email;
            persisted.Phone = customer.Phone;
            persisted.Country = customer.Country;
            persisted.Province = customer.Province;
            persisted.City = customer.City;
            persisted.Street = customer.Street;
            persisted.Address = customer.Address;
            persisted.ZipCode = customer.ZipCode;
            try
            {
                await _context.SaveChangesAsync();

                return new ApiResult<Customer>(ApiCode.Success, "", persisted);
            }
            catch (DbUpdateConcurrencyException)
            {
                return new ApiResult<Customer>(ApiCode.NotFoundCustomer, "This customer was not found", null);
            }
        }

        /// <summary>
        /// 按当前用户角色和 JWT 归属声明校验客户范围。
        /// </summary>
        /// <param name="customer">已从数据库加载并包含租户的客户。</param>
        /// <returns>客户属于当前用户可管理范围时返回 <see langword="true"/>。</returns>
        private bool CanAccessCustomer(Customer customer)
        {
            if (User.IsInRole(nameof(UserRole.SystemAdmin)))
            {
                return true;
            }

            if (customer.Tenant == null || customer.Tenant.Deleted)
            {
                return false;
            }

            if (!TryGetClaimId(IoTSharpClaimTypes.Tenant, out var tenantId)
                || tenantId != customer.Tenant.Id)
            {
                return false;
            }

            if (User.IsInRole(nameof(UserRole.TenantAdmin)))
            {
                return true;
            }

            return (User.IsInRole(nameof(UserRole.CustomerAdmin))
                    || User.IsInRole(nameof(UserRole.NormalUser)))
                && TryGetClaimId(IoTSharpClaimTypes.Customer, out var customerId)
                && customerId == customer.Id;
        }

        /// <summary>
        /// 严格读取单一的非空 GUID 声明，缺失、重复或格式错误均拒绝。
        /// </summary>
        /// <param name="claimType">声明类型。</param>
        /// <param name="id">解析后的标识。</param>
        /// <returns>声明有效时返回 <see langword="true"/>。</returns>
        private bool TryGetClaimId(string claimType, out Guid id)
        {
            id = Guid.Empty;
            var values = User.Claims
                .Where(claim => claim.Type == claimType)
                .Select(claim => claim.Value)
                .ToArray();
            return values.Length == 1
                && Guid.TryParse(values[0], out id)
                && id != Guid.Empty;
        }

        /// <summary>
        /// 为当前客户所在的租户新增客户
        /// </summary>
        /// <param name="customer"></param>
        /// <returns></returns>
        [Authorize(Roles = nameof(UserRole.CustomerAdmin))]
        [HttpPost]
        public async Task<ApiResult<Customer>> PostCustomer(CustomerDto customer)
        {
            if (customer.TenantId != Guid.Empty && (User.IsInRole(nameof(UserRole.SystemAdmin)) || User.IsInRole(nameof(UserRole.TenantAdmin))))
            {
                var tent = _context.Tenant.Find(customer.TenantId);
                customer.Tenant = tent;
            }
            else
            {
                var tid = User.Claims.First(c => c.Type == IoTSharpClaimTypes.Tenant);
                var tidguid = new Guid(tid.Value);
                var tent = _context.Tenant.Find(tidguid);
                customer.Tenant = tent;
            }
            _context.Customer.Add(customer);
            await _context.SaveChangesAsync();

            return new ApiResult<Customer>(ApiCode.Success, "Ok", await _context.Customer.SingleOrDefaultAsync(c => c.Id == customer.Id));
        }

        /// <summary>
        /// 删除指定的客户ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        // DELETE: api/Customers/5
        [Authorize(Roles = nameof(UserRole.TenantAdmin))]
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public async Task<ApiResult<Customer>> DeleteCustomer(Guid id)
        {
            var customer = await _context.Customer.FindAsync(id);
            if (customer == null)
            {
                return new ApiResult<Customer>(ApiCode.NotFoundCustomer, "This customer was not found", null);
            }
            customer.Deleted = true;
            _context.Customer.Update(customer);
            await _context.SaveChangesAsync();
            return new ApiResult<Customer>(ApiCode.Success, "Ok", customer);
        }
    }
}
