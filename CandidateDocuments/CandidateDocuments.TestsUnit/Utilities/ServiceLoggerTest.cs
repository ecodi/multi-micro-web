using System;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using CandidateDocuments.Application.Logging;

namespace CandidateDocuments.Tests.Unit.Utilities
{
    public class ServiceLoggerTest
    {
        private readonly Mock<ILogService> _logServiceMock;

        public ServiceLoggerTest()
        {
            _logServiceMock = new Mock<ILogService>();
        }

        public class LogMethod : ServiceLoggerTest
        {
            [Theory,
                InlineData(true),
                InlineData(false)]
            public void FilteresLogsToBeStored(bool filterResult)
            {
                Func<string, LogLevel, bool> filter = (name, level) => filterResult;
                var logger = new ServiceLogger("TestLogger", filter, _logServiceMock.Object);
                Func<object, Exception, string> formatter = (state, exc) => "";
                logger.Log(LogLevel.Information, new EventId(), null, null, formatter);
                _logServiceMock.Verify(m => m.StoreLog(It.IsAny<LogData>()), filterResult ? Times.Once() : Times.Never());
            }
        }
    }
}
