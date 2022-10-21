using IoTSharp.Contracts;
using IoTSharp.Data;
using IoTSharp.Dtos;
using IoTSharp.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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
            if (User.IsInRole(nameof(UserRole.SystemAdmin)))
            {
                var data = new
                {
                    menu = new[]
                                    {
                        new MenuItem
                        {
                            text = "主导航",
                            i18n = "主导航",
                            vi18n="iot.home",
                            routename="home",
                            group = true,
                            hideInBreadcrumb = true,
                            vpath="/dashboard/v1",
                            isAffix=true,
                            isLink=false,
                            children = new MenuItem[]
                            {
                                new()
                                {
                                    text = "仪表盘",
                                    i18n = "仪表盘",vi18n="iot.dashboardmnt",routename="dashboard",
                                    vpath="/dashboard/v1",
                                    icon = "anticon-dashboard",
                                    children = new MenuItem[]
                                   {
                                        new() { text = "仪表盘", i18n = "",vi18n="iot.dashboard",routename="dashboard", link = "/dashboard/v1",      vpath="/dashboard/v1", }
                                    }
                                },

                                new()
                                {
                                    text = "租户管理",
                                    i18n = "",vi18n="iot.tenantmnt",routename="tenantmnt",vpath = "/iot/settings/tenantlist",
                                    icon = "anticon-cloud",
                                    children = new MenuItem[]
                               {
                                        new() { text = "租户列表", i18n = "",vi18n="iot.tenantlist",routename="tenantlist", link = "/iot/settings/tenantlist" , vpath = "/iot/settings/tenantlist"}
                                    }
                                },

                                new()
                                {
                                    text = "客户管理",
                                    i18n = "",vi18n="iot.customermnt",routename="customermnt",vpath="/iot/settings/customerlist",
                                    icon = "anticon-appstore",
                                    children = new MenuItem[]
                                        {
                                        new() { text = "客户列表", i18n = "",vi18n="iot.customerlist",routename="customerlist",  link = "/iot/settings/customerlist",  vpath="/iot/settings/customerlist",  }
                                    }
                                },
                                new()
                                {
                                    text = "用户管理",
                                    i18n = "",vi18n="iot.usermnt",routename="usermnt",
                                    icon = "anticon-user",vpath="/iot/settings/userlist",
                                    children = new MenuItem[]
                                        {
                                        new() { text = "用户列表", i18n = "",vi18n="iot.userlist",routename="userlist", link = "/iot/settings/userlist" ,vpath="/iot/settings/userlist",}
                                    }
                                },

                                new()
                                {
                                    text = "产品管理",
                                    i18n = "",vi18n="iot.producemnt",routename="producemnt",
                                    icon = "medicinebox",vpath="/iot/produce/producelist",
                                    children = new MenuItem[]
                                    {
                                        new() { text = "产品列表", i18n = "",vi18n="iot.producelist",routename="producelist", link = "/iot/produce/producelist",vpath="/iot/produce/producelist", }
                                    }
                                },
                                new()
                                {
                                    text = "设备管理",
                                    i18n = "",vi18n="iot.devicemnt",routename="devicemnt",vpath="/iot/devices/devicelist",
                                    icon = "anticon-database",
                                    children = new MenuItem[]
                                        {   /*  new { text = "型号管理", i18n = "",vi18n="",link="/iot/devicemodel/devicemodellist" },*/
                                        new() { text = "设备管理", i18n = "",vi18n="iot.devicelist",routename="devicelist", link = "/iot/devices/devicelist" ,vpath="/iot/devices/devicelist",},
                                        new() { text = "网关配置器", i18n = "",vi18n="iot.devicegraph",routename="devicegraph", link = "/iot/devices/devicegraph",vpath="/iot/devices/devicegraph", },

                                    },
                                },

                                new()
                                {
                                    text = "资产管理",
                                    i18n = "",vi18n="iot.assetsmnt",routename="assetsmnt",vpath="/iot/assets/assetlist",
                                    icon = "anticon-gold",
                                    children = new MenuItem[]
                                    {   /*  new { text = "型号管理", i18n = "",vi18n="",link="/iot/devicemodel/devicemodellist" },*/
                                    
                                        new() { text = "资产列表", i18n = "",vi18n="iot.assetlist", routename="assetlist",link = "/iot/assets/assetlist" ,vpath="/iot/assets/assetlist",},

                                    },
                                }, new()
                                {
                                    text = "告警管理",
                                    i18n = "",vi18n="iot.alarmmnt",routename="alarmmnt",vpath = "/iot/alarms/alarmlist",
                                    icon = "anticon-alert",
                                    children = new MenuItem[]
                                    {   /*  new { text = "型号管理", i18n = "",vi18n="",link="/iot/devicemodel/devicemodellist" },*/
                             
                                        new() { text = "告警列表", i18n = "",vi18n="iot.alarmlist", routename="alarmlist",link = "/iot/alarms/alarmlist",vpath = "/iot/alarms/alarmlist", },
                                    },
                                },

                                new()
                                {
                                    text = "规则链 ",
                                    i18n = "",vi18n="iot.rulesmnt",routename="rulesmnt",vpath = "/iot/alarms/flowlist",
                                    icon = "anticon-fork",
                                    children = new MenuItem[]
                                   {
                                        //new { text = "设备列表", i18n = "",vi18n="", link = "/iot/device/devicelist" },
                                        new() { text = "设计器", i18n = "",vi18n="iot.flowlist",routename="flowlist", link = "/iot/rules/flowlist",vpath = "/iot/rules/flowlist", },
                                        //new { text = "脚本管理", i18n = "",vi18n="", link = "/iot/rules/scriptlist" },
                                        //new { text = "组件管理", i18n = "",vi18n="", link = "/iot/rules/componentlist" },
                                        new() { text = "事件", i18n = "",vi18n="iot.flowevents",routename="flowevents", link = "/iot/rules/flowevents",vpath = "/iot/rules/flowevents",  },
                                        //new { text = "执行器", i18n = "",vi18n="", link = "/iot/rules/taskexecutorlist" },
                                        //new { text = "订阅", i18n = "",vi18n="", link = "/iot/rules/subscriptionlist" },
                                    }
                                },

                                //<i nz-icon nzType="partition" nzTheme="outline"></i>
                                new()
                                {
                                    text = "设置",
                                    i18n = "",vi18n="iot.settingsmnt",routename="settingsmnt",vpath = "/iot/settings/certmgr",
                                    icon = "anticon-setting",
                                    children = new MenuItem[]
                                   {
                                        new() { text = "字典分组", i18n = "",vi18n="iot.dicgroupmnt",routename="dicgroupmnt", link = "/iot/settings/dictionarygrouplist",vpath = "/iot/settings/dictionarygrouplist", },
                                        new() { text = "字典", i18n = "",vi18n="iot.dicmnt",routename="dicmnt", link = "/iot/settings/dictionarylist",vpath = "/iot/settings/dictionarylist", },
                                        new() { text = "国际化", i18n = "",vi18n="iot.i18nlist",routename="i18nlist", link = "/iot/settings/i18nlist",vpath = "/iot/settings/i18nlist", },
                                        //new { text = "表单", i18n = "",vi18n="", link = "/iot/settings/dynamicformlist" },
                                        new() { text = "证书管理", i18n = "",vi18n="iot.certmnt",routename="certmnt", link = "/iot/settings/certmgr" ,vpath = "/iot/settings/certmgr",},
                                    }
                                },
                            }
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
            else if (User.IsInRole(nameof(UserRole.TenantAdmin)))
            {
                return new ApiResult<dynamic>(ApiCode.Success, "OK", new
                {
                    menu = new MenuItem[]
                  {
                            new()
                            {
                                text = "主导航",
                                i18n = "主导航",
                                vi18n="iot.home",
                                routename="home",
                                group = true,
                                hideInBreadcrumb = true,
                                vpath="",
                                isAffix=true,
                                isLink=false,
                                children = new MenuItem[]
                            {
                                    new()
                                    {
                                        text = "仪表盘",
                                        i18n = "",
                                        vi18n="iot.dashboardmnt",routename="dashboardmnt",
                                        icon = "anticon-dashboard",
                                        children = new MenuItem[]
                                   {
                                            new() { text = "仪表盘", i18n = "",vi18n="iot.dashboard",routename="dashboard", link = "/dashboard/v1" }
                                        }
                                    },

                                    new()
                                    {
                                        text = "租户管理",
                                        i18n = "",
                                        vi18n="iot.tenantmnt",routename="tenantmnt",
                                        icon = "anticon-cloud",
                                        children = new MenuItem[]
                                        {

                                            new() { text = "租户列表", i18n = "",vi18n="iot.tenantlist",routename="tenantlist", link = "/iot/settings/tenantlist" }
                                        }
                                    },

                                    new()
                                    {
                                        text = "客户管理",
                                        i18n="",vi18n="iot.customermnt",routename="customermnt",
                                        icon="anticon-appstore",
                                        children=new MenuItem[]
                                        {
                                            new() { text = "客户列表", i18n = "",vi18n="iot.customerlist",routename="customerlist",link="/iot/settings/customerlist" }
                                        }
                                    },
                                    new()
                                    {
                                        text = "用户管理",
                                        i18n="",
                                        vi18n="iot.usermnt",routename="usermnt",
                                        icon="anticon-user",
                                        children=new MenuItem[]
                                        {
                                            new()
                                            {
                                                text = "用户列表",
                                                i18n = "",
                                                vi18n="iot.userlist",routename="userlist",
                                                link="/iot/settings/userlist" }
                                        }
                                    },   new()
                                    {
                                        text = "设备管理",
                                        i18n="",vi18n="iot.devicemnt",routename="devicemnt",
                                        icon="anticon-database",
                                        children=new MenuItem[]
                                        {   /*  new { text = "型号管理", i18n = "",vi18n="",link="/iot/devicemodel/devicemodellist" },*/
                                            new() { text = "设备管理", i18n = "iot.devicelist",vi18n="",routename="devicelist",link="/iot/devices/devicelist" },
                                            new() { text = "网关配置器", i18n = "iot.devicegraph",vi18n="",routename="devicegraph", link = "/iot/devices/devicegraph" },

                                        },
                                    },  new()
                                    {
                                        text = "资产管理",
                                        i18n = "",vi18n="iot.assetsmnt",routename="assetsmnt",
                                        icon = "anticon-gold",
                                        children = new MenuItem[]
                                        {   /*  new { text = "型号管理", i18n = "",vi18n="",link="/iot/devicemodel/devicemodellist" },*/
                                    
                                            new() { text = "资产列表", i18n = "",vi18n="iot.assetlist",routename="assetlist", link = "/iot/assets/assetlist" },

                                        },
                                    }, new()
                                    {
                                        text = "告警管理",
                                        i18n = "",vi18n="iot.alarmsmnt",routename="alarmsmnt",
                                        icon = "anticon-alert",
                                        children = new MenuItem[]
                                        {   /*  new { text = "型号管理", i18n = "",vi18n="",link="/iot/devicemodel/devicemodellist" },*/
                             
                                            new() { text = "告警列表", i18n = "",vi18n="iot.alarmlist",routename="alarmlist", link = "/iot/alarms/alarmlist" },
                                        },
                                    },
                                    new()
                                    {
                                        text = "规则链 ",
                                        i18n = "",vi18n="iot.rulesmnt",routename="rulesmnt",
                                        icon = "anticon-fork",
                                        children = new MenuItem[]{
                                            new() { text = "设计器", i18n = "",vi18n="iot.flowlist",routename="flowlist", link = "/iot/rules/flowlist" },
                                            new() { text = "事件", i18n = "",vi18n="iot.flowevents",routename="flowevents", link = "/iot/rules/flowevents" },

                                        }
                                    },
                                }
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
                    Email = profile.Email.FirstOrDefault(),
                    Customer = profile.Customer,
                    Tenant = profile.Tenant,
                    Logo = ""
                }
              );
            }

            return new ApiResult<dynamic>(ApiCode.Success, "OK", null);
        }
    }
}