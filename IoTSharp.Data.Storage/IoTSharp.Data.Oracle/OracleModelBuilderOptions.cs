using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Oracle.EntityFrameworkCore.Metadata;
using Oracle.EntityFrameworkCore.Metadata.Conventions;
using System;

namespace IoTSharp.Data.Oracle
{
    /// <summary>
    /// https://blog.csdn.net/m0_37903882/article/details/105120696
    /// </summary>
    public class OracleModelBuilderOptions : IDataBaseModelBuilderOptions
    {
        public OracleModelBuilderOptions()
        {
        }

        public IInfrastructure<IServiceProvider> Infrastructure { get; set; }

        public void OnModelCreating(ModelBuilder modelBuilder)
        {
            //fix ORA-00972: identity is too long 
            //https://docs.oracle.com/en/database/oracle/oracle-data-access-components/19.3.2/odpnt/EFCoreIdentifier.html#GUID-FA43F1A1-EDA2-462F-8844-45D49EF67607
            //Setting maximum identifier length to 30 characters; By default, it's set to 128.
            modelBuilder.Model.SetMaxIdentifierLength(30);
          
        }
    }
}