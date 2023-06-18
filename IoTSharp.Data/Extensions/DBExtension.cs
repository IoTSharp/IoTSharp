using Microsoft.AspNetCore.Builder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using Microsoft.Extensions.Logging;

namespace IoTSharp.Data.Extensions
{
    public static class DBExtension
    {
        public static void CheckApplicationDBMigrations(this IApplicationBuilder app)
        {
            using var scope = app.ApplicationServices.CreateScope();
            var logfac = scope.ServiceProvider.GetService<ILoggerFactory>();
            var _logger = logfac.CreateLogger(MethodBase.GetCurrentMethod().Name);
            using var applicationDb = scope.ServiceProvider.GetService<ApplicationDbContext>();
            try
			{
                if (applicationDb.Database.IsRelational() && applicationDb.Database.GetPendingMigrations().Any())
                {
                    applicationDb.Database.Migrate();
                }
            }
			catch (Exception ex)
			{
                _logger.LogError(ex,$"{MethodBase.GetCurrentMethod().Name}{ex.Message}" );
				throw;
			}
        }
    }
}
