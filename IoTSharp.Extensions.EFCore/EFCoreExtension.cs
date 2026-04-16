using IoTSharp.Extensions.EFCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;

namespace Microsoft.EntityFrameworkCore
{
    public static class EFCoreExtension
    {
        private static IRelationalDatabaseFacadeDependencies GetFacadeDependencies(DatabaseFacade databaseFacade)
        {
            if (((IDatabaseFacadeDependenciesAccessor)databaseFacade).Dependencies is IRelationalDatabaseFacadeDependencies relationalDatabaseFacadeDependencies)
            {
                return relationalDatabaseFacadeDependencies;
            }
            throw new InvalidOperationException(RelationalStrings.RelationalNotInUse);
        }

        public static int ExecuteNonQuery(this DatabaseFacade databaseFacade, string sql, params object[] parameters)
        {
            IRelationalDatabaseFacadeDependencies facadeDependencies = GetFacadeDependencies(databaseFacade);
            IConcurrencyDetector concurrencyDetector = ((IDatabaseFacadeDependencies)facadeDependencies).ConcurrencyDetector;

            using (concurrencyDetector.EnterCriticalSection())
            {
                var rawSqlCommand = databaseFacade.Build(sql, parameters);
                return rawSqlCommand.RelationalCommand.ExecuteNonQuery(new RelationalCommandParameterObject(facadeDependencies.RelationalConnection, rawSqlCommand.ParameterValues, null, ((IDatabaseFacadeDependenciesAccessor)databaseFacade).Context, facadeDependencies.CommandLogger));
            }
        }

        public static async Task<int> ExecuteNonQueryAsync(this DatabaseFacade databaseFacade, string sql, params object[] parameters)
        {
            IRelationalDatabaseFacadeDependencies facadeDependencies = GetFacadeDependencies(databaseFacade);
            IConcurrencyDetector concurrencyDetector = ((IDatabaseFacadeDependencies)facadeDependencies).ConcurrencyDetector;
            var commandLogger = facadeDependencies.CommandLogger;
            using (concurrencyDetector.EnterCriticalSection())
            {
                var rawSqlCommand = databaseFacade.Build(sql, parameters);
                return await rawSqlCommand.RelationalCommand.ExecuteNonQueryAsync(new RelationalCommandParameterObject(facadeDependencies.RelationalConnection, rawSqlCommand.ParameterValues, null, ((IDatabaseFacadeDependenciesAccessor)databaseFacade).Context, commandLogger));
            }
        }

        internal static T ExecuteReader<T>(this DatabaseFacade databaseFacade, string sql, object[] parameters, Func<DbDataReader, T> func)
        {
            T result = default(T);
            IRelationalDatabaseFacadeDependencies facadeDependencies = GetFacadeDependencies(databaseFacade);
            IConcurrencyDetector concurrencyDetector = ((IDatabaseFacadeDependencies)facadeDependencies).ConcurrencyDetector;
            var commandLogger = (facadeDependencies).CommandLogger;
            using (concurrencyDetector.EnterCriticalSection())
            {
                var rawSqlCommand = databaseFacade.Build(sql, parameters);
                using (var reader = rawSqlCommand.RelationalCommand.ExecuteReader(new RelationalCommandParameterObject(facadeDependencies.RelationalConnection, rawSqlCommand.ParameterValues, null, ((IDatabaseFacadeDependenciesAccessor)databaseFacade).Context, commandLogger)))
                {
                    result = func.Invoke(reader.DbDataReader);
                }
            }
            return result;
        }

        internal class RAWSQLCommand
        {
            internal IRelationalCommand RelationalCommand { get; set; }
            internal IReadOnlyDictionary<string, object> ParameterValues { get; set; }
        }

        private static RAWSQLCommand Build(this DatabaseFacade databaseFacade, string sql, params object[] parameters)
        {
            var builder = GetFacadeDependencies(databaseFacade).RawSqlCommandBuilder;
            RAWSQLCommand command;
            if (parameters == null || parameters.Length == 0)
            {
                command = new RAWSQLCommand { RelationalCommand = builder.Build(sql) };
            }
            else
            {
                var cmdx = builder.Build(sql, parameters);
                command = new RAWSQLCommand() { RelationalCommand = cmdx.RelationalCommand, ParameterValues = cmdx.ParameterValues };
            }
            return command;
        }

        public static SqlQuery<T> SqlQuery<T>(this DatabaseFacade databaseFacade, string sql)
        {
            return new SqlQuery<T>(databaseFacade, sql, Array.Empty<object>());
        }

        public static SqlQuery<T> SqlQuery<T>(this DatabaseFacade databaseFacade, string sql, params object[] parameters)
        {
            return new SqlQuery<T>(databaseFacade, sql, parameters);
        }

        internal static async Task<T> ExecuteReaderAsync<T>(this DatabaseFacade databaseFacade, string sql, object[] parameters, Func<DbDataReader, Task<T>> func)
        {
            T result = default(T);
            IRelationalDatabaseFacadeDependencies facadeDependencies = GetFacadeDependencies(databaseFacade);
            IConcurrencyDetector concurrencyDetector = ((IDatabaseFacadeDependencies)facadeDependencies).ConcurrencyDetector;
            var commandLogger = facadeDependencies.CommandLogger;
            using (concurrencyDetector.EnterCriticalSection())
            {
                var rawSqlCommand = databaseFacade.Build(sql, parameters);
                using (var reader = await rawSqlCommand.RelationalCommand.ExecuteReaderAsync(new RelationalCommandParameterObject(facadeDependencies.RelationalConnection, rawSqlCommand.ParameterValues, null, ((IDatabaseFacadeDependenciesAccessor)databaseFacade).Context, commandLogger)))
                {
                    result = await func.Invoke(reader.DbDataReader);
                }
            }
            return result;
        }

        public static async Task<T> ExecuteScalarAsync<T>(this DatabaseFacade databaseFacade, string sql, params object[] parameters)
                                    => (T)await ExecuteScalarAsync(databaseFacade, sql, parameters);

        public static T ExecuteScalar<T>(this DatabaseFacade databaseFacade, string sql, params object[] parameters)
                                    => (T)ExecuteScalar(databaseFacade, sql, parameters);

        public static async Task<object> ExecuteScalarAsync(this DatabaseFacade databaseFacade, string sql, params object[] parameters)
        {
            IRelationalDatabaseFacadeDependencies facadeDependencies = GetFacadeDependencies(databaseFacade);
            IConcurrencyDetector concurrencyDetector = ((IDatabaseFacadeDependencies)facadeDependencies).ConcurrencyDetector;
            var commandLogger = facadeDependencies.CommandLogger;
            using (concurrencyDetector.EnterCriticalSection())
            {
                var rawSqlCommand = databaseFacade.Build(sql, parameters);
                return await rawSqlCommand.RelationalCommand.ExecuteScalarAsync(new RelationalCommandParameterObject(facadeDependencies.RelationalConnection, rawSqlCommand.ParameterValues, null, ((IDatabaseFacadeDependenciesAccessor)databaseFacade).Context, commandLogger));
            }
        }

        public static object ExecuteScalar(this DatabaseFacade databaseFacade, string sql, params object[] parameters)
        {
            IRelationalDatabaseFacadeDependencies facadeDependencies = GetFacadeDependencies(databaseFacade);
            IConcurrencyDetector concurrencyDetector = ((IDatabaseFacadeDependencies)facadeDependencies).ConcurrencyDetector;
            var commandLogger = facadeDependencies.CommandLogger;
            using (concurrencyDetector.EnterCriticalSection())
            {
                var rawSqlCommand = databaseFacade.Build(sql, parameters);
                return rawSqlCommand.RelationalCommand.ExecuteScalar(new RelationalCommandParameterObject(facadeDependencies.RelationalConnection, rawSqlCommand.ParameterValues, null, ((IDatabaseFacadeDependenciesAccessor)databaseFacade).Context, commandLogger));
            }
        }
    }
}