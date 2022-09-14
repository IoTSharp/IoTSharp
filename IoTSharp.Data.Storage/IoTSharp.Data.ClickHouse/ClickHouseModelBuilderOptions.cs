using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System;

namespace IoTSharp.Data.ClickHouse
{
    public class ClickHouseModelBuilderOptions : IDataBaseModelBuilderOptions
    {
        public ClickHouseModelBuilderOptions()
        {
        }

        public IInfrastructure<IServiceProvider> Infrastructure { get; set; }

        public void OnModelCreating(ModelBuilder modelBuilder)
        {
           
        }
    }
}