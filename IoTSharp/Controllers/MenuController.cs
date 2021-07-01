using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace IoTSharp.Controllers
{

    /// <summary>
    /// this is test purpose
    /// </summary>

    [Route("api/[controller]")]
    [ApiController]
    public class MenuController : ControllerBase
    {
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
