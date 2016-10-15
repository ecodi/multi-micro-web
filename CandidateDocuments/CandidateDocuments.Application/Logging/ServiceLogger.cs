using System;
using Microsoft.Extensions.Logging;

namespace CandidateDocuments.Application.Logging
{
    /// <summary>
    /// Manages logging with usage of provided logging service.
    /// </summary>
    public class ServiceLogger : ILogger
    {
        private readonly string _categoryName;
        private readonly Func<string, LogLevel, bool> _filter;
        private readonly ILogService _logService;

        public ServiceLogger(string categoryName, Func<string, LogLevel, bool> filter, ILogService logService)
        {
            _categoryName = categoryName;
            _filter = filter;
            _logService = logService;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return _filter == null || _filter(_categoryName, logLevel);
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel)) return;
            if (formatter == null) throw new ArgumentNullException(nameof(formatter));

            var message = formatter(state, exception);
            _logService.StoreLog(new LogData
            {
                LogLevel = logLevel,
                EventId = eventId,
                Exception = exception,
                Message = message
            });
        }
    }
}
