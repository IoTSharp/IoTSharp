using IoTSharp.Contracts;
using IoTSharp.Data;
using IoTSharp.Dtos;
using IoTSharp.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;

namespace IoTSharp.Controllers
{
    /// <summary>
    /// this is test purpose
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize]
    public class MenuController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ApplicationDbContext _context;

        public MenuController(UserManager<IdentityUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public ApiResult<dynamic> GetUserAsset(int type)
        {
            return new ApiResult<dynamic>(ApiCode.Success, "OK", null);
        }

        [HttpGet]
        public ApiResult<dynamic> GetProfile()
        {
            var profile = this.GetUserProfile();
            var sysmenu = new List<MenuItem>();
            if (User.IsInRole(nameof(UserRole.SystemAdmin)))
            {
                sysmenu.Add(new() { text = "证书管理", i18n = "", vi18n = "iot.certmnt", routename = "certmnt", link = "/iot/settings/certmgr", vpath = "/iot/settings/certmgr", });
                sysmenu.Add(new() { text = "租户列表", i18n = "", vi18n = "iot.tenantlist", routename = "tenantlist", link = "/iot/settings/tenantlist", vpath = "/iot/settings/tenantlist" });
            }
            if (User.IsInRole(nameof(UserRole.TenantAdmin)))
            {
                sysmenu.Add(new() { text = "客户列表", i18n = "", vi18n = "iot.customerlist", routename = "customerlist", link = "/iot/settings/customerlist", vpath = "/iot/settings/customerlist", });
            }
            if (User.IsInRole(nameof(UserRole.CustomerAdmin)))
            {
                sysmenu.Add(new() { text = "用户列表", i18n = "", vi18n = "iot.userlist", routename = "userlist", link = "/iot/settings/userlist", vpath = "/iot/settings/userlist", });
            }

            var _user_menu = new List<MenuItem>
            {
                new()
                {
                    text = "仪表盘",
                    i18n = "仪表盘",
                    vi18n = "iot.dashboardmnt",
                    routename = "dashboard",
                    vpath = "/dashboard",
                    icon = "anticon-dashboard",
                    children = new MenuItem[]
                        {
                            new() { text = "仪表盘", i18n = "", vi18n="iot.dashboard", routename="dashboard", link = "/dashboard",      vpath="/dashboard", }
                        }
                }
            };
            if (User.IsInRole(nameof(UserRole.NormalUser)))
            {
                _user_menu.Add(
                new()
                {
                    text = "数字孪生",
                    i18n = "",
                    vi18n = "iot.devicemnt",
                    routename = "devicemnt",
                    vpath = "/iot/devices",
                    icon = "anticon-database",
                    children = new MenuItem[]
                        {
                            new() { text = "设备管理", i18n = "", vi18n="iot.devicelist", routename="devicelist", link = "/iot/devices/devicelist" , vpath="/iot/devices/devicelist",},
                            new() { text = "设备告警", i18n = "", vi18n="iot.alarmlist", routename="alarmlist", link = "/iot/alarms/alarmlist", vpath = "/iot/alarms/alarmlist", },
                            new() { text = "规则链设计", i18n = "", vi18n="iot.flowlist", routename="flowlist", link = "/iot/rules/flowlist", vpath = "/iot/rules/flowlist", },
                            new() { text = "规则链审计", i18n = "", vi18n="iot.flowevents", routename="flowevents", link = "/iot/rules/flowevents", vpath = "/iot/rules/flowevents",  },
                          //  new() { text = "网关配置器", i18n = "", vi18n="iot.devicegraph", routename="devicegraph", link = "/iot/devices/devicegraph", vpath="/iot/devices/devicegraph", },
                        }
                });
                _user_menu.Add(new()
                {
                    text = "产品管理",
                    i18n = "",
                    vi18n = "iot.producemnt",
                    routename = "producemnt",
                    icon = "medicinebox",
                    vpath = "/iot/produce",
                    children = new MenuItem[]
                        {
                            new() { text = "产品列表", i18n = "", vi18n="iot.producelist", routename="producelist", link = "/iot/produce/producelist", vpath="/iot/produce/producelist", }
                        }
                });
                _user_menu.Add(new()
                {
                    text = "资产管理",
                    i18n = "",
                    vi18n = "iot.assetsmnt",
                    routename = "assetsmnt",
                    vpath = "/iot/assets",
                    icon = "anticon-gold",
                    children = new MenuItem[]
                        {
                            new() { text = "资产列表", i18n = "", vi18n="iot.assetlist", routename="assetlist", link = "/iot/assets/assetlist" , vpath="/iot/assets/assetlist",},
                        },
                });
            }
            _user_menu.Add(new()
            {
                text = "系统管理",
                i18n = "",
                vi18n = "iot.settingsmnt",
                routename = "settingsmnt",
                vpath = "/iot/settings",
                icon = "anticon-setting",
                children = sysmenu.ToArray()
            });
            var data = new
            {
                menu = new[]{
                        new MenuItem
                        {
                            text = "主导航",
                            i18n = "主导航",
                            vi18n="iot.home",
                            routename="home",
                            group = true,
                            hideInBreadcrumb = true,
                            vpath="/dashboard",
                            isAffix=true,
                            isLink=false,
                            children = _user_menu.ToArray()
                        }
                    },
                funcs = Enumerable.Range(0, 500),
                username = profile.Name,
                AppName = "IoTSharp",
                Modules = new[]
                                {
                        "kanban",
                        "statistics",
                        "lists",
                        //"warning"
                    }, // 用户首页模块
                Email = this.User.GetEmail(),
                Customer = User.GetCustomerId(),
                Tenant = User.GetTenantId(),
                Logo = ""
            };
            return new ApiResult<dynamic>(ApiCode.Success, "OK", data);
        }
    }
}