using IoTSharp.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IoTSharp.Extensions
{
    public static class DeviceExtension
    {
      
        /// <summary>
        /// When creating a device, all the things that need to be done here are done
        /// </summary>
        /// <param name="_context"></param>
        /// <param name="device"></param>
        public static void   AfterCreateDevice(this ApplicationDbContext _context, Device device)
        {
            if (device.Customer == null || device.Tenant == null || string.IsNullOrEmpty(device.Name))
            {
                throw new Exception("Customer or  Tenant or  Name is null or empty!");
            }
            else
            {
                _context.DeviceIdentities.Add(new DeviceIdentity()
                {
                    Device = device,
                    IdentityType = IdentityType.AccessToken,
                    IdentityId = Guid.NewGuid().ToString().Replace("-", "")
                });
                Dictionary<string, object> pairs = new Dictionary<string, object>();
                pairs.Add("CreateDateTime", DateTime.Now);
                _context.PreparingData<AttributeLatest>(pairs, device.Id, DataSide.ServerSide);
            }
        }
        public static void AfterCreateDevice(this ApplicationDbContext _context, Device device,string username,string password)
        {
            if (device.Customer == null || device.Tenant == null || string.IsNullOrEmpty(device.Name))
            {
                throw new Exception("Customer or  Tenant or  Name is null or empty!");
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
                pairs.Add("CreateDateTime", DateTime.Now);
                _context.PreparingData<AttributeLatest>(pairs, device.Id, DataSide.ServerSide);
            }
        }
        public static async Task<DeviceRule[]> GerDeviceRulesList(this ApplicationDbContext _dbContext, Guid devid, MountType mountType)
        {
            DeviceRule[] lst = null;
            var r = from dr in _dbContext.DeviceRules.Include(d => d.Device).Include(d => d.FlowRule) where dr.Device.Id == devid && dr.FlowRule.MountType == mountType select dr ;
            if (r.Any())
            {
                lst = await r.ToArrayAsync();
            }
            return lst;
        }
        public static async Task<Guid> GerDeviceRpcRulesList(this ApplicationDbContext _dbContext, Guid devid, MountType mountType,string method)
        {
            var rules = await GerDeviceRulesList(_dbContext, devid, mountType);
            var g = (rules.FirstOrDefault(r => r.FlowRule.Name == method)?.FlowRule.RuleId);
            return g.GetValueOrDefault();
        }
        public static async Task<Guid[]> GerDeviceRulesIdList(this ApplicationDbContext _dbContext, Guid devid, MountType mountType)
        {
            var rules =await  GerDeviceRulesList(_dbContext, devid, mountType);
            return rules?.Select(xc => xc.FlowRule.RuleId).ToArray();
        }
            


    }
}
