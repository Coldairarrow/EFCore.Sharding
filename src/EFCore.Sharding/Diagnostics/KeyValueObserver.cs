using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;

namespace EFCore.Sharding
{
    internal class KeyValueObserver : IObserver<KeyValuePair<string, object>>
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly int _minCommandElapsedMilliseconds;
        private readonly ConcurrentDictionary<Guid, string> _commandStackTraceDic
            = new ConcurrentDictionary<Guid, string>();
        private BlockingCollection<KeyValuePair<string, object>> _eventsQueue
            = new BlockingCollection<KeyValuePair<string, object>>();
        private Task _task;
        private readonly object _taskLock = new object();

        public KeyValueObserver(ILoggerFactory loggerFactory, int minCommandElapsedMilliseconds)
        {
            _loggerFactory = loggerFactory;
            _minCommandElapsedMilliseconds = minCommandElapsedMilliseconds;
        }
        public void OnCompleted()
            => throw new NotImplementedException();

        public void OnError(Exception error)
            => throw new NotImplementedException();

        public void OnNext(KeyValuePair<string, object> value)
        {
            //OnNext会阻塞当前线程,通过队列转异步处理
            if (_task == null)
            {
                lock (_taskLock)
                {
                    if (_task == null)
                    {
                        _task = Task.Factory.StartNew(() =>
                        {
                            foreach (var value in _eventsQueue.GetConsumingEnumerable())
                            {
                                var logger = _loggerFactory?.CreateLogger(GetType());
                                try
                                {
                                    LogLevel logLevel = LogLevel.Information;

                                    Exception ex = null;
                                    if (value.Key == RelationalEventId.CommandCreated.Name)
                                    {
                                        //只有CommandCreated时能拿到堆栈行号
                                        _commandStackTraceDic[((CommandCorrelatedEventData)value.Value).CommandId] = Environment.StackTrace;
                                    }
                                    if (value.Key == RelationalEventId.CommandExecuted.Name)
                                    {
                                        logLevel = LogLevel.Information;
                                    }
                                    if (value.Key == RelationalEventId.CommandError.Name)
                                    {
                                        logLevel = LogLevel.Information;
                                        ex = ((CommandErrorEventData)value.Value).Exception;
                                    }
                                    if (value.Key == RelationalEventId.CommandExecuted.Name || value.Key == RelationalEventId.CommandError.Name)
                                    {
                                        var commandEndEventData = value.Value as CommandEndEventData;

                                        if (logLevel == LogLevel.Error || commandEndEventData.Duration.TotalMilliseconds > _minCommandElapsedMilliseconds)
                                        {
                                            using var scop = logger.BeginScope(new Dictionary<string, object>
                                            {
                                                { "StackTrace",_commandStackTraceDic[commandEndEventData.CommandId]}
                                            });

                                            var message = @"执行SQL耗时({ElapsedMilliseconds}ms)
{SQL}";
                                            logger?.Log(
                                                logLevel,
                                                ex, message,
                                                (long)commandEndEventData.Duration.TotalMilliseconds,
                                                GetGeneratedSql(commandEndEventData.Command));
                                        }

                                        _commandStackTraceDic.TryRemove(commandEndEventData.CommandId, out _);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    logger?.LogError(ex, ex.Message);
                                }
                            }
                        });
                    }
                }
            }

            _eventsQueue.Add(value);
        }

        private static string GetGeneratedSql(DbCommand cmd)
        {
            string result = cmd.CommandText.ToString();

            if (result.Length > 100 * 1024)
            {
                result = result.Substring(0, 100 * 1024) + $"...剩余{result.Length - 100 * 1024}字符";
            }

            return result;
        }
    }
}
