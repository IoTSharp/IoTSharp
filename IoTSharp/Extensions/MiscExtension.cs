using IoTSharp.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.WindowsServices;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace IoTSharp.Extensions
{
    public static class MiscExtension
    {
        public static Task Forget(this Task task)
        {
            return Task.CompletedTask;
        }
        public static BadRequestObjectResult ExceptionRequest( this ControllerBase @base,Exception exception)
        {
            MethodBase mb = new StackTrace(exception).GetFrame(0).GetMethod();
            MethodBase cu = new StackTrace(true).GetFrame(0).GetMethod();
            return @base.BadRequest(new
            {
                code = ApiCode.Exception,
                msg = exception.Message,
                data = new
                {
                    ExceptionMethod = mb.DeclaringType.FullName + "." + mb.Name,
                    MethodName = cu.Name
                }
            });
        }
      
    }
}