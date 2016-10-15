using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using CandidateDocuments.Application.Core;
using CandidateDocuments.Infrastructure.FailureHandlers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace CandidateDocuments.Tests.Unit.Infrastructure
{
    public class PollyFailureHandlerTest
    {
        private readonly PollyFailureHandler _pollyHandler;
        private const string PolicyName = "policy";
        private readonly PollyPolicyOptions _policyOptions = new PollyPolicyOptions
        {
            BreakOnNumberOfExceptions = 3,
            BreakedCircuitPeriod = TimeSpan.FromMinutes(60),
            NumberOfRetriesPerRequest = 1
        };

        public PollyFailureHandlerTest()
        {
            var policiesOptions = new PollyPolicyProviderOptions
            {
                PoliciesOptions = new Dictionary<string, PollyPolicyOptions> { {PolicyName, _policyOptions} }
            };

            var optionsMock = new Mock<IOptions<PollyPolicyProviderOptions>>();
            optionsMock.SetupGet(m => m.Value).Returns(policiesOptions);
            var loggerMock = new Mock<ILogger<PollyPolicyProvider>>();
            var policyProvider = new PollyPolicyProvider(optionsMock.Object, loggerMock.Object);
            _pollyHandler = new PollyFailureHandler(policyProvider);
        }

        public class ExecuteAsyncMethod : PollyFailureHandlerTest
        {
            private readonly Mock<Func<Task<int>>> _succededActionMock = new Mock<Func<Task<int>>>();
            private readonly Mock<Func<Task<int>>> _failedActionMock = new Mock<Func<Task<int>>>();

            public ExecuteAsyncMethod()
            {
                _succededActionMock.Setup(a => a()).Returns(() => Task.FromResult(1));
                _failedActionMock.Setup(a => a()).Returns(() => Task.FromException<int>(new HttpRequestException()));
            }

            [Fact]
            public async void ExecutesSuccededActionOnce()
            {
                await _pollyHandler.ExecuteAsync(PolicyName, _succededActionMock.Object);
                _succededActionMock.Verify(a => a(), Times.Once);
            }

            [Fact]
            public async void RetriesFailuresBasedOnPolicy()
            {
                await Assert.ThrowsAsync<FailureHandledError>(() => _pollyHandler.ExecuteAsync(PolicyName, _failedActionMock.Object));
                _failedActionMock.Verify(a => a(), Times.Exactly(_policyOptions.NumberOfRetriesPerRequest + 1));
            }

            [Fact]
            public async void BreakesCircuitBasedOnPolicy()
            {
                for (var i = 0; i <= _policyOptions.BreakOnNumberOfExceptions; i++)
                    await
                        Assert.ThrowsAsync<FailureHandledError>(
                            () => _pollyHandler.ExecuteAsync(PolicyName, _failedActionMock.Object));
                _failedActionMock.Verify(a => a(), Times.Exactly(_policyOptions.BreakOnNumberOfExceptions));
            }
        }
    }
}
