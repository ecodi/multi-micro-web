using Moq;
using Xunit;
using CandidateDocuments.Application.Logging;

namespace CandidateDocuments.Tests.Unit.Utilities
{
    public class ServiceLoggerProviderTest
    {
        private readonly ServiceLoggerProvider _loggerProvider;

        public ServiceLoggerProviderTest()
        {
            var logServiceMock = new Mock<ILogService>();
            _loggerProvider = new ServiceLoggerProvider(null, logServiceMock.Object);
        }

        public class CreateLoggerMethod : ServiceLoggerProviderTest
        {
            [Fact]
            public void ReturnsLogger()
            {
                var logger = _loggerProvider.CreateLogger("TestLogger");
                Assert.NotNull(logger);
            }

            [Fact]
            public void ReturnsTransientLogger()
            {
                const string categoryName = "TestLogger";
                Assert.NotSame(
                    _loggerProvider.CreateLogger(categoryName),
                    _loggerProvider.CreateLogger(categoryName));
            }
        }
    }
}
