using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System;

namespace IoTSharp.Data.Cassandra
{
    public class CassandraModelBuilderOptions : IDataBaseModelBuilderOptions
    {
        public CassandraModelBuilderOptions()
        {
        }

        public IInfrastructure<IServiceProvider> Infrastructure { get; set; }

        public void OnModelCreating(ModelBuilder modelBuilder)
        {
           
        }
    }
}