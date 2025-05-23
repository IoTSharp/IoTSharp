﻿using IoTSharp.Contracts;
using IoTSharp.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
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
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ILogger<DeviceTool> _logger;

        public DeviceTool(ApplicationDbContext context, UserManager<IdentityUser> userManager, ILogger<DeviceTool> logger,
            SignInManager<IdentityUser> signInManager)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;

        }
        /// <summary>
        /// Get the list of sub-devices.
        /// </summary>
        /// <returns></returns>
        [McpServerTool(Name = "DevicesList"), Description("Get the list of  devices.")]
        public  ApiResult<List<MCPDeviceDto>> DevicesList()
        {

            
            var data= new ApiResult<List<MCPDeviceDto>>(ApiCode.Success, "OK", new List<MCPDeviceDto>());
            var f = from c in _context.Device select new MCPDeviceDto(c.Name,c.Id,c.DeviceType) ;
            data.Data = f.ToList();
            return data;
        }

        /// <summary>
        /// Get the current connection status of the sub-device.
        /// </summary>
        /// <param name="deviceName"></param>
        /// <returns></returns>
        [McpServerTool, Description("Get the current connection status of the  device.")]
        public bool? GetDeviceStatus(
            [Description("name of device")] string deviceName
            )
        {
            bool?  result = false;
            var f = from c in _context.Device where  c.Name == deviceName select c;
            var devid = f.FirstOrDefault()?.Id;
            var al = from a in _context.AttributeLatest where devid == a.DeviceId && (a.KeyName == Constants._Active) select a;

             result = al.FirstOrDefault()?.Value_Boolean;
            return result;
        }

        /// <summary>
        /// Get the current value of a variable of a sub-device.
        /// </summary>
        /// <param name="deviceName"></param>
        /// <returns></returns>
        [McpServerTool, Description("Get  device attributes.")]
        [CanBeNull]
        public Dictionary<string, object> GetDeviceAttributes(
            [Description("name of device")] string deviceName
            )
        {
           var  result = new Dictionary<string, object>();
            var f = from c in _context.Device where c.Name == deviceName select c;
            var devid = f.FirstOrDefault()?.Id;
            var al = from a in _context.AttributeLatest where devid == a.DeviceId select a;// new KeyValuePair<string,object>( a.KeyName,a.ToObject());
            result=al.ToDictionary(x => x.KeyName, x => x.ToObject());
            return result;
        }

        /// <summary>
        /// Get the current value of a variable of a sub-device.
        /// </summary>
        /// <param name="deviceName"></param>
        /// <param name="attributeName"></param>
        /// <returns></returns>
        [McpServerTool, Description("Get the current value of a Attribute of a device.")]
        public object GetDeviceAttribute(
            [Description("name of device")] string deviceName,
            [Description("name of attribute")] string attributeName)
        {
            var f = from c in _context.Device where c.Name == deviceName select c;
            var devid = f.FirstOrDefault()?.Id;
            var al = from a in _context.AttributeLatest where devid == a.DeviceId   && a.KeyName== attributeName  select a;// new KeyValuePair<string,object>( a.KeyName,a.ToObject());
            return al.FirstOrDefault()?.ToObject();
        }
    }
}
