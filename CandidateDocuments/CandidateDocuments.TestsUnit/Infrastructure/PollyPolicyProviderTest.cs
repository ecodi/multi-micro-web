using System.Collections.Generic;
using System.Linq;
using CandidateDocuments.Infrastructure.FailureHandlers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace CandidateDocuments.Tests.Unit.Infrastructure
{
    public class PollyPolicyProviderTest
    {
        private readonly Dictionary<string, Mock<PollyPolicyOptions>> _optionsMocks = new Dictionary<string, Mock<PollyPolicyOptions>> {
            { "policy 1", new Mock<PollyPolicyOptions>() },
            { "policy 2", new Mock<PollyPolicyOptions>() }
        };
        private readonly PollyPolicyProvider _policyProvider;

        public PollyPolicyProviderTest()
        {
            var policiesOptions = new PollyPolicyProviderOptions
            {
                PoliciesOptions = _optionsMocks.ToDictionary(m => m.Key, m => m.Value.Object)
            };
            foreach (var optionMock in _optionsMocks.Values)
                optionMock.Setup(m => m.BreakOnNumberOfExceptions).Returns(1);

            var optionsMock = new Mock<IOptions<PollyPolicyProviderOptions>>();
            optionsMock.SetupGet(m => m.Value).Returns(policiesOptions);
            var loggerMock = new Mock<ILogger<PollyPolicyProvider>>();
            _policyProvider = new PollyPolicyProvider(optionsMock.Object, loggerMock.Object);
        }

        public class GetWaitAndRetryPolicyMethod : PollyPolicyProviderTest
        {
            [Theory,
                InlineData("policy 1"),
                InlineData("policy 2")]
            public void ReturnsPolicyBasedOnOptions(string name)
            {
                var policy = _policyProvider.GetWaitAndRetryPolicy(name);
                Assert.NotNull(policy);
                _optionsMocks[name].VerifyGet(o => o.NumberOfRetriesPerRequest, Times.AtLeastOnce);
            }

            [Fact]
            public void ReturnsTransientPolicy()
            {
                const string name = "policy 1";
                Assert.NotSame(
                    _policyProvider.GetWaitAndRetryPolicy(name),
                    _policyProvider.GetWaitAndRetryPolicy(name));
            }
        }

        public class GetCircutBreakerPolicyMethod : PollyPolicyProviderTest
        {
            [Theory,
                InlineData("policy 1"),
                InlineData("policy 2")]
            public void ReturnsPolicyBasedOnOptions(string name)
            {
                var policy = _policyProvider.GetCircutBreakerPolicy(name);
                Assert.NotNull(policy);
                _optionsMocks[name].VerifyGet(o => o.BreakOnNumberOfExceptions, Times.AtLeastOnce);
                _optionsMocks[name].VerifyGet(o => o.BreakedCircuitPeriod, Times.AtLeastOnce);
            }

            [Fact]
            public void ReturnsSingletonPolicyBasedOnName()
            {
                const string name = "policy 1";
                Assert.Same(
                    _policyProvider.GetCircutBreakerPolicy(name),
                    _policyProvider.GetCircutBreakerPolicy(name));
                Assert.NotSame(
                    _policyProvider.GetCircutBreakerPolicy(name),
                    _policyProvider.GetCircutBreakerPolicy("policy 2"));
            }
        }
    }
}
