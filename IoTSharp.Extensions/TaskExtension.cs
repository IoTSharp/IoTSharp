using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IoTSharp.Extensions
{
    /// <summary>
    /// 并行分区执行
    /// </summary>
    public static class TaskExtension
    {

        public static Task Forget(this Task task)
        {
            return Task.CompletedTask;
        }
        /// <summary>
        ///  并行处理<paramref name="source"/>,一个并行任务中分配<paramref name="rangeSize"/>个元素给 <paramref name="action"/>
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="source">源数据</param>
        /// <param name="rangeSize">一个并行任务的最大分配数量</param>
        /// <param name="parallelOptions"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public static ParallelLoopResult ForEach<T>(IEnumerable<T> source, int rangeSize, ParallelOptions parallelOptions, Action<T> action)
        {
            return Parallel.ForEach(Partitioner.Create(0, source.Count(), Math.Min(source.Count(), rangeSize)), parallelOptions ?? new ParallelOptions(), (range, loopState) =>
            {
                for (int i = range.Item1; i < range.Item2; i++)
                {
                    action?.Invoke(source.ElementAt(i));
                }
            });
        }


   /// <summary>
   /// 并行处理<paramref name="source"/>,在任务分区处理时，每一个区同时通过<paramref name="factory"/>获取一个<typeparamref name="T1"/>对象
   /// </summary>
   /// <typeparam name="T"></typeparam>
   /// <typeparam name="T1">针对每个分区的注入对象</typeparam>
   /// <param name="source"></param>
   /// <param name="options"></param>
   /// <param name="factory"></param>
   /// <param name="action"></param>
   /// <returns></returns>
        public static ParallelLoopResult ForEach<T, T1>(IEnumerable<T> source, ParallelOptions  options,  Action<T, T1> action)
                                    => ForEach<T, Action<T, T1>>(source, options, action);
        /// <summary>
        /// 并行处理<paramref name="source"/>,分为<paramref name="_maxDegreeOfParallelism"/>个区，每一个区同时通过<paramref name="factory"/>获取一个<typeparamref name="T1"/>对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="T1"></typeparam>
        /// <param name="source"></param>
        /// <param name="_maxDegreeOfParallelism"></param>
        /// <param name="factory"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public static ParallelLoopResult ForEach<T, T1>(IEnumerable<T> source, int _maxDegreeOfParallelism,  Action<T, T1> action)
                                 => ForEach(source, new ParallelOptions() { MaxDegreeOfParallelism = _maxDegreeOfParallelism }, action);

        /// <summary>
        /// 并行处理<paramref name="source"/>,分为<paramref name="_maxDegreeOfParallelism"/>个区，
        /// 每一个区同时通过<paramref name="factory"/>获取<typeparamref name="T1"/>、<typeparamref name="T2"/>，并传递给<paramref name="action"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="source"></param>
        /// <param name="_maxDegreeOfParallelism"></param>
        /// <param name="factory"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public static ParallelLoopResult ForEach<T, T1, T2>(IEnumerable<T> source, int _maxDegreeOfParallelism,  Action<T, T1, T2> action)
                                    => ForEach(source, new ParallelOptions() { MaxDegreeOfParallelism = _maxDegreeOfParallelism }, action);
        /// <summary>
        ///  并行处理<paramref name="source"/>,分为<paramref name="_maxDegreeOfParallelism"/>个区，
        /// 每一个区同时通过<paramref name="factory"/>获取<typeparamref name="T1"/>、<typeparamref name="T2"/>、<typeparamref name="T3"/>，并传递给<paramref name="action"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <param name="source"></param>
        /// <param name="_maxDegreeOfParallelism"></param>
        /// <param name="factory"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public static ParallelLoopResult ForEach<T, T1, T2,T3>(IEnumerable<T> source,int  _maxDegreeOfParallelism  ,  Action<T, T1, T2,T3> action)
                                    => ForEach(source, new ParallelOptions() { MaxDegreeOfParallelism = _maxDegreeOfParallelism }, action);

        /// <summary>
        ///  并行处理<paramref name="source"/>,分为<paramref name="_maxDegreeOfParallelism"/>个区，
        /// 每一个区同时通过<paramref name="factory"/>获取<typeparamref name="T1"/>、<typeparamref name="T2"/>、<typeparamref name="T3"/>、<typeparamref name="T4"/>，并传递给<paramref name="action"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <param name="source"></param>
        /// <param name="_maxDegreeOfParallelism"></param>
        /// <param name="factory"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public static ParallelLoopResult ForEach<T, T1, T2,T3, T4>(IEnumerable<T> source, int _maxDegreeOfParallelism,  Action<T, T1, T2, T3,T4> action)
                               => ForEach(source, new ParallelOptions() { MaxDegreeOfParallelism = _maxDegreeOfParallelism }, action);

        public static ParallelLoopResult ForEach<T, T1, T2, T3,T4,T5>(IEnumerable<T> source, int _maxDegreeOfParallelism,  Action<T, T1, T2, T3,T4,T5> action)
                               => ForEach(source, new ParallelOptions() { MaxDegreeOfParallelism = _maxDegreeOfParallelism }, action);
        public static ParallelLoopResult ForEach<T, T1, T2, T3, T4, T5,T6>(IEnumerable<T> source, int _maxDegreeOfParallelism,  Action<T, T1, T2, T3, T4, T5,T6> action)
                       => ForEach(source, new ParallelOptions() { MaxDegreeOfParallelism = _maxDegreeOfParallelism }, action);

        public static ParallelLoopResult ForEach<T, T1, T2, T3, T4, T5,T6, T7>(IEnumerable<T> source, int _maxDegreeOfParallelism, Action<T, T1, T2, T3, T4, T5,T6, T7> action)
               => ForEach(source, new ParallelOptions() { MaxDegreeOfParallelism = _maxDegreeOfParallelism }, action);

        public static ParallelLoopResult ForEach<T, T1,T2>(IEnumerable<T> source, ParallelOptions parallelOptions, Action<T, T1, T2> action, Action<T1, T2> _init_action, Action<T1, T2> _finish_action)
        {
            return ForEach(source, parallelOptions, action, (Delegate)_init_action, _finish_action);
        }
        public static ParallelLoopResult ForEach<T, T1, T2,T3>(IEnumerable<T> source, ParallelOptions parallelOptions,  Action<T, T1, T2,T3> action, Action<T1, T2> _init_action, Action<T1, T2,T3> _finish_action)
        {
            return ForEach(source, parallelOptions, action, (Delegate)_init_action, _finish_action);
        }
        public static ParallelLoopResult ForEach<T, T1, T2, T3,T4,T5,T6>(IEnumerable<T> source, int _maxDegreeOfParallelism,  Action<T, T1, T2, T3, T4, T5, T6> action, Action<T1, T2, T3, T4, T5, T6> _init_action, Action<T1, T2, T3, T4, T5, T6> _finish_action)
        {
            return ForEach(source, new ParallelOptions() { MaxDegreeOfParallelism = _maxDegreeOfParallelism }, action, (Delegate)_init_action, _finish_action);
        }
        public static ParallelLoopResult ForEach<T, T1, T2, T3,T4,T5>(IEnumerable<T> source, int _maxDegreeOfParallelism,  Action<T, T1, T2, T3, T4, T5> action, Action<T1, T2, T3, T4, T5> _init_action, Action<T1, T2, T3, T4, T5> _finish_action)
        {
            return ForEach(source, new ParallelOptions() { MaxDegreeOfParallelism = _maxDegreeOfParallelism }, action, (Delegate)_init_action, _finish_action);
        }
        public static ParallelLoopResult ForEach<T, T1, T2, T3, T4>(IEnumerable<T> source, int _maxDegreeOfParallelism,  Action<T, T1, T2, T3, T4> action, Action<T1, T2, T3, T4> _init_action, Action<T1, T2, T3, T4> _finish_action)
        {
            return ForEach(source, new ParallelOptions() { MaxDegreeOfParallelism = _maxDegreeOfParallelism }, action, (Delegate)_init_action, _finish_action);
        }
        public static ParallelLoopResult ForEach<T, T1, T2,T3>(IEnumerable<T> source, int _maxDegreeOfParallelism,  Action<T, T1, T2,T3> action, Action<T1, T2,T3> _init_action, Action<T1, T2,T3> _finish_action)
        {
            return ForEach(source, new ParallelOptions() { MaxDegreeOfParallelism = _maxDegreeOfParallelism }, action, (Delegate)_init_action, _finish_action);
        }
        public static ParallelLoopResult ForEach<T, T1, T2>(IEnumerable<T> source, int _maxDegreeOfParallelism,  Action<T, T1, T2> action, Action<T1, T2> _init_action, Action<T1, T2> _finish_action)
        {
            return ForEach(source, new ParallelOptions() { MaxDegreeOfParallelism = _maxDegreeOfParallelism }, action, (Delegate)_init_action, _finish_action);
        }

        public static ParallelLoopResult ForEach<T, T1>(IEnumerable<T> source, ParallelOptions parallelOptions, Action<T, T1> action, Action<T1> _init_action, Action<T1> _finish_action)
        {
            return ForEach(source, parallelOptions, action, (Delegate)_init_action, _finish_action);
        }

        public static ParallelLoopResult ForEach<T, A>(IEnumerable<T> source, ParallelOptions parallelOptions, A action) where A : Delegate
        {
            return ForEach<T, A, A>(source, parallelOptions, action, null, null);
        }
        private static ParallelLoopResult ForEach<T, A, B>(IEnumerable<T> source, ParallelOptions parallelOptions, A action, B _init_action, B _finish_action) where A : Delegate, B where B : Delegate
        {
            int rangeSize = parallelOptions.MaxDegreeOfParallelism ==1? source.Count() : (source.Count() + parallelOptions.MaxDegreeOfParallelism - 1) / parallelOptions.MaxDegreeOfParallelism;
            
            return Parallel.ForEach(Partitioner.Create(0, source.Count(), Math.Min(source.Count(), rangeSize)), parallelOptions, (range, loopState) =>
            {
            
                    List<object> list = new List<object>();
                    action.GetType().GenericTypeArguments.Skip(1).ToList().ForEach(t =>
                    {
                          list.Add(Activator.CreateInstance(t));
                    });
                    _init_action?.DynamicInvoke(list.ToArray());
                    for (int i = range.Item1; i < range.Item2; i++)
                    {
                        var objs = new List<object>();
                        objs.Add(source.ElementAt(i));
                        objs.AddRange(list);
                        action.DynamicInvoke(objs.ToArray());
                    }
                    _finish_action?.DynamicInvoke(list.ToArray());
            });
        }

        /// <summary>
        /// 并行处理<paramref name="source"/>，按照最多<paramref name="_maxDegreeOfParallelism"/>个个数分配元素给<paramref name="action"/>处理
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="_maxDegreeOfParallelism">最大任务量</param>
        /// <param name="action"></param>
        /// <returns></returns>
        public static ParallelLoopResult ForEach<T>(IEnumerable<T> source, int _maxDegreeOfParallelism, Action<T> action) =>
                                   ForEach(source, (source.Count() + _maxDegreeOfParallelism - 1) / _maxDegreeOfParallelism, new ParallelOptions() { MaxDegreeOfParallelism = _maxDegreeOfParallelism }, action);
        /// <summary>
        /// 并行处理<paramref name="source"/>给<paramref name="action"/>处理
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public static ParallelLoopResult ForEach<T>(IEnumerable<T> source, Action<T> action)
        {
            ParallelOptions parallelOptions = new ParallelOptions();
            return ForEach(source, (source.Count() + parallelOptions.MaxDegreeOfParallelism - 1) / parallelOptions.MaxDegreeOfParallelism, parallelOptions, action);
        }
        public static ParallelLoopResult ForEach<T>(IEnumerable<T> source, ParallelOptions parallelOptions, Action<T> action) =>
                                    ForEach(source, (source.Count() + parallelOptions.MaxDegreeOfParallelism - 1) / parallelOptions.MaxDegreeOfParallelism, parallelOptions, action);

    }
}
