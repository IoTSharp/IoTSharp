using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IoTSharp.Data;
using IoTSharp.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using IoTSharp.Extensions;

namespace IoTSharp.Controllers
{

    /// <summary>
    /// this is test purpose
    /// </summary>

    [Route("api/[controller]")]
    [ApiController]
    public class MenuController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ApplicationDbContext _context;
        public MenuController(UserManager<IdentityUser> userManager, ApplicationDbContext context)
        {
            this._userManager = userManager;
            this._context = context;
        }



        public async Task<AppMessage> GetUserAsset(int type)
        {
      

            var profile = await this.GetUserProfile();
            switch (type)
            {
                case 1:
                    if (profile.Roles.Contains("SystemAdmin"))
                    {
                        return new AppMessage()
                        {
                            ErrType = ErrType.正常返回, Result = new[] {""}
                        };
                    }
                    if (profile.Roles.Contains("SystemAdmin"))
                    {
                        return new AppMessage()
                        {
                            ErrType = ErrType.正常返回,
                            Result = new[] { "" }
                        };
                    }
                    if (profile.Roles.Contains("SystemAdmin"))
                    {
                        return new AppMessage()
                        {
                            ErrType = ErrType.正常返回,
                            Result = new[] { "" }
                        };
                    }
                    if (profile.Roles.Contains("SystemAdmin"))
                    {
                        return new AppMessage()
                        {
                            ErrType = ErrType.正常返回,
                            Result = new[] { "" }
                        };
                    }
                    if (profile.Roles.Contains("SystemAdmin"))
                    {
                        return new AppMessage()
                        {
                            ErrType = ErrType.正常返回,
                            Result = new[] { "" }
                        };
                    }




                    break;

            }



            return new AppMessage();
        }



        [Authorize]
        [HttpGet("[action]")]
        public async Task<AppMessage> GetProfile()
        {
         
            var profile = await this.GetUserProfile();
            if (profile.Roles.FirstOrDefault()?.Contains("SystemAdmin")??false)
            {
                return new AppMessage()
                {
                    Result = new
                    {
                        menu = new[]
                    {
                        new
                        {
                            text = "主导航",i18n="menu.main",group=true ,hideInBreadcrumb=true,
                            children=new[]
                            {
                               new
                               {
                                   text = "仪表盘",
                                   i18n="menu.dashboard",
                                   icon="anticon-dashboard",
                                   children=new[]
                                   {

                                       new { text = "仪表盘", i18n = "",link="/dashboard/v1" }
                                   }
                               },

                               new
                               {
                               text = "租户管理",
                               i18n="",
                               icon="anticon-rocket",
                               children=new[]
                               {

                                   new { text = "租户列表", i18n = "",link="/iot/tenant/tenantlist" }
                               }
                               },

                               //new
                               //{
                               //    text = "客户管理",
                               //    i18n="",
                               //    icon="anticon-rocket",
                               //    children=new[]
                               //    {

                               //        new { text = "租户列表", i18n = "",link="/iot/customer/customerlist" }
                               //    }
                               //},
                               //new
                               //{
                               //    text = "用户管理",
                               //    i18n="",
                               //    icon="anticon-rocket",
                               //    children=new[]
                               //    {

                               //        new { text = "用户列表", i18n = "",link="/iot/user/userlist" }
                               //    }
                               //},
                               //new
                               //{
                               //    text = "设备管理",
                               //    i18n="",
                               //    icon="anticon-rocket",
                               //    children=new[]
                               //    {

                               //        new { text = "设备列表", i18n = "",link="/iot/device/devicelist" },
                               //        new { text = "设计", i18n = "",link="/iot/device/devicegraph" },
                               //        new { text = "规则链", i18n = "",link="/iot/flow/flowlist" },
                               //    }
                               //},
                               new
                               {
                                   text = "资源",
                                   i18n="",
                                   icon="anticon-rocket",
                                   children=new[]
                                   {

                                       new { text = "字典分组", i18n = "",link="/iot/dictionary/dictionarygrouplist" },
                                       new { text = "字典", i18n = "",link="/iot/dictionary/dictionarylist" },
                                       new { text = "国际化", i18n = "",link="/iot/resouce/i18nlist" },
                                       new { text = "表单", i18n = "",link="/iot/util/dynamicformlist" },
                                   }
                               },

                            }
                        }
                    },
                        funcs = Enumerable.Range(0, 500),
                        username= profile.Name,
                        AppName="IOTSHARP",
                        Email= profile.Email.FirstOrDefault(),
                        Logo=""
                    }
                };
            }

            if (profile.Roles.FirstOrDefault() ? .Contains("TenantAdmin")??false)
            {
                return new AppMessage()
                {
                    Result = new
                    {
                        menu = new[]
                        {
                            new
                            {
                                text = "主导航",i18n="menu.main",group=true ,hideInBreadcrumb=true,
                                children=new[]
                                {
                                    new
                                    {
                                        text = "仪表盘",i18n="menu.dashboard",icon="anticon-dashboard",
                                        children=new[]
                                        {

                                            new { text = "仪表盘", i18n = "menu.dashboard.v1",link="/dashboard/v1" }
                                        }
                                    },

                                    new
                                    {
                                        text = "客户管理",i18n="",icon="anticon-rocket",
                                        children=new[]
                                        {

                                            new { text = "客户列表", i18n = "",link="/iot/customer/customerlist" }
                                        }
                                    },
                                    new
                                    {
                                        text = "用户管理",i18n="",icon="anticon-rocket",
                                        children=new[]
                                        {

                                            new { text = "用户列表", i18n = "menu.customer.userlist",link="/iot/user/userlist" }
                                        }
                                    },

                                }
                            }
                        },
                        funcs = Enumerable.Range(0, 500),
                        username = profile.Name,
                        AppName = "IOTSHARP",
                        Email = profile.Email.FirstOrDefault(),
                        Logo = ""
                    }
                };
            }
            if (profile.Roles.FirstOrDefault() ? .Contains("CustomerAdmin")??false)
            {
                return new AppMessage()
                {
                    Result = new
                    {
                        menu = new[]
                        {
                            new
                            {
                                text = "主导航",i18n="menu.main",group=true ,hideInBreadcrumb=true,
                                children=new[]
                                {
                                    new
                                    {
                                        text = "仪表盘",i18n="menu.dashboard",icon="anticon-dashboard",
                                        children=new[]
                                        {

                                            new { text = "仪表盘", i18n = "menu.dashboard.v1",link="/dashboard/v1" }
                                        }
                                    },

                                    new
                                    {
                                        text = "设备管理",i18n="menu.tenant.devicemanage",icon="anticon-rocket",
                                        children=new[]
                                        {

                                            new { text = "设备列表", i18n = "",link="/iot/device/devicelist" },
                                            new { text = "规则链", i18n = "",link="/iot/device/devicelist" },
                                            new { text = "设计", i18n = "",link="/iot/device/devicelist" },
                                            new { text = "场景", i18n = "",link="/iot/device/devicelist" },
                                        }
                                    },
                                    new
                                    {
                                        text = "用户管理",i18n="",icon="anticon-rocket",
                                        children=new[]
                                        {

                                            new { text = "用户列表", i18n = "menu.user.customerlist",link="/iot/user/userlist" }
                                        }
                                    },

                                }
                            }
                        },
                        funcs = Enumerable.Range(0, 500),
                        username = profile.Name,
                        AppName = "IOTSHARP",
                        Email = profile.Email.FirstOrDefault(),
                        Logo = ""
                    }
                };
            }

            if (profile.Roles.FirstOrDefault()?.Contains("NormalUser")??false)
            {
                return new AppMessage()
                {
                    Result = new
                    {
                        menu = new[]
                        {
                            new
                            {
                                text = "主导航",i18n="menu.main",group=true ,hideInBreadcrumb=true,
                                children=new[]
                                {
                                    new
                                    {
                                        text = "仪表盘",i18n="menu.dashboard",icon="anticon-dashboard",
                                        children=new[]
                                        {

                                            new { text = "仪表盘", i18n = "menu.dashboard.v1",link="/dashboard/v1" }
                                        }
                                    },

                                    new
                                    {
                                        text = "设备管理",i18n="menu.tenant.devicemanage",icon="anticon-rocket",
                                        children=new[]
                                        {

                                            new { text = "设备列表", i18n = "",link="/iot/device/devicelist" },
                                            new { text = "规则链", i18n = "",link="/iot/device/devicelist" },
                                            new { text = "设计", i18n = "",link="/iot/device/devicelist" },
                                            new { text = "场景", i18n = "",link="/iot/device/devicelist" },
                                        }
                                    },

                                }
                            }
                        },
                        funcs = Enumerable.Range(0, 500),
                        username = profile.Name,
                        AppName = "IOTSHARP",
                        Email = profile.Email.FirstOrDefault(),
                        Logo = ""
                    }
                };
            }

            return new AppMessage();
        }




        [AllowAnonymous]
        [HttpGet("[action]")]
        public JsonResult getMenuList()
        {









            return new JsonResult(new[]
            {

                new
                {
                    fullPath="/dashboard",
                    component="/@/layouts/default/index.vue",
                    meta=new
                    {
                        icon="ion:tv-outline",
                        title="routes.demo.iframe.frame", single=true

                    },
                    name="WelcomeParent",
                    alias="",
                    redirect="",
                    caseSensitive="false",

                },


            });
        }



        [AllowAnonymous]
        [HttpGet("[action]")]
        public JsonResult getPermCode()
        {

            return new JsonResult(new[]
            {
                "this","is","test"

            });
        }
    }
}
