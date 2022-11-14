using IoTSharp.Controllers.Models;
using IoTSharp.Models;
using Microsoft.EntityFrameworkCore.Query;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using IoTSharp.EasyEFQuery;

namespace IoTSharp.Extensions
{
 
    public static class QueryExtensions
    {
        public static async Task<PagedData<R>> Query<T, R,P>(this QueryDto _dto, IQueryable<T> src, Expression<Func<T, bool>> _where, Expression<Func<T,P>> func ,  Expression<Func<T, R>> conver, CancellationToken cancellationToken = default(CancellationToken)) where T : class
        {
            if (!string.IsNullOrEmpty( _dto.Name))
            {
               var  member = (MemberExpression)func.Body;
                var queries = new QueryCollection
                {
                    new Query { Name = member.Member.Name, Operator =Operators.StartWith, Value = _dto.Name },
                    new Query { Name = member.Member.Name, Operator =Operators.Contains, Value = _dto.Name },
                    new Query { Name = member.Member.Name, Operator = Operators.EndWidth, Value = _dto.Name  }
                };
                _where = _where.AndWith(queries);
            }
            var _total = await src.CountAsync(_where, cancellationToken);
            var query = src.Where(_where).Skip((_dto.Offset) * _dto.Limit)
                          .Take(_dto.Limit);
            List<R> rs = new List<R>();
            rs = await query.Select(conver).ToListAsync(cancellationToken);
            var data = new PagedData<R>
            {
                total = _total,
                rows = rs
            };
            return data;
        }
    }
}
