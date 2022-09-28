using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.ObjectPool;
using System;


namespace Microsoft.Extensions.ObjectPool
{
    internal class PooledObjectByFuncPolicy<T> : PooledObjectPolicy<T> where T : class
    {
        private readonly Func<T> _createfunc;
        private readonly Func<T, bool> _returnfunc;
        public PooledObjectByFuncPolicy(Func<T> createfunc)
        {
            _createfunc = createfunc;
            _returnfunc = t => true;
        }

        public PooledObjectByFuncPolicy(Func<T> createfunc, Func<T, bool> returnfunc)
        {
            _createfunc = createfunc;
            _returnfunc = returnfunc;
        }
        public override T Create()
        {
            return _createfunc.Invoke();
        }

        public override bool Return(T obj)
        {
            return _returnfunc.Invoke(obj);
        }
    }
    public static class ObjectPoolExtension
    {
        /// <summary>
        /// 添加类型为<typeparamref name="T"/>至对象池进行复用
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddObjectPool<T>(this IServiceCollection services) where T : class,new ()
        {
            return services.AddSingleton(s =>
            {
                var provider = s.GetRequiredService<ObjectPoolProvider>();
                return provider.Create<T>();
            });
        }
        /// <summary>
        /// 将<typeparamref name="T"/>类型添加至对象池， 并使用<paramref name="_create"/>进行创建初始化， 使用<paramref name="_returnfunc"/>归还
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="services"></param>
        /// <param name="_create"></param>
        /// <param name="_returnfunc"></param>
        /// <returns></returns>
        public static IServiceCollection AddObjectPool<T>(this IServiceCollection services, Func<T> _create, Func<T, bool> _returnfunc) where T : class
        {
            return services.AddSingleton(s =>
            {
                var provider = s.GetRequiredService<ObjectPoolProvider>();
                return provider.Create(new PooledObjectByFuncPolicy<T>(_create, _returnfunc));
            });
        }
        /// <summary>
        ///  将<typeparamref name="T"/>类型添加至对象池， 并使用<paramref name="_create"/>进行创建初始化
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="services"></param>
        /// <param name="_create"></param>
        /// <returns></returns>
        public static IServiceCollection AddObjectPool<T>(this IServiceCollection services, Func<T> _create) where T : class
        {
            return services.AddObjectPool(_create, t => true);
        }
    }
}
