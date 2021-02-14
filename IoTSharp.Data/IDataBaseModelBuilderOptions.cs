using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoTSharp.Data
{
    public interface IDataBaseModelBuilderOptions
    {
        public void OnModelCreating(ModelBuilder modelBuilder);
    }
}
