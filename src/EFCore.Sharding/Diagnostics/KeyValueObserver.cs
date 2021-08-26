using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;

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
            if (value.Key == RelationalEventId.CommandError.Name)
            {
                var payload = (CommandErrorEventData)value.Value;

                _loggerFactory?.CreateLogger(GetType())?.LogError(payload.Exception, @"执行SQL耗时({ElapsedMilliseconds:N}ms) SQL:{SQL}",
                    payload.Duration.TotalMilliseconds, GetGeneratedSql(payload.Command));
            }
        }

        private string GetGeneratedSql(DbCommand cmd)
        {
            string result = cmd.CommandText.ToString();
            foreach (DbParameter p in cmd.Parameters)
            {
                var formattedValue = GetFormattedValue(p.Value);

                //最大记录10M数据
                if (formattedValue.Length < 10 * 1024 * 1024)
                {
                    result = result.Replace(p.ParameterName.ToString(), GetFormattedValue(p.Value));
                }
            }
            return result;
        }

        private string GetFormattedValue(object value)
        {
            string formattedValue = string.Empty;
            if (IsNumber(value))
            {
                formattedValue = value.ToString();
            }
            else if (value is string || value is DateTime || value is DateTimeOffset)
            {
                formattedValue = $"'{value}'";
            }
            else if (value is IEnumerable ienumerable)
            {
                formattedValue = $"array[{string.Join(",", ienumerable.Cast<object>().Select(x => GetFormattedValue(x)).ToArray())}]";
            }
            else
            {
                formattedValue = $"'{value}'";
            }

            return formattedValue;
        }

        private bool IsNumber(object value)
        {
            return value is sbyte
                || value is byte
                || value is short
                || value is ushort
                || value is int
                || value is uint
                || value is long
                || value is ulong
                || value is float
                || value is double
                || value is decimal;
        }
    }
}
