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
                var querym = _context.Customer.Include(c=>c.Tenant).Where(c => !c.Deleted && c.Tenant.Id==m.tenantId);
                var data = await m.Query(querym, c => c.Name);
                return new ApiResult<PagedData<Customer>>(ApiCode.Success, "OK", data);
            }
            else
            {

                return new ApiResult<PagedData<Customer>>(ApiCode.NotFoundCustomer, "没有指定客户ID",new PagedData<Customer> ());
            }
           
        }

        /// <summary>
        /// 返回指定id的客户
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize(Roles = nameof(UserRole.NormalUser))]
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public async Task<ApiResult<Customer>> GetCustomer(Guid id)
        {
            var customer = await _context.Customer.FindAsync(id);
            if (customer == null)
            {
                return new ApiResult<Customer>(ApiCode.NotFoundCustomer, "This customer was not found", null);
            }
            return new ApiResult<Customer>(ApiCode.Success, "OK", customer);
        }

        /// <summary>
        /// 修改指定租户为 指定的信息
        /// </summary>
        /// <param name="id"></param>
        /// <param name="customer"></param>
        /// <returns></returns>
        [Authorize(Roles = nameof(UserRole.CustomerAdmin))]
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ApiResult<Customer>> PutCustomer(Guid id, CustomerDto customer)
        {
            if (id != customer.Id)
            {
                return new ApiResult<Customer>(ApiCode.InValidData, "InValidData", customer);
            }
            if(customer.TenantId!= Guid.Empty)
            {
                customer.Tenant = _context.Tenant.Find(customer.TenantId);
            }
            _context.Entry(customer).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();

                return new ApiResult<Customer>(ApiCode.Success, "", customer);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Customer.Any(e => e.Id == id && e.Tenant.Id == customer.TenantId))
                {
                    return new ApiResult<Customer>(ApiCode.NotFoundCustomer, "This customer was not found", customer);
                }
                else
                {
                    throw;
                }
            }
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