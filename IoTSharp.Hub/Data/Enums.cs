using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IoTSharp.Hub.Data
{
    public enum AttributeType
    {
        Boolean,
        String,
        Long,
        Double,
        Json,
        XML,
        Binary
    }
    public enum DatabaseType
    {
        mssql,
        npgsql,
        sqlite
    }
}
