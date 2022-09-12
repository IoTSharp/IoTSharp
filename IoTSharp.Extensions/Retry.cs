using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace IoTSharp.Extensions
{
    /// <summary>
    /// 重试类
    /// </summary>
    public static class Retry
    {
        [Obsolete("已放弃，请调用T RetryOnAny<T>(int times, Func<T> action)")]
        public static T Invoke<T>(int times, Func<T> action)
        {
            return RetryOnAny(times, action);
        }
        /// <summary>
        /// 无论遇到任何错误，最多尝试<paramref name="times"/>次
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="times"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public static T RetryOnAny<T>(int times, Func<T> action)
        {
            return RetryOnAny(times, a =>
            {
                return action.Invoke();
            }, ef =>
            {
                Thread.Sleep(TimeSpan.FromSeconds(ef.current * 5));
            });
        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="times"></param>
        /// <param name="action"></param>
        /// <param name="efunc"></param>
        /// <returns></returns>
      [Obsolete("放弃，请调用RetryOnAny(int times, Func<int, T> action, Action<(int current, Exception ex)> efunc)")]
        public static T Invoke<T>(int times, Func<int, T> action, Action<(int current, Exception ex)> efunc)
        {
            return RetryOnAny(times, action, efunc);
        }
        /// <summary>
        /// 当遇到<typeparamref name="E"/>的异常时重试指定<paramref name="times"/>次，遇到其他异常则认为失败。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="E"></typeparam>
        /// <param name="times"></param>
        /// <param name="action"></param>
        /// <param name="efunc"></param>
        /// <returns></returns>
        public static T RetryOnException<T, E>(int times, Func<int, T> action, Action<(int current, Exception ex)> efunc) where E : Exception
        {
            Exception exception = null;
            for (int i = 0; i < times; i++)
            {
                try
                {
                    try
                    {
                        return action.Invoke(i + 1);
                    }
                    catch (E)
                    {
                    }
                }
                catch (Exception ex)
                {
                    exception = ex;
                    break;
                }
            }
            throw exception;
        }
        /// <summary>
        /// 重试指定任务，除非遇到异常<typeparamref name="E"/>就不再重试
        /// </summary>
        /// <typeparam name="T">返回值</typeparam>
        /// <typeparam name="E">遇到此异常不再重试</typeparam>
        /// <param name="times">次数</param>
        /// <param name="action">调用的方法</param>
        /// <param name="efunc"></param>
        /// <returns></returns>
        public static T RetryUnlessException<T, E>(int times, Func<int, T> action) where E : Exception
        {
            Exception exception = null;
            for (int i = 0; i < times; i++)
            {
                try
                {
                    try
                    {
                        return action.Invoke(i + 1);
                    }
                    catch (E ex)
                    {
                        exception = ex;
                        break;
                    }
                }
                catch (Exception)
                {

                }
            }
            throw exception;
        }
       
        public static T RetryOnAny<T>(int times, Func<int, T> action, Action<(int current, Exception ex)> efunc)
        {
            Exception exception = null;
            for (int i = 0; i < times; i++)
            {
                try
                {
                    return action.Invoke(i + 1);
                }
                catch (Exception ex)
                {
                    exception = ex;
                    efunc?.Invoke((i + 1, ex));
                }
            }
            throw exception;
        }
        public static T RetryOnAny<T>(int times, Func<T> action, Action<(int current, Exception ex)> efunc)
        {
            Exception exception = null;
            for (int i = 0; i < times; i++)
            {
                try
                {
                    return action.Invoke();
                }
                catch (Exception ex)
                {
                    exception = ex;
                    efunc?.Invoke((i + 1, ex));
                }
            }
            throw exception;
        }
    }
}
