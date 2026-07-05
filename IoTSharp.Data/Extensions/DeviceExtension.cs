using IoTSharp.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IoTSharp.Data.Extensions
{
    public static class DeviceExtension
    {

        /// <summary>
        /// When creating a device, all the things that need to be done here are done
        /// </summary>
        /// <param name="_context"></param>
        /// <param name="device"></param>
        /// <param name="prodId">归属于产品的设备都需要提前创建，因此这里只有前端API调用的时候传入值， 通过MQTT等创建的不传入值， 通过产品秘钥认证登录的 也会传入此值 。 </param>
        public static void AfterCreateDevice(this ApplicationDbContext _context, Device device, Guid? prodId = null)
        {
            if (device.Customer == null || device.Tenant == null || string.IsNullOrEmpty(device.Name))
            {
                throw new Exception($"Customer({device.Customer?.Id}) or  Tenant({device.Tenant?.Id}) or  Name({device.Name}) is null or empty!");
            }
            else
            {
                Dictionary<string, object> pairs = new Dictionary<string, object>
                {
                    { "CreateDateTime", DateTime.UtcNow }
                };
                Product prod = null;
                if (prodId != null && prodId != Guid.Empty)
                {
                    prod = _context.Products.Include(p => p.DefaultAttributes).FirstOrDefault(p => p.Id == prodId);
                    if (prod != null)
                    {
                        device.Product = prod;
                        device.DeviceType = prod.DefaultDeviceType;
                        device.Timeout = prod.DefaultTimeout;
                        prod.Devices ??= [];
                        prod.Devices.Add(device);
                        _context.PrepareNewAttributeLatest(prod.DefaultAttributes, device.Id);
                    }
                }
                _context.EnsureDeviceIdentity(device, ResolveProductDefaultIdentityType(prod), prod);
                _context.PrepareNewAttributeLatest(pairs, device.Id, DataSide.ServerSide);
            }
        }
        public static void AfterCreateDevice(this ApplicationDbContext _context, Device device, string username, string password)
        {
            if (device.Customer == null || device.Tenant == null || string.IsNullOrEmpty(device.Name))
            {
                throw new Exception($"Customer({device.Customer?.Id}) or  Tenant({device.Tenant?.Id}) or  Name({device.Name}) is null or empty!");
            }
            else
            {
                var identity = new DeviceIdentity
                {
                    Device = device,
                    IdentityType = IdentityType.DevicePassword,
                    IdentityId = username,
                    IdentityValue = password
                };
                device.DeviceIdentity = identity;
                _context.DeviceIdentities.Add(identity);
                Dictionary<string, object> pairs = new Dictionary<string, object>();
                pairs.Add("CreateDateTime", DateTime.UtcNow);
                _context.PrepareNewAttributeLatest(pairs, device.Id, DataSide.ServerSide);
            }
        }
        public static void AfterCreateDevice(this ApplicationDbContext _context, Device device, Guid prodId, string username, string password)
        {
            if (device.Customer == null || device.Tenant == null || string.IsNullOrEmpty(device.Name))
            {
                throw new Exception($"Customer({device.Customer?.Id}) or  Tenant({device.Tenant?.Id}) or  Name({device.Name}) is null or empty!");
            }
            else
            {
                var prod = _context.Products.Include(p => p.DefaultAttributes).FirstOrDefault(p => p.Id == prodId);
                if (prod != null)
                {
                    device.Product = prod;
                    device.DeviceType = prod.DefaultDeviceType;
                    device.Timeout = prod.DefaultTimeout;
                    prod.Devices ??= [];
                    prod.Devices.Add(device);
                    _context.PrepareNewAttributeLatest(prod.DefaultAttributes ?? new List<ProductData>(), device.Id);
                }

                var identity = new DeviceIdentity
                {
                    Device = device,
                    IdentityType = IdentityType.DevicePassword,
                    IdentityId = username,
                    IdentityValue = password
                };
                device.DeviceIdentity = identity;
                _context.DeviceIdentities.Add(identity);
                Dictionary<string, object> pairs = new Dictionary<string, object>();
                pairs.Add("CreateDateTime", DateTime.UtcNow);
                _context.PrepareNewAttributeLatest(pairs, device.Id, DataSide.ServerSide);
            }
        }

        public static DeviceIdentity EnsureDeviceIdentity(this ApplicationDbContext _context, Device device, IdentityType identityType, Product product = null)
        {
            if (device == null)
            {
                throw new ArgumentNullException(nameof(device));
            }

            var identity = device.DeviceIdentity ?? _context.DeviceIdentities.FirstOrDefault(c => c.DeviceId == device.Id);
            if (identity == null)
            {
                identity = new DeviceIdentity
                {
                    Device = device,
                    DeviceId = device.Id
                };
                device.DeviceIdentity = identity;
                _context.DeviceIdentities.Add(identity);
            }

            var previousIdentityType = identity.IdentityType;
            identity.IdentityType = identityType;
            switch (identityType)
            {
                case IdentityType.ProductToken:
                    product ??= device.Product;
                    if (product == null)
                    {
                        product = _context.Device
                            .Where(d => d.Id == device.Id && d.Product != null && d.Product.Deleted == false)
                            .Select(d => d.Product)
                            .FirstOrDefault();
                    }

                    if (string.IsNullOrWhiteSpace(device.Name))
                    {
                        throw new Exception("Device name is required for product token authentication.");
                    }

                    if (product == null || string.IsNullOrWhiteSpace(product.ProductToken))
                    {
                        throw new Exception("Product token is required for product token authentication.");
                    }

                    identity.IdentityId = device.Name;
                    identity.IdentityValue = product.ProductToken;
                    break;
                case IdentityType.DevicePassword:
                    if (string.IsNullOrWhiteSpace(device.Name))
                    {
                        throw new Exception("Device name is required for device password authentication.");
                    }

                    identity.IdentityId = device.Name;
                    if (string.IsNullOrWhiteSpace(identity.IdentityValue))
                    {
                        identity.IdentityValue = Guid.NewGuid().ToString().Replace("-", "");
                    }
                    break;
                case IdentityType.X509Certificate:
                    if (string.IsNullOrWhiteSpace(identity.IdentityId) || previousIdentityType != IdentityType.X509Certificate)
                    {
                        identity.IdentityId = device.Id.ToString("N");
                    }
                    break;
                case IdentityType.AccessToken:
                default:
                    if (previousIdentityType != IdentityType.AccessToken || string.IsNullOrWhiteSpace(identity.IdentityId))
                    {
                        identity.IdentityId = Guid.NewGuid().ToString().Replace("-", "");
                    }
                    identity.IdentityValue = null;
                    break;
            }

            device.DeviceIdentity = identity;
            return identity;
        }

        public static IdentityType ResolveProductDefaultIdentityType(Product product)
        {
            if (product == null)
            {
                return IdentityType.AccessToken;
            }

            return product.DefaultIdentityType == IdentityType.DevicePassword
                ? IdentityType.ProductToken
                : product.DefaultIdentityType;
        }

        private static void PrepareNewAttributeLatest(this ApplicationDbContext context, IEnumerable<ProductData> attributes, Guid deviceId)
        {
            if (attributes == null)
            {
                return;
            }

            var now = DateTime.UtcNow;
            var rows = attributes
                .Where(attribute => !string.IsNullOrWhiteSpace(attribute.KeyName))
                .Select(attribute => new AttributeLatest
                {
                    Catalog = DataCatalog.AttributeLatest,
                    DateTime = now,
                    DeviceId = deviceId,
                    KeyName = attribute.KeyName
                });
            context.AttributeLatest.AddRange(rows);
        }

        private static void PrepareNewAttributeLatest(this ApplicationDbContext context, Dictionary<string, object> data, Guid deviceId, DataSide dataSide)
        {
            if (data == null)
            {
                return;
            }

            var now = DateTime.UtcNow;
            foreach (var item in data.Where(item => item.Key != null && item.Value != null))
            {
                var row = new AttributeLatest
                {
                    Catalog = DataCatalog.AttributeLatest,
                    DateTime = now,
                    DeviceId = deviceId,
                    KeyName = item.Key,
                    DataSide = dataSide
                };
                row.FillKVToMe(item);
                context.AttributeLatest.Add(row);
            }
        }

        public static async Task<DeviceRule[]> GerDeviceRulesList(this ApplicationDbContext _dbContext, Guid devid, EventType mountType)
        {
            DeviceRule[] lst = null;
            var r = from dr in _dbContext.DeviceRules.Include(d => d.Device).Include(d => d.FlowRule) where dr.Device.Id == devid && dr.FlowRule.MountType == mountType select dr;
            if (r.Any())
            {
                lst = await r.ToArrayAsync();
            }
            return lst;
        }
        public static async Task<Guid> GerDeviceRpcRulesList(this ApplicationDbContext _dbContext, Guid devid, EventType mountType, string method)
        {
            var rules = await GerDeviceRulesList(_dbContext, devid, mountType);
            var g = (rules?.FirstOrDefault(r => r.FlowRule.Name == method)?.FlowRule.RuleId);
            return (Guid)g;
        }
        public static async Task<Guid[]> GerDeviceRulesIdList(this ApplicationDbContext _dbContext, Guid devid, EventType mountType)
        {
            var rules = await GerDeviceRulesList(_dbContext, devid, mountType);
            return rules?.Select(xc => xc.FlowRule.RuleId).ToArray();
        }



    }
}
