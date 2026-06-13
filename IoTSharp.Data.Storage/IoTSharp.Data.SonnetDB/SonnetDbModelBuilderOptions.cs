using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System;

namespace IoTSharp.Data.SonnetDB
{
    public class SonnetDbModelBuilderOptions : IDataBaseModelBuilderOptions
    {
        public IInfrastructure<IServiceProvider> Infrastructure { get; set; }

        public void OnModelCreating(ModelBuilder modelBuilder)
        {
        }
    }
}
