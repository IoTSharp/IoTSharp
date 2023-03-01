using Microsoft.AspNetCore.Builder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;


namespace IoTSharp.Data.Extensions
{
    public static class DBExtension
    {
        public static void CheckApplicationDBMigrations(this IApplicationBuilder app)
        {
            using var scope = app.ApplicationServices.CreateScope();
            using var applicationDb = scope.ServiceProvider.GetService<ApplicationDbContext>();
            if (applicationDb.Database.IsRelational() && applicationDb.Database.GetPendingMigrations().Any())
            {
                applicationDb.Database.Migrate();
            }
        }
    }
}
