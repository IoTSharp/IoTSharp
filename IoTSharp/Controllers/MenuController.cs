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
    /// 提供当前用户可访问的控制台导航与基础资料。
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

        /// <summary>
        /// 根据角色返回按领域边界组织的控制台导航。
        /// </summary>
        /// <returns>包含导航、用户与应用信息的结果。</returns>
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
                    text = "运行概览",
                    i18n = "运行概览",
                    vi18n = "iot.dashboardmnt",
                    routename = "dashboard",
                    vpath = "/dashboard",
                    icon = "anticon-dashboard",
                    children = new MenuItem[]
                        {
                            new() { text = "运行概览", i18n = "", vi18n="iot.dashboard", routename="dashboard", link = "/dashboard", vpath="/dashboard", }
                        }
                }
            };
            if (User.IsInRole(nameof(UserRole.NormalUser)))
            {
                _user_menu.Add(
                new()
                {
                    text = "接入与采集",
                    i18n = "",
                    vi18n = "iot.devicemnt",
                    routename = "accessmnt",
                    vpath = "/iot/access",
                    icon = "anticon-database",
                    children = new MenuItem[]
                        {
                            new() { text = "产品与采集模板", i18n = "", vi18n="iot.productlist", routename="productlist", link = "/iot/product/productlist", vpath="/iot/product/productlist", },
                            new() { text = "资产", i18n = "", vi18n="iot.assetlist", routename="assetlist", link = "/iot/assets/assetlist", vpath="/iot/assets/assetlist",},
                            new() { text = "设备管理", i18n = "", vi18n="iot.devicelist", routename="devicelist", link = "/iot/devices/devicelist" , vpath="/iot/devices/devicelist",},
                            new() { text = "网关", i18n = "", vi18n="iot.gatewaylist", routename="gatewaylist", link = "/iot/devices/gatewaylist", vpath="/iot/devices/gatewaylist",},
                            new() { text = "Edge 节点", i18n = "", vi18n="iot.edgelist", routename="edgelist", link = "/iot/devices/edgelist" , vpath="/iot/devices/edgelist",},
                        }
                });
                _user_menu.Add(new()
                {
                    text = "实时规则",
                    i18n = "",
                    vi18n = "iot.rulesmnt",
                    routename = "rulesmnt",
                    vpath = "/iot/rules",
                    icon = "anticon-branches",
                    children = new MenuItem[]
                        {
                            new() { text = "规则链设计", i18n = "", vi18n="iot.flowlist", routename="flowlist", link = "/iot/rules/flowlist", vpath = "/iot/rules/flowlist", },
                            new() { text = "规则链审计", i18n = "", vi18n="iot.flowevents", routename="flowevents", link = "/iot/rules/flowevents", vpath = "/iot/rules/flowevents",  },
                        }
                });
                _user_menu.Add(new()
                {
                    text = "运维与发布",
                    i18n = "",
                    vi18n = "iot.operationsmnt",
                    routename = "operationsmnt",
                    icon = "anticon-deployment-unit",
                    vpath = "/iot/operations",
                    children = new MenuItem[]
                        {
                            new() { text = "Edge 任务", i18n = "", vi18n="iot.edgetasks", routename="edgetasks", link = "/iot/devices/edgetasks" , vpath="/iot/devices/edgetasks",},
                            new() { text = "发布中心", i18n = "", vi18n="iot.releasecenter", routename="releasecenter", link = "/iot/release/releaselist", vpath="/iot/release/releaselist",},
                            new() { text = "设备告警", i18n = "", vi18n="iot.alarmlist", routename="alarmlist", link = "/iot/alarms/alarmlist", vpath = "/iot/alarms/alarmlist", },
                        }
                });
            }
            if (sysmenu.Count > 0)
            {
                _user_menu.Add(new()
                {
                    text = "平台治理",
                    i18n = "",
                    vi18n = "iot.settingsmnt",
                    routename = "governancemnt",
                    vpath = "/iot/settings",
                    icon = "anticon-setting",
                    children = sysmenu.ToArray()
                });
            }
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
