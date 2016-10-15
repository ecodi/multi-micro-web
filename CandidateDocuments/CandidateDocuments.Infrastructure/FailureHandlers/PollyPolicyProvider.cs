using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;

namespace CandidateDocuments.Infrastructure.FailureHandlers
{
    public class PollyPolicyOptions
    {
        public virtual int BreakOnNumberOfExceptions { get; set; }
        public virtual TimeSpan BreakedCircuitPeriod { get; set; }
        public virtual int NumberOfRetriesPerRequest { get; set; }
    }

    public class PollyPolicyProviderOptions
    {
        public virtual IDictionary<string, PollyPolicyOptions> PoliciesOptions { get; set; }
    }

    /// <summary>
    /// Provides failure handlers based on Polly library.
    /// </summary>
    public class PollyPolicyProvider
    {
        private readonly IOptions<PollyPolicyProviderOptions> _options;
        private readonly ILogger<PollyPolicyProvider> _logger;

        private readonly IDictionary<string, CircuitBreakerPolicy> _circutBreakerPolicies =
            new Dictionary<string, CircuitBreakerPolicy>();

        public PollyPolicyProvider(IOptions<PollyPolicyProviderOptions> options, ILogger<PollyPolicyProvider> logger)
        {
            _options = options;
            _logger = logger;
        }

        /// <summary>
        /// Provides "Wait And Retry" policy.
        /// </summary>
        /// <param name="name">Name of the policy category</param>
        /// <returns>Stability policy</returns>
        public RetryPolicy GetWaitAndRetryPolicy(string name)
        {
            var policyOptions = _options.Value.PoliciesOptions[name];
            var waitAndRetryPolicy = Policy
                .Handle<Exception>(e => !(e is BrokenCircuitException))
                .WaitAndRetryAsync(
                    policyOptions.NumberOfRetriesPerRequest,
                    retryAttempt => TimeSpan.FromMilliseconds(500 * retryAttempt),
                    (exception, timeSpan, retryCount, context) =>
                    {
                        _logger.LogInformation($"Retrying '{name}': {retryCount}");
                    });
            return waitAndRetryPolicy;
        }

        /// <summary>
        /// Provides "Circut Breaker" policy.
        /// </summary>
        /// <param name="name">Name of the policy category</param>
        /// <returns>Stability policy</returns>
        public CircuitBreakerPolicy GetCircutBreakerPolicy(string name)
        {
            lock (_circutBreakerPolicies)
            {
                if (_circutBreakerPolicies.ContainsKey(name)) return _circutBreakerPolicies[name];
                var policyOptions = _options.Value.PoliciesOptions[name];
                var circuitBreakerPolicy = Policy
                    .Handle<Exception>()
                    .CircuitBreakerAsync(
                        exceptionsAllowedBeforeBreaking: policyOptions.BreakOnNumberOfExceptions,
                        durationOfBreak: policyOptions.BreakedCircuitPeriod,
                        onBreak: (ex, breakDelay) =>
                        {
                            _logger.LogWarning($"Circut breaker '{name}' active for {breakDelay}");
                        },
                        onReset: () =>
                        {
                            _logger.LogInformation($"Circut breaker '{name}' closed");
                        },
                        onHalfOpen: () =>
                        {
                            _logger.LogInformation($"Circut breaker '{name}' half-opened");
                        }
                    );
                _circutBreakerPolicies[name] = circuitBreakerPolicy;
                return circuitBreakerPolicy;
            }
        }
    }
}
