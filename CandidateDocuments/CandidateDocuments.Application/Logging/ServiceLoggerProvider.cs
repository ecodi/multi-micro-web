using System;
using Microsoft.Extensions.Logging;

namespace CandidateDocuments.Application.Logging
{
    /// <summary>
    /// Provides logging managers.
    /// </summary>
    public class ServiceLoggerProvider : ILoggerProvider
    {
        private readonly Func<string, LogLevel, bool> _filter;
        private readonly ILogService _logService;

        public ServiceLoggerProvider(Func<string, LogLevel, bool> filter, ILogService logService)
        {
            _filter = filter;
            _logService = logService;
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new ServiceLogger(categoryName, _filter, _logService);
        }

        public void Dispose()
        {
        }
    }
}
