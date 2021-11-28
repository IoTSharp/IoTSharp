using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System;

namespace IoTSharp.Data.InMemory
{
    public class InMemoryModelBuilderOptions : IDataBaseModelBuilderOptions
    {
        public InMemoryModelBuilderOptions()
        {
        }

        public IInfrastructure<IServiceProvider> Infrastructure { get; set; }

        public void OnModelCreating(ModelBuilder modelBuilder)
        {
           
        }
    }
}