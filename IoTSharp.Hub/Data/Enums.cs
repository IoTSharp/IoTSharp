using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IoTSharp.Hub.Data
{

    public enum KeyValueScope
    {
        ShareSide,
        ServerSide,
        ClientSide
    }

    public enum DataType
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
