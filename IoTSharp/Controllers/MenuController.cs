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
    [Route("api/[controller]")]
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

        [HttpGet("[action]")]
        public ApiResult<dynamic> GetProfile()
        {
            var profile = this.GetUserProfile();
            if (User.IsInRole(nameof(UserRole.SystemAdmin)))
            {
                var data = new
                {
                    menu = new[]
                                    {
                        new
                        {
                            text = "主导航", i18n = "主导航", group = true, hideInBreadcrumb = true,
                            children = new[]
                            {
                                new
                                {
                                    text = "仪表盘",
                                    i18n = "仪表盘",
                                    icon = "anticon-dashboard",
                                    children = new[]
                                   {
                                        new { text = "仪表盘", i18n = "", link = "/dashboard/v1" }
                                    }
                                },

                                new
                                {
                                    text = "租户管理",
                                    i18n = "",
                                    icon = "anticon-cloud",
                                    children = new[]
                               {
                                        new { text = "租户列表", i18n = "", link = "/iot/tenant/tenantlist" }
                                    }
                                },

                                new
                                {
                                    text = "客户管理",
                                    i18n = "",
                                    icon = "anticon-appstore",
                                    children = new[]
                                        {
                                        new { text = "客户列表", i18n = "", link = "/iot/customer/customerlist" }
                                    }
                                },
                                new
                                {
                                    text = "用户管理",
                                    i18n = "",
                                    icon = "anticon-user",
                                    children = new[]
                                        {
                                        new { text = "用户列表", i18n = "", link = "/iot/user/userlist" }
                                    }
                                }, new
                                {
                                    text = "设备管理",
                                    i18n = "",
                                    icon = "anticon-database",
                                    children = new[]
                                        {   /*  new { text = "型号管理", i18n = "",link="/iot/devicemodel/devicemodellist" },*/
                                        new { text = "设备管理", i18n = "", link = "/iot/device/devicelist" },
                                        new { text = "网关配置器", i18n = "", link = "/iot/device/devicegraph" },
                                    },
                                },
                                new
                                {
                                    text = "规则链 ",
                                    i18n = "",
                                    icon = "anticon-fork",
                                    children = new[]
                                   {
                                        //new { text = "设备列表", i18n = "", link = "/iot/device/devicelist" },
                                        new { text = "设计器", i18n = "", link = "/iot/flow/flowlist" },
                                        new { text = "脚本管理", i18n = "", link = "/iot/flow/scriptlist" },
                                        new { text = "组件管理", i18n = "", link = "/iot/flow/componentlist" },
                                        new { text = "事件", i18n = "", link = "/iot/flow/flowevents" },
                                        new { text = "执行器", i18n = "", link = "/iot/flow/taskexecutorlist" },
                                        new { text = "订阅", i18n = "", link = "/iot/flow/subscriptionlist" },
                                    }
                                },

                                //<i nz-icon nzType="partition" nzTheme="outline"></i>
                                new
                                {
                                    text = "设置",
                                    i18n = "",
                                    icon = "anticon-setting",
                                    children = new[]
                                   {
                                        new { text = "字典分组", i18n = "", link = "/iot/dictionary/dictionarygrouplist" },
                                        new { text = "字典", i18n = "", link = "/iot/dictionary/dictionarylist" },
                                        new { text = "国际化", i18n = "", link = "/iot/resouce/i18nlist" },
                                        new { text = "表单", i18n = "", link = "/iot/util/dynamicformlist" },
                                        new { text = "证书管理", i18n = "", link = "/iot/settings/certmgr" },
                                    }
                                },
                            }
                        }
                    },
                    funcs = Enumerable.Range(0, 500),
                    username = profile.Name,
                    AppName = "iotsharp",
                    Modules = new[]
                                    {
                        "kanban",
                        "statistics",
                        "lists",
                        //"warning"
                    }, // 用户首页模块
                    Email = this.User.GetEmail(),
                    Comstomer = User.GetCustomerId(),
                    Tenant = User.GetTenantId(),
                    Logo = ""
                };
                return new ApiResult<dynamic>(ApiCode.Success, "OK", data);
            }
            else if (User.IsInRole(nameof(UserRole.TenantAdmin)))
            {
                return new ApiResult<dynamic>(ApiCode.Success, "OK", new
                {
                    menu = new[]
                  {
                            new
                            {
                                text = "主导航", i18n = "主导航", group = true, hideInBreadcrumb = true,
                                children = new[]
                            {
                                    new
                                    {
                                        text = "仪表盘",
                                        i18n = "仪表盘",
                                        icon = "anticon-dashboard",
                                        children = new[]
                                   {
                                            new { text = "仪表盘", i18n = "", link = "/dashboard/v1" }
                                        }
                                    },

                                    new
                                    {
                                        text = "租户管理",
                                        i18n = "",
                                        icon = "anticon-cloud",
                                        children = new[]
                               {
                                            new { text = "租户列表", i18n = "", link = "/iot/tenant/tenantlist" }
                                        }
                                    },

                                    new
                                    {
                                        text = "客户管理",
                                        i18n="",
                                        icon="anticon-appstore",
                                        children=new[]
                                        {
                                            new { text = "客户列表", i18n = "",link="/iot/customer/customerlist" }
                                        }
                                    },
                                    new
                                    {
                                        text = "用户管理",
                                        i18n="",
                                        icon="anticon-user",
                                        children=new[]
                                        {
                                            new { text = "用户列表", i18n = "",link="/iot/user/userlist" }
                                        }
                                    },   new
                                    {
                                        text = "设备管理",
                                        i18n="",
                                        icon="anticon-database",
                                        children=new[]
                                        {   /*  new { text = "型号管理", i18n = "",link="/iot/devicemodel/devicemodellist" },*/
                                            new { text = "设备管理", i18n = "",link="/iot/device/devicelist" },
                                            new { text = "网关配置器", i18n = "", link = "/iot/device/devicegraph" },
                                        },
                                    },
                                    new
                                    {
                                        text = "规则链 ",
                                        i18n = "",
                                        icon = "anticon-fork",
                                        children = new[]
                                   {
                                            //new { text = "设备列表", i18n = "", link = "/iot/device/devicelist" },
                                            new { text = "设计器", i18n = "", link = "/iot/flow/flowlist" },
                                            new { text = "脚本管理", i18n = "", link = "/iot/flow/scriptlist" },
                                            new { text = "组件管理", i18n = "", link = "/iot/flow/componentlist" },
                                            new { text = "事件", i18n = "", link = "/iot/flow/flowevents" },
                                            new { text = "执行器", i18n = "", link = "/iot/flow/taskexecutorlist" },
                                            new { text = "订阅", i18n = "", link = "/iot/flow/subscriptionlist" },
                                        }
                                    },
                                }
                            }
                        },
                    funcs = Enumerable.Range(0, 500),
                    username = profile.Name,
                    AppName = "iotsharp",
                    Modules = new[]
                  {
                        "kanban",
                        "statistics",
                        "lists",
                        //"warning"
                    }, // 用户首页模块
                    Email = profile.Email.FirstOrDefault(),
                    Comstomer = profile.Comstomer,
                    Tenant = profile.Tenant,
                    Logo = ""
                }
              );
            }

            return new ApiResult<dynamic>(ApiCode.Success, "OK", null);
        }
    }
}