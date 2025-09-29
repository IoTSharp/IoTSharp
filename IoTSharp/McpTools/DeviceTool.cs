using IoTSharp.Contracts;
using IoTSharp.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using NJsonSchema.Annotations;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
namespace IoTSharp.McpTools
{
    public record  MCPDeviceDto (string Name,Guid Id,  DeviceType DeviceType);

    [McpServerToolType]
    public sealed class DeviceTool
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DeviceTool> _logger;
    

        public DeviceTool(ApplicationDbContext context, ILogger<DeviceTool> logger , IServiceScopeFactory scopeFactor     )
        {
            _context = context;
            _logger = logger;
     

        }
        [McpServerTool, Description("Echoes the input back to the client.")]
        public   string echo(IMcpServer _server,string message)
        {
            return $"hello, you client name is :{_server.ClientInfo.Name}  version is: {_server.ClientInfo.Version}, you input message is \"{message}\".";
        }
        /// <summary>
        /// Get the list of sub-devices.
        /// </summary>
        /// <returns></returns>
        [McpServerTool(Name = "DevicesList"), Description("Get the list of  devices.")]
        public   List<MCPDeviceDto> DevicesList(IMcpServer _server)
        {
            var query = QueryByApiKey(_server);
            var cf = from c in query select new MCPDeviceDto(c.Name, c.Id, c.DeviceType);
            var data =  cf.ToList();
            return data;
        }

        private IQueryable<Device> QueryByApiKey(IMcpServer _server)
        {
            IQueryable<Device> query = null;
            var _API_KEY = _server.ServerOptions.Capabilities!.Experimental["API_KEY"].ToString();
            var ais = _context.AISettings.FirstOrDefault(a => a.MCP_API_KEY == _API_KEY);
            if (ais == null || !ais.Enable)
            {
                throw new UnauthorizedAccessException("API_KEY is not valid");
            }
            else
            {

                if (ais.Role == UserRole.CustomerAdmin)
                {
                    var qc = from c in _context.Customer.Include(c1 => c1.AISettings) where c.AISettings.Id == ais.Id select c;
                    var customer = qc.FirstOrDefault();
                    query = from c in _context.Device.Include(c => c.Customer) where c.Customer == customer select c;
                }
                else if (ais.Role == UserRole.TenantAdmin)
                {
                    var qt = from t in _context.Tenant.Include(t1 => t1.AISettings) where t.AISettings.Id == ais.Id select t;
                    var tenant = qt.FirstOrDefault();
                    query = from c in _context.Device.Include(c => c.Tenant) where c.Tenant == tenant select c;
                }
                else
                {
                    throw new ArgumentException($"The role {ais.Role} is not valid");
                }
            }
            return query;
        }

        /// <summary>
        /// Get the current connection status of the sub-device.
        /// </summary>
        /// <param name="_server"></param>
        /// <param name="deviceName"></param>
        /// <returns></returns>
        [McpServerTool, Description("Get the current connection status of the  device.")]
        public bool? GetDeviceStatus([Description("name of device")] string deviceName, IMcpServer _server)
        {
            var query = QueryByApiKey(_server);
            var f = from c in query where c.Name == deviceName select c;
            var devid = f.FirstOrDefault()?.Id;
            var al = from a in _context.AttributeLatest where devid == a.DeviceId && (a.KeyName == Constants._Active) select a;
            var result = (bool)(al.FirstOrDefault()?.Value_Boolean);
            return result;
        }

        /// <summary>
        /// Get the current value of a variable of a sub-device.
        /// </summary>
        /// <param name="deviceName"></param>
        /// <param name="_server"></param>
        /// <returns></returns>
        [McpServerTool, Description("Get  device attributes.")]
        [CanBeNull]
        public Dictionary<string, object> GetDeviceAttributes(
            [Description("name of device")] string deviceName
            , IMcpServer _server)
        {
            var query = QueryByApiKey(_server);
            var f = from c in query where c.Name == deviceName select c;
            var devid = f.FirstOrDefault()?.Id;
            var al = from a in _context.AttributeLatest where devid == a.DeviceId select a;// new KeyValuePair<string,object>( a.KeyName,a.ToObject());
            var result=al.ToDictionary(x => x.KeyName, x => x.ToObject());
            return result;
        }

        /// <summary>
        /// Get the current value of a variable of a sub-device.
        /// </summary>
        /// <param name="deviceName">设备名称</param>
        /// <param name="attributeName">属性名称</param>
        /// <param name="_server">MCP名称</param>
        /// <returns></returns>
        [McpServerTool, Description("Get the current value of a Attribute of a device.")]
        public object GetDeviceAttribute(
            [Description("name of device")] string deviceName,
            [Description("name of attribute")] string attributeName, IMcpServer _server)
        {
            var query = QueryByApiKey(_server);
            var f = from c in _context.Device where c.Name == deviceName select c;
            var devid = f.FirstOrDefault()?.Id;
            var al = from a in _context.AttributeLatest where devid == a.DeviceId   && a.KeyName== attributeName  select a;// new KeyValuePair<string,object>( a.KeyName,a.ToObject());
            return al.FirstOrDefault()?.ToObject();
        }
    }
}
