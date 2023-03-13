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
        public static void   AfterCreateDevice(this ApplicationDbContext _context, Device device,Guid? prodId = null)
        {
            if (device.Customer == null || device.Tenant == null || string.IsNullOrEmpty(device.Name))
            {
                throw new Exception($"Customer({device.Customer?.Id}) or  Tenant({device.Tenant?.Id}) or  Name({device.Name}) is null or empty!");
            }
            else
            {
                var di = new DeviceIdentity()
                {
                    Device = device,
                    IdentityType = IdentityType.AccessToken,
                    IdentityId = Guid.NewGuid().ToString().Replace("-", "")
                };
           
                Dictionary<string, object> pairs = new Dictionary<string, object>
                {
                    { "CreateDateTime", DateTime.UtcNow }
                };
                if (prodId != null && prodId != Guid.Empty  )
                {
                    var prod = _context.Produces.Include(p=>p.DefaultAttributes).FirstOrDefault( p=>p.Id==prodId);
                    if (prod != null)
                    {
                        prod.Devices.Add(device);
                        _context.PreparingData<AttributeLatest>(prod.DefaultAttributes, device.Id);
                        di.IdentityType = prod.DefaultIdentityType;
                        device.DeviceType = prod.DefaultDeviceType;
                        device.Timeout=prod.DefaultTimeout;
                    }
                }
                _context.DeviceIdentities.Add(di);
                _context.PreparingData<AttributeLatest>(pairs, device.Id, DataSide.ServerSide);
            }
        }
        public static void AfterCreateDevice(this ApplicationDbContext _context, Device device,string username,string password)
        {
            if (device.Customer == null || device.Tenant == null || string.IsNullOrEmpty(device.Name))
            {
                throw new Exception($"Customer({device.Customer?.Id}) or  Tenant({device.Tenant?.Id}) or  Name({device.Name}) is null or empty!");
            }
            else
            {
                _context.DeviceIdentities.Add(new DeviceIdentity()
                {
                    Device = device,
                    IdentityType = IdentityType.DevicePassword,
                    IdentityId = username,
                    IdentityValue = password
                }) ;
                Dictionary<string, object> pairs = new Dictionary<string, object>();
                pairs.Add("CreateDateTime", DateTime.UtcNow);
                _context.PreparingData<AttributeLatest>(pairs, device.Id, DataSide.ServerSide);
            }
        }
        public static async Task<DeviceRule[]> GerDeviceRulesList(this ApplicationDbContext _dbContext, Guid devid, EventType mountType)
        {
            DeviceRule[] lst = null;
            var r = from dr in _dbContext.DeviceRules.Include(d => d.Device).Include(d => d.FlowRule) where dr.Device.Id == devid && dr.FlowRule.MountType == mountType select dr ;
            if (r.Any())
            {
                lst = await r.ToArrayAsync();
            }
            return lst;
        }
        public static async Task<Guid> GerDeviceRpcRulesList(this ApplicationDbContext _dbContext, Guid devid, EventType mountType,string method)
        {
            var rules = await GerDeviceRulesList(_dbContext, devid, mountType);
            var g = (rules?.FirstOrDefault(r => r.FlowRule.Name == method)?.FlowRule.RuleId);
            return (Guid)g;
        }
        public static async Task<Guid[]> GerDeviceRulesIdList(this ApplicationDbContext _dbContext, Guid devid, EventType mountType)
        {
            var rules =await  GerDeviceRulesList(_dbContext, devid, mountType);
            return rules?.Select(xc => xc.FlowRule.RuleId).ToArray();
        }
            


    }
}
