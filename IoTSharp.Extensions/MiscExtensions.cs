
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Threading;
namespace IoTSharp.Extensions
{

    public static class MiscExtensions
    {
        public static string MD5Sum(this string text) => BitConverter.ToString(MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(text))).Replace("-", "");

      
    }

}

