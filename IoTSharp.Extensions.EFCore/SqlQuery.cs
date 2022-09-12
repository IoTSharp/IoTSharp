using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;

namespace IoTSharp.Extensions.EFCore
{
    public class SqlQuery<T>
    {
        private DatabaseFacade _databaseFacade;
        private string _sql; private object[] _parameters;

        public SqlQuery(DatabaseFacade databaseFacade, string sql, object[] parameters)
        {
            _databaseFacade = databaseFacade;
            _sql = sql;
            _parameters = parameters;
        }

        public Task<IList<T>> ToIListAsync()
        {
            return ExecuteReaderAsync((dbReader) => dbReader.ToIListAsync<T>());
        }

        public Task<List<T>> ToListAsync()
        {
            return ExecuteReaderAsync((dbReader) => dbReader.ToListAsync<T>());
        }

        private Task<U> ExecuteReaderAsync<U>(Func<DbDataReader, Task<U>> p)
        {
            return _databaseFacade.ExecuteReaderAsync(_sql, _parameters, p);
        }

        private U ExecuteReader<U>(Func<DbDataReader, U> p)
        {
            return _databaseFacade.ExecuteReader(_sql, _parameters, p);
        }

        public async Task<T> FirstOrDefaultAsync()
        {
            return await ExecuteReaderAsync((dbReader) => dbReader.FirstOrDefaultAsync<T>());
        }

        public async Task<T> SingleOrDefaultAsync()
        {
            return await ExecuteReaderAsync((dbReader) => dbReader.SingleOrDefaultAsync<T>());
        }

        public async Task<T> FirstAsync()
        {
            var result = await FirstOrDefaultAsync();
            if (result == null)
                throw new InvalidOperationException("Sequence contains no elements");

            return result;
        }

        public async Task<T> SingleAsync()
        {
            var result = await SingleOrDefaultAsync();
            if (result == null)
                throw new InvalidOperationException("Sequence contains no elements");

            return result;
        }

        public IList<T> ToList()
        {
            return ExecuteReader((dbReader) => dbReader.ToList<T>());
        }

        public DataTable ToDataTable()
        {
            return ExecuteReader((dbReader) => dbReader.ToDataTable());
        }

        public async Task<DataTable> ToDataTableAsync()
        {
            return await ExecuteReaderAsync((dbReader) => dbReader.ToDataTableAsync());
        }

        public T FirstOrDefault()
        {
            return ExecuteReader((dbReader) => dbReader.FirstOrDefault<T>());
        }

        public T SingleOrDefault()
        {
            return ExecuteReader((dbReader) => dbReader.SingleOrDefault<T>());
        }

        public T First()
        {
            var result = FirstOrDefault();
            if (result == null)
                throw new InvalidOperationException("Sequence contains no elements");

            return result;
        }

        public T Single()
        {
            var result = SingleOrDefault();
            if (result == null)
                throw new InvalidOperationException("Sequence contains no elements");

            return result;
        }
    }
}