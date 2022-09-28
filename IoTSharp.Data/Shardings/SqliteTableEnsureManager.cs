using System;
using ShardingCore.TableExists.Abstractions;
using System.Collections.Generic;
using System.Data.Common;
using ShardingCore.Core.VirtualRoutes.TableRoutes.RouteTails.Abstractions;

namespace ShardingCore.TableExists
{
    public class SqliteTableEnsureManager : AbstractTableEnsureManager
    {
        public SqliteTableEnsureManager(IRouteTailFactory routeTailFactory) : base(routeTailFactory)
        {
        }

        public override ISet<string> DoGetExistTables(DbConnection connection, string dataSourceName)
        {
            ISet<string> result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            using (var query = connection.CreateCommand())
            {
                query.CommandText = "SELECT tbl_name FROM  sqlite_master;";
                using (var reader = query.ExecuteReader())
                {
                    while (reader.Read())
                    {
                         var   str = (string)reader["tbl_name"];
                        result.Add(str);
                    }
                }
            }
            return result;
        }

    }
}
