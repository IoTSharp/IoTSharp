using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using IoTSharp.Contracts;
using IoTSharp.Controllers.Models;
using IoTSharp.Data;
using IoTSharp.Data.Extensions;
using IoTSharp.Dtos;
using IoTSharp.EventBus;
using IoTSharp.Extensions;
using IoTSharp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Logging;
using Microsoft.Scripting.Utils;
using ShardingCore.Extensions;

namespace IoTSharp.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {

        private readonly ApplicationDbContext _context;
        private readonly ILogger _logger;
        private readonly IPublisher _queue;

        public ProductsController(ApplicationDbContext context, ILogger<ProductsController> logger, IPublisher queue)
        {
            _context = context;
            _logger = logger;
            _queue = queue;
        }

        /// <summary>
        /// 产品列表
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<ApiResult<PagedData<ProductDto>>> List([FromQuery] QueryDto m)
        {
            var profile = this.GetUserProfile();
            m.Limit = m.Limit < 5 ? 5 : m.Limit;
            var query = _context.Products
                .Where(x => x.Customer.Id == profile.Customer && x.Tenant.Id == profile.Tenant && !x.Deleted);
            if (!string.IsNullOrEmpty(m.Name))
            {
                query = query.Where(x => x.Name.StartsWith(m.Name) || x.Name.Contains(m.Name) || x.Name.EndsWith(m.Name));
            }

            var total = await query.CountAsync();
            var rows = await query
                .OrderBy(x => x.Name)
                .Skip(m.Offset * m.Limit)
                .Take(m.Limit)
                .Select(c => new ProductDto
                {
                    Id = c.Id,
                    DefaultIdentityType = c.DefaultIdentityType,
                    DefaultTimeout = c.DefaultTimeout,
                    Description = c.Description,
                    Name = c.Name,
                    DefaultDeviceType = c.DefaultDeviceType,
                    Devices = new List<Device>()
                })
                .ToListAsync();

            if (rows.Count > 0)
            {
                foreach (var row in rows)
                {
                    var ProductId = row.Id;
                    row.Devices = await _context.Device
                        .Where(d => !d.Deleted && d.Product.Id == ProductId)
                        .ToListAsync();
                }
            }

            var data = new PagedData<ProductDto>
            {
                total = total,
                rows = rows
            };
            return new ApiResult<PagedData<ProductDto>>(ApiCode.Success, "OK", data);
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="ProductId"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<ApiResult<List<ProductData>>> ProductDatas(Guid ProductId)
        {
            var result = await _context.ProductDatas.Include(c => c.Owner).Where(p => p.Owner.Id == ProductId)
                .ToListAsync();
            return new ApiResult<List<ProductData>>(ApiCode.Success, "OK", result);
        }

        /// <summary>
        /// 获取产品
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>

        [HttpGet]
        public async Task<ApiResult<ProductAddDto>> Get(Guid id)
        {
            var result = await _context.Products.SingleOrDefaultAsync(c => c.Id == id && c.Deleted == false);
            if (result != null)
            {
                return new ApiResult<ProductAddDto>(ApiCode.Success, "OK", new ProductAddDto
                {
                    Id = result.Id,
                    Icon = result.Icon,
                    DefaultDeviceType = result.DefaultDeviceType,
                    DefaultIdentityType = result.DefaultIdentityType,
                    DefaultTimeout = result.DefaultTimeout,
                    Description = result.Description,
                    ProductToken = result.ProductToken,
                    GatewayConfiguration = result.GatewayConfiguration,
                    GatewayType = result.GatewayType,
                    Name = result.Name

                });
            }
            else
            {
                return new ApiResult<ProductAddDto>(ApiCode.CantFindObject, "Product is  not found", null);
            }

        }

        /// <summary>
        /// 删除产品
        /// </summary>
        /// <param name="ProductId">产品ID</param>
        /// <returns></returns>

        [HttpGet]
        public async Task<ApiResult<bool>> Delete(Guid ProductId)
        {

            try
            {
                var Product = await _context.Products.FindAsync(ProductId);
                if (Product != null)
                {
                    Product.Deleted = true;
                    _context.Products.Update(Product);
                    await _context.SaveChangesAsync();
                    return new ApiResult<bool>(ApiCode.Success, "OK", true);
                }

                return new ApiResult<bool>(ApiCode.CantFindObject, "Product is  not found", false);
            }
            catch (Exception e)
            {
                return new ApiResult<bool>(ApiCode.Exception, e.Message, false);
            }

        }

        /// <summary>
        /// 新增产品
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ApiResult<bool>> Save(ProductAddDto dto)
        {

            try
            {
                var profile = this.GetUserProfile();
                var Product = new Product();
                Product.Customer = _context.Customer.Find(profile.Customer);
                Product.Tenant = _context.Tenant.Find(profile.Tenant);
                Product.DefaultDeviceType = dto.DefaultDeviceType;
                Product.DefaultIdentityType = dto.DefaultIdentityType;
                Product.DefaultTimeout = dto.DefaultTimeout;
                Product.Description = dto.Description;
                Product.Name =
                    dto.Name;
                Product.GatewayConfiguration = dto.GatewayConfiguration;
                Product.Icon = dto.Icon;
                Product.Deleted = false;
                Product.GatewayType = dto.GatewayType;
                Product.ProductToken = NormalizeProductToken(dto.ProductToken);
                if (string.IsNullOrEmpty(Product.ProductToken))
                {
                    Product.ProductToken = NewProductToken();
                }
                if (await ProductTokenExists(Product.ProductToken))
                {
                    return new ApiResult<bool>(ApiCode.InValidData, "Product token already exists", false);
                }
                _context.Products.Add(Product);
                await _context.SaveChangesAsync();
                return new ApiResult<bool>(ApiCode.Success, "OK", true);
            }
            catch (Exception e)
            {
                return new ApiResult<bool>(ApiCode.Exception, e.Message, false);
            }

        }

        /// <summary>
        /// 修改产品
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPut]
        public async Task<ApiResult<bool>> Update(ProductAddDto dto)
        {

            try
            {
                var Product = await _context.Products.SingleOrDefaultAsync(c => c.Id == dto.Id && c.Deleted == false);
                if (Product != null)
                {
                    Product.DefaultIdentityType = dto.DefaultIdentityType;
                    Product.DefaultDeviceType = dto.DefaultDeviceType;
                    Product.DefaultTimeout = dto.DefaultTimeout;
                    Product.Description = dto.Description;
                    Product.GatewayType = dto.GatewayType;
                    Product.GatewayConfiguration = dto.GatewayConfiguration;
                    Product.Name = dto.Name;
                    Product.Icon = dto.Icon;
                    var ProductToken = NormalizeProductToken(dto.ProductToken);
                    if (!string.IsNullOrEmpty(ProductToken))
                    {
                        if (await ProductTokenExists(ProductToken, Product.Id))
                        {
                            return new ApiResult<bool>(ApiCode.InValidData, "Product token already exists", false);
                        }
                        Product.ProductToken = ProductToken;
                    }
                    await SyncProductDeviceIdentitiesAsync(Product);
                    _context.Products.Update(Product);
                    await _context.SaveChangesAsync();


                    return new ApiResult<bool>(ApiCode.Success, "Ok", true);
                }

                return new ApiResult<bool>(ApiCode.CantFindObject, "Product is  not found", false);
            }
            catch (Exception e)
            {
                return new ApiResult<bool>(ApiCode.Exception, e.Message, false);
            }

        }

        /// <summary>
        /// 获取产品属性
        /// </summary>
        /// <param name="ProductId">产品ID</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<ApiResult<List<ProductDataItemDto>>> GetProductData(Guid ProductId)
        {
            var Product = await _context.Products.Include(c => c.DefaultAttributes)
                .SingleOrDefaultAsync(c => c.Id == ProductId && c.Deleted == false);
            if (Product != null)
            {
                var result = Product.DefaultAttributes.Select(c =>
                    new ProductDataItemDto
                    { KeyName = c.KeyName, DataSide = c.DataSide, Type = c.Type }).ToList();
                return new ApiResult<List<ProductDataItemDto>>(ApiCode.Success, "Ok", result);
            }

            return new ApiResult<List<ProductDataItemDto>>(ApiCode.CantFindObject, "Product is  not found", null);
        }




        /// <summary>
        /// 获取产品命令定义。
        /// </summary>
        /// <param name="ProductId">产品ID。</param>
        /// <returns></returns>
        [Authorize(Roles = nameof(UserRole.NormalUser))]
        [HttpGet]
        public async Task<ApiResult<List<ProductCommandDto>>> GetProductCommands(Guid ProductId)
        {
            if (ProductId == Guid.Empty)
            {
                return new ApiResult<List<ProductCommandDto>>(ApiCode.NotFoundProduct, "Product not found", new List<ProductCommandDto>());
            }

            var product = await FindCurrentProductAsync(ProductId);
            if (product == null)
            {
                return new ApiResult<List<ProductCommandDto>>(ApiCode.NotFoundProduct, "Product not found", new List<ProductCommandDto>());
            }

            var commands = await ListProductCommandsAsync(ProductId);
            return new ApiResult<List<ProductCommandDto>>(ApiCode.Success, "Ok", commands);
        }

        /// <summary>
        /// 根据设备所属 Product 获取命令定义。
        /// </summary>
        /// <param name="id">设备ID。</param>
        /// <returns></returns>
        [Authorize(Roles = nameof(UserRole.NormalUser))]
        [HttpGet]
        public async Task<ApiResult<List<ProductCommandDto>>> GetProductCommandsByDevice(Guid id)
        {
            var profile = this.GetUserProfile();
            var productId = await _context.Device
                .Where(d => d.Id == id
                    && !d.Deleted
                    && d.TenantId == profile.Tenant
                    && d.CustomerId == profile.Customer
                    && d.Product != null
                    && !d.Product.Deleted)
                .Select(d => d.Product.Id)
                .FirstOrDefaultAsync();

            if (productId == Guid.Empty)
            {
                return new ApiResult<List<ProductCommandDto>>(ApiCode.NotFoundDevice, "Device or product not found", new List<ProductCommandDto>());
            }

            var commands = await ListProductCommandsAsync(productId);
            return new ApiResult<List<ProductCommandDto>>(ApiCode.Success, "Ok", commands);
        }

        /// <summary>
        /// 获取单个产品命令定义。
        /// </summary>
        /// <param name="id">命令定义ID。</param>
        /// <returns></returns>
        [Authorize(Roles = nameof(UserRole.NormalUser))]
        [HttpGet]
        public async Task<ApiResult<ProductCommandDto>> GetProductCommand(Guid id)
        {
            var command = await _context.ProductCommands.SingleOrDefaultAsync(c => c.CommandId == id && c.CommandStatus > -1);
            if (command == null)
            {
                return new ApiResult<ProductCommandDto>(ApiCode.CantFindObject, "Command not found", null);
            }

            var product = await FindCurrentProductAsync(command.ProductId);
            if (product == null)
            {
                return new ApiResult<ProductCommandDto>(ApiCode.NotFoundProduct, "Product not found", null);
            }

            return new ApiResult<ProductCommandDto>(ApiCode.Success, "Ok", ToProductCommandDto(command));
        }

        /// <summary>
        /// 新增产品命令定义。
        /// </summary>
        /// <param name="dto">产品命令定义。</param>
        /// <returns></returns>
        [Authorize(Roles = nameof(UserRole.CustomerAdmin))]
        [HttpPost]
        public async Task<ApiResult<bool>> SaveProductCommand([FromBody] ProductCommandDto dto)
        {
            if (dto == null || dto.ProductId == Guid.Empty)
            {
                return new ApiResult<bool>(ApiCode.InValidData, "Product command payload is invalid", false);
            }

            var product = await FindCurrentProductAsync(dto.ProductId);
            if (product == null)
            {
                return new ApiResult<bool>(ApiCode.NotFoundProduct, "Product not found", false);
            }

            var profile = this.GetUserProfile();
            var command = new ProductCommand
            {
                CommandId = dto.CommandId == Guid.Empty ? Guid.NewGuid() : dto.CommandId,
                CommandTitle = dto.CommandTitle,
                CommandType = dto.CommandType,
                CommandParams = dto.CommandParams,
                CommandName = dto.CommandName,
                CommandTemplate = dto.CommandTemplate,
                ProductId = dto.ProductId,
                CreateDateTime = DateTime.UtcNow,
                Creator = profile.Id,
                CommandStatus = 0
            };

            _context.ProductCommands.Add(command);
            await _context.SaveChangesAsync();
            return new ApiResult<bool>(ApiCode.Success, "Ok", true);
        }

        /// <summary>
        /// 修改产品命令定义。
        /// </summary>
        /// <param name="dto">产品命令定义。</param>
        /// <returns></returns>
        [Authorize(Roles = nameof(UserRole.CustomerAdmin))]
        [HttpPost]
        public async Task<ApiResult<bool>> UpdateProductCommand([FromBody] ProductCommandDto dto)
        {
            if (dto == null || dto.CommandId == Guid.Empty || dto.ProductId == Guid.Empty)
            {
                return new ApiResult<bool>(ApiCode.InValidData, "Product command payload is invalid", false);
            }

            var product = await FindCurrentProductAsync(dto.ProductId);
            if (product == null)
            {
                return new ApiResult<bool>(ApiCode.NotFoundProduct, "Product not found", false);
            }

            var command = await _context.ProductCommands
                .SingleOrDefaultAsync(c => c.CommandId == dto.CommandId
                    && c.ProductId == dto.ProductId
                    && c.CommandStatus > -1);
            if (command == null)
            {
                return new ApiResult<bool>(ApiCode.CantFindObject, "Command not found", false);
            }

            command.CommandTitle = dto.CommandTitle;
            command.CommandType = dto.CommandType;
            command.CommandParams = dto.CommandParams;
            command.CommandName = dto.CommandName;
            command.CommandTemplate = dto.CommandTemplate;
            _context.ProductCommands.Update(command);
            await _context.SaveChangesAsync();
            return new ApiResult<bool>(ApiCode.Success, "Ok", true);
        }

        /// <summary>
        /// 删除产品命令定义。
        /// </summary>
        /// <param name="id">命令定义ID。</param>
        /// <returns></returns>
        [Authorize(Roles = nameof(UserRole.CustomerAdmin))]
        [HttpGet]
        public async Task<ApiResult<bool>> DeleteProductCommand(Guid id)
        {
            var command = await _context.ProductCommands.SingleOrDefaultAsync(c => c.CommandId == id && c.CommandStatus > -1);
            if (command == null)
            {
                return new ApiResult<bool>(ApiCode.CantFindObject, "Command not found", false);
            }

            var product = await FindCurrentProductAsync(command.ProductId);
            if (product == null)
            {
                return new ApiResult<bool>(ApiCode.NotFoundProduct, "Product not found", false);
            }

            command.CommandStatus = -1;
            _context.ProductCommands.Update(command);
            await _context.SaveChangesAsync();
            return new ApiResult<bool>(ApiCode.Success, "Ok", true);
        }

        /// <summary>
        /// 查询产品命令定义。
        /// </summary>
        /// <param name="ProductId">产品ID。</param>
        /// <returns></returns>
        private async Task<List<ProductCommandDto>> ListProductCommandsAsync(Guid ProductId)
        {
            var commands = await _context.ProductCommands
                .Where(c => c.ProductId == ProductId && c.CommandStatus > -1)
                .OrderBy(c => c.CommandName)
                .ToListAsync();
            return commands.Select(ToProductCommandDto).ToList();
        }

        /// <summary>
        /// 查询当前用户作用域内的产品。
        /// </summary>
        /// <param name="ProductId">产品ID。</param>
        /// <returns></returns>
        private Task<Product> FindCurrentProductAsync(Guid ProductId)
        {
            var profile = this.GetUserProfile();
            return _context.Products
                .Include(p => p.Tenant)
                .Include(p => p.Customer)
                .SingleOrDefaultAsync(p => p.Id == ProductId
                    && !p.Deleted
                    && p.Tenant.Id == profile.Tenant
                    && p.Customer.Id == profile.Customer);
        }

        private static ProductCommandDto ToProductCommandDto(ProductCommand command)
        {
            return new ProductCommandDto
            {
                CommandId = command.CommandId,
                ProductId = command.ProductId,
                CommandTitle = command.CommandTitle,
                CommandType = command.CommandType,
                CommandParams = command.CommandParams,
                CommandName = command.CommandName,
                CommandTemplate = command.CommandTemplate
            };
        }

        /// <summary>
        /// 修改属性
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ApiResult<bool>> EditProductData(ProductDataEditDto dto)
        {

            try
            {
                var Product = await _context.Products.Include(c => c.DefaultAttributes)
                    .SingleOrDefaultAsync(c => c.Id == dto.ProductId && c.Deleted == false);
                if (Product != null)
                {

                    var d = _context.ProductDatas.ToList();
                    var pds = _context.ProductDatas.Include(c => c.Owner).Where(c => c.Owner.Id == dto.ProductId).ToList();
                    if (dto.ProductData != null && dto.ProductData.Length > 0)
                    {
                        var data = dto.ProductData.Select(c => new ProductData
                        {
                            KeyName = c.KeyName,
                            DataSide = c.DataSide,
                            Type = c.Type,
                            Owner = Product,
                            DateTime = DateTime.UtcNow
                        }).ToList();

                        var delete_keynames = pds.Select(c => c.KeyName.ToLower())
                            .Except(dto.ProductData.Select(c => c.KeyName.ToLower())).ToArray();
                        foreach (var item in data)
                        {
                            var pd = pds.FirstOrDefault(c => c.KeyName.ToLower() == item.KeyName.ToLower());
                            if (pd != null)
                            {
                                pd.DataSide = item.DataSide;
                                pd.Type = item.Type;
                                _context.ProductDatas.Update(pd);
                            }
                            else
                            {
                                Product.DefaultAttributes.Add(item);
                            }
                        }
                        await _context.SaveChangesAsync();
                        if (delete_keynames.Length > 0)
                        {
                            var deleteattr = pds.Join(delete_keynames, x => x.KeyName.ToLower(), y => y, (x, y) => x)
                                .ToArray();
                            _context.ProductDatas.RemoveRange(deleteattr);
                            await _context.SaveChangesAsync();

                        }
                        return new ApiResult<bool>(ApiCode.Success, "Ok", true);
                    }

                    return new ApiResult<bool>(ApiCode.CantFindObject, "Product data is  not found", false);
                }

                return new ApiResult<bool>(ApiCode.CantFindObject, "Product is  not found", false);
            }
            catch (Exception e)
            {
                return new ApiResult<bool>(ApiCode.Exception, e.Message, false);
            }

        }




        /// <summary>
        /// 获取产品字典
        /// </summary>
        /// <param name="ProductId">产品ID</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<ApiResult<List<ProductDictionary>>> GetProductDictionary(Guid ProductId)
        {
            var Product = await _context.Products.Include(c => c.Dictionaries)
                .SingleOrDefaultAsync(c => c.Id == ProductId && c.Deleted == false);
            if (Product != null)
            {


                var dic = Product.Dictionaries.Where(d => d.Deleted == false).ToList();
                return new ApiResult<List<ProductDictionary>>(ApiCode.Success, "Ok", dic);
            }

            return new ApiResult<List<ProductDictionary>>(ApiCode.CantFindObject, "Product is  not found", null);
        }


        /// <summary>
        /// 修改字典
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ApiResult<bool>> EditProductDictionary(ProductDictionaryEditDto dto)
        {
            var profile = this.GetUserProfile();
            try
            {
                var Product = await _context.Products.Include(c => c.Dictionaries)
                    .SingleOrDefaultAsync(c => c.Id == dto.ProductId && c.Deleted == false);
                if (Product != null)
                {
                    var deletedic = Product.Dictionaries.Select(c => c.Id)
                        .Except(dto.ProductDictionaryData.Where(c => c.Id != Guid.Empty).Select(c => c.Id)).ToList();

                    foreach (var item in dto.ProductDictionaryData)
                    {

                        if (item.Id == Guid.Empty)
                        {

                            var ProductDictionary = new ProductDictionary();
                            ProductDictionary.KeyName = item.KeyName;
                            ProductDictionary.DataType = item.DataType;
                            ProductDictionary.Customer = profile.Customer;
                            ProductDictionary.DefaultValue = item.DefaultValue;
                            ProductDictionary.Display = item.Display;
                            ProductDictionary.DisplayName = item.DisplayName;
                            ProductDictionary.KeyDesc = item.KeyDesc;
                            ProductDictionary.Tag = item.Tag;
                            ProductDictionary.Deleted = false;

                            ProductDictionary.UnitConvert = item.UnitConvert;
                            ProductDictionary.Unit = item.Unit;
                            ProductDictionary.UnitExpression = item.UnitExpression;
                            Product.Dictionaries.Add(ProductDictionary);
                            await _context.SaveChangesAsync();
                        }
                        else
                        {
                            var ProductDictionary =
                                await _context.ProductDictionaries.SingleOrDefaultAsync(c => c.Id == item.Id);
                            if (ProductDictionary != null)
                            {
                                ProductDictionary.KeyName = item.KeyName;
                                ProductDictionary.DataType = item.DataType;
                                ProductDictionary.Customer = profile.Customer;
                                ProductDictionary.DefaultValue = item.DefaultValue;
                                ProductDictionary.Display = item.Display;
                                ProductDictionary.DisplayName = item.DisplayName;
                                ProductDictionary.KeyDesc = item.KeyDesc;
                                ProductDictionary.Tag = item.Tag;
                                ProductDictionary.Deleted = false;
                                ProductDictionary.UnitConvert = item.UnitConvert;
                                ProductDictionary.Unit = item.Unit;
                                ProductDictionary.UnitExpression = item.UnitExpression;
                                _context.ProductDictionaries.Update(ProductDictionary);
                                await _context.SaveChangesAsync();
                            }

                        }
                    }
                    var sc = Product.Dictionaries.Where(c => deletedic.Any(p => p == c.Id));
                    sc.ForEach(c => c.Deleted = true);
                    _context.ProductDictionaries.UpdateRange(sc);
                    await _context.SaveChangesAsync();

                    return new ApiResult<bool>(ApiCode.Success, "Ok", true);
                }

                return new ApiResult<bool>(ApiCode.CantFindObject, "Product is  not found", false);
            }
            catch (Exception e)
            {
                return new ApiResult<bool>(ApiCode.Exception, e.Message, false);
            }

        }

        /// <summary>
        /// 获取产品的数据映射关系（产品字段 ↔ 设备字段）
        /// </summary>
        /// <param name="ProductId">产品ID</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<ApiResult<List<ProductDataMappingDto>>> GetDataMappings(Guid ProductId)
        {
            if (ProductId == Guid.Empty)
            {
                return new ApiResult<List<ProductDataMappingDto>>(ApiCode.NotFoundProduct, "Product not found", new List<ProductDataMappingDto>());
            }

            var productExists = await _context.Products.AnyAsync(p => p.Id == ProductId && !p.Deleted);
            if (!productExists)
            {
                return new ApiResult<List<ProductDataMappingDto>>(ApiCode.NotFoundProduct, "Product not found", new List<ProductDataMappingDto>());
            }

            var mappings = await _context.ProductDataMappings
                .Include(m => m.Product)
                .Where(m => m.Product.Id == ProductId && !m.Deleted)
                .Select(m => new ProductDataMappingDto
                {
                    Id = m.Id,
                    ProductKeyName = m.ProductKeyName,
                    DataCatalog = m.DataCatalog,
                    DeviceId = m.DeviceId,
                    DeviceKeyName = m.DeviceKeyName,
                    Description = m.Description
                })
                .ToListAsync();
            return new ApiResult<List<ProductDataMappingDto>>(ApiCode.Success, "Ok", mappings);
        }

        /// <summary>
        /// 保存产品的数据映射关系（全量替换）
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ApiResult<bool>> SaveDataMappings([FromBody] SaveProductDataMappingsDto dto)
        {
            try
            {
                var Product = await _context.Products.Include(p => p.DefaultAttributes)
                    .SingleOrDefaultAsync(p => p.Id == dto.ProductId && !p.Deleted);
                if (Product == null)
                    return new ApiResult<bool>(ApiCode.CantFindObject, "Product not found", false);

                // Remove old mappings for this Product
                var existing = await _context.ProductDataMappings
                    .Include(m => m.Product)
                    .Where(m => m.Product.Id == dto.ProductId)
                    .ToListAsync();
                _context.ProductDataMappings.RemoveRange(existing);

                // Add new mappings
                if (dto.Mappings != null)
                {
                    foreach (var item in dto.Mappings)
                    {
                        _context.ProductDataMappings.Add(new ProductDataMapping
                        {
                            Id = item.Id == Guid.Empty ? Guid.NewGuid() : item.Id,
                            Product = Product,
                            ProductKeyName = item.ProductKeyName,
                            DataCatalog = item.DataCatalog,
                            DeviceId = item.DeviceId,
                            DeviceKeyName = item.DeviceKeyName,
                            Description = item.Description,
                            Deleted = false
                        });
                    }
                }
                await _context.SaveChangesAsync();
                return new ApiResult<bool>(ApiCode.Success, "Ok", true);
            }
            catch (Exception e)
            {
                return new ApiResult<bool>(ApiCode.Exception, e.Message, false);
            }
        }

        /// <summary>
        /// HTTP方式上传遥测数据到产品。数据将按映射关系路由到对应的设备键。
        /// </summary>
        /// <param name="product_token">Product's ProductToken</param>
        /// <param name="telemetrys">Telemetry key-value pairs using product key names</param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("/api/Products/{product_token}/Telemetry")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public async Task<ActionResult<ApiResult>> ProductTelemetry(string product_token, [FromBody] Dictionary<string, object> telemetrys)
        {
            var (ok, Product) = _context.GetProductByToken(product_token);
            if (ok)
                return Ok(new ApiResult(ApiCode.CantFindObject, $"{product_token} is not a valid Product token"));
            try
            {
                var mappings = await _context.ProductDataMappings
                    .Include(m => m.Product)
                    .Where(m => m.Product.Id == Product.Id && !m.Deleted && m.DataCatalog == DataCatalog.TelemetryData)
                    .ToListAsync();

                // Group by device and route each mapped key
                var byDevice = mappings
                    .Where(m => telemetrys.ContainsKey(m.ProductKeyName))
                    .GroupBy(m => m.DeviceId);

                foreach (var grp in byDevice)
                {
                    var devicePayload = grp.ToDictionary(
                        m => m.DeviceKeyName,
                        m => telemetrys[m.ProductKeyName]);
                    await _queue.PublishActive(grp.Key, ActivityStatus.Activity);
                    await _queue.PublishTelemetryData(new PlayloadData
                    {
                        DeviceId = grp.Key,
                        MsgBody = devicePayload,
                        DataSide = DataSide.ClientSide,
                        DataCatalog = DataCatalog.TelemetryData
                    });
                }
                return Ok(new ApiResult(ApiCode.Success, "OK"));
            }
            catch (Exception ex)
            {
                return Ok(new ApiResult(ApiCode.Exception, ex.Message));
            }
        }

        /// <summary>
        /// 获取产品的属性数据（通过映射关系从关联设备聚合）
        /// </summary>
        /// <param name="product_token">Product's ProductToken</param>
        /// <param name="dataSide">Specifying data side</param>
        /// <param name="keys">Comma-separated product key names (optional filter)</param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet("/api/Products/{product_token}/Attributes/{dataSide}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public async Task<ActionResult<ApiResult>> ProductAttributes(string product_token, DataSide dataSide, string keys)
        {
            var (ok, Product) = _context.GetProductByToken(product_token);
            if (ok)
                return Ok(new ApiResult(ApiCode.CantFindObject, $"{product_token} is not a valid Product token"));
            try
            {
                var mappings = await _context.ProductDataMappings
                    .Include(m => m.Product)
                    .Where(m => m.Product.Id == Product.Id && !m.Deleted && m.DataCatalog == DataCatalog.AttributeData)
                    .ToListAsync();

                string[] keyFilter = string.IsNullOrEmpty(keys)
                    ? Array.Empty<string>()
                    : keys.Split(',', StringSplitOptions.RemoveEmptyEntries);

                // Filter by requested keys
                if (keyFilter.Length > 0)
                    mappings = mappings.Where(m => keyFilter.Contains(m.ProductKeyName)).ToList();

                // Collect device keys needed
                var deviceIds = mappings.Select(m => m.DeviceId).Distinct().ToList();
                var deviceAttrs = await _context.AttributeLatest
                    .Where(a => deviceIds.Contains(a.DeviceId) && a.DataSide == dataSide)
                    .ToListAsync();

                // Build result: map device key values back to Product key names
                var result = mappings
                    .Select(m =>
                    {
                        var attr = deviceAttrs.FirstOrDefault(a =>
                            a.DeviceId == m.DeviceId && a.KeyName == m.DeviceKeyName);
                        return new
                        {
                            ProductKeyName = m.ProductKeyName,
                            DeviceId = m.DeviceId,
                            DeviceKeyName = m.DeviceKeyName,
                            DataCatalog = m.DataCatalog,
                            DataSide = attr?.DataSide,
                            DateTime = attr?.DateTime,
                            Type = attr?.Type,
                            Value = attr?.ToObject()
                        };
                    })
                    .ToArray();
                return Ok(new ApiResult<object[]>(ApiCode.Success, "Ok", result));
            }
            catch (Exception ex)
            {
                return Ok(new ApiResult(ApiCode.Exception, ex.Message));
            }
        }

        /// <summary>
        /// HTTP方式上传属性数据到产品。数据将按映射关系路由到对应的设备键。
        /// </summary>
        /// <param name="product_token">Product's ProductToken</param>
        /// <param name="attributes">Attribute key-value pairs using product key names</param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("/api/Products/{product_token}/Attributes")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public async Task<ActionResult<ApiResult>> ProductAttributesUpload(string product_token, [FromBody] Dictionary<string, object> attributes)
        {
            var (ok, Product) = _context.GetProductByToken(product_token);
            if (ok)
                return Ok(new ApiResult(ApiCode.CantFindObject, $"{product_token} is not a valid Product token"));
            try
            {
                var mappings = await _context.ProductDataMappings
                    .Include(m => m.Product)
                    .Where(m => m.Product.Id == Product.Id && !m.Deleted && m.DataCatalog == DataCatalog.AttributeData)
                    .ToListAsync();

                // Group by device and route each mapped key
                var byDevice = mappings
                    .Where(m => attributes.ContainsKey(m.ProductKeyName))
                    .GroupBy(m => m.DeviceId);

                foreach (var grp in byDevice)
                {
                    var devicePayload = grp.ToDictionary(
                        m => m.DeviceKeyName,
                        m => attributes[m.ProductKeyName]);
                    await _queue.PublishActive(grp.Key, ActivityStatus.Activity);
                    await _queue.PublishAttributeData(new PlayloadData
                    {
                        DeviceId = grp.Key,
                        MsgBody = devicePayload,
                        DataSide = DataSide.ClientSide,
                        DataCatalog = DataCatalog.AttributeData
                    });
                }
                return Ok(new ApiResult(ApiCode.Success, "OK"));
            }
            catch (Exception ex)
            {
                return Ok(new ApiResult(ApiCode.Exception, ex.Message));
            }
        }

        private static string NormalizeProductToken(string ProductToken)
        {
            return ProductToken?.Trim();
        }

        private static string NewProductToken()
        {
            return Guid.NewGuid().ToString().Replace("-", "");
        }

        private Task<bool> ProductTokenExists(string ProductToken, Guid? exceptProductId = null)
        {
            return _context.Products.AnyAsync(p =>
                p.ProductToken == ProductToken &&
                p.Deleted == false &&
                (!exceptProductId.HasValue || p.Id != exceptProductId.Value));
        }

        private async Task SyncProductDeviceIdentitiesAsync(Product Product)
        {
            var devices = await _context.Device
                .Include(d => d.DeviceIdentity)
                .Where(d => !d.Deleted && d.Product.Id == Product.Id)
                .ToListAsync();

            foreach (var device in devices)
            {
                _context.EnsureDeviceIdentity(device, DeviceExtension.ResolveProductDefaultIdentityType(Product), Product);
            }
        }
    }
}
