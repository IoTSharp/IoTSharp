using System;
using ShardingCore.TableExists.Abstractions;
using System.Collections.Generic;
using System.Data.Common;
using ShardingCore.Core.VirtualRoutes.TableRoutes.RouteTails.Abstractions;

namespace ShardingCore.TableExists
{
    public class PostgreSqlTableEnsureManager : AbstractTableEnsureManager
    {
        public PostgreSqlTableEnsureManager(IRouteTailFactory routeTailFactory) : base(routeTailFactory)
        {
        }
        private const string Tables = "Tables";
        private const string TABLE_NAME = "TABLE_NAME";

        public override ISet<string> DoGetExistTables(DbConnection connection, string dataSourceName)
        {
            ISet<string> result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            using (var dataTable = connection.GetSchema(Tables))
            {
                for (int i = 0; i < dataTable.Rows.Count; i++)
                {
                    result.Add(dataTable.Rows[i][TABLE_NAME].ToString());
                }
            }
            return result;
        }

    }
}
