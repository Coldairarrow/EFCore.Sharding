using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data.Common;

namespace EFCore.Sharding
{
    internal class KeyValueObserver : IObserver<KeyValuePair<string, object>>
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly int _minCommandElapsedMilliseconds;
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
            if (value.Key == RelationalEventId.CommandExecuted.Name)
            {
                var payload = (CommandExecutedEventData)value.Value;
                if (payload.Duration.TotalMilliseconds > _minCommandElapsedMilliseconds)
                {
                    _loggerFactory?.CreateLogger(GetType())?.LogInformation(@"执行SQL耗时({ElapsedMilliseconds:N}ms) SQL:{SQL}",
                        payload.Duration.TotalMilliseconds, GetGeneratedSql(payload.Command));
                }
            }
        }

        private string GetGeneratedSql(DbCommand cmd)
        {
            string result = cmd.CommandText.ToString();
            foreach (DbParameter p in cmd.Parameters)
            {
                string isQuted = (
                    p.Value is string
                    || p.Value is DateTime
                    || p.Value is DateTimeOffset)
                    ? "'" : "";
                result = result.Replace(p.ParameterName.ToString(), isQuted + p.Value.ToString() + isQuted);
            }
            return result;
        }
    }
}
