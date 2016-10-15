using System;
using System.Threading;
using System.Threading.Tasks;
using CandidateDocuments.Application.Core;

namespace CandidateDocuments.Infrastructure.FailureHandlers
{
    /// <summary>
    /// Failure handler based on Polly library.
    /// Introduces "Wait And Retry" and "Circut Breaker" stability patterns.
    /// </summary>
    public class PollyFailureHandler : IFailureHandler
    {
        private readonly PollyPolicyProvider _policyProvider;

        public PollyFailureHandler(PollyPolicyProvider policyProvider)
        {
            _policyProvider = policyProvider;
        }

        public async Task<TResult> ExecuteAsync<TResult>(string stabilityPolicyName, Func<Task<TResult>> action) where TResult : new()
        {
            if (string.IsNullOrWhiteSpace(stabilityPolicyName)) return await action();

            var result = default(TResult);
            var waitAndRetryPolicy = _policyProvider.GetWaitAndRetryPolicy(stabilityPolicyName);
            var circuitBreakerPolicy = _policyProvider.GetCircutBreakerPolicy(stabilityPolicyName);
            var cancellationToken = new CancellationToken();
            try
            {
                await waitAndRetryPolicy.ExecuteAsync(async token =>
                {
                    result = await circuitBreakerPolicy.ExecuteAsync(action);
                }, cancellationToken);
            }
            catch (Exception)
            {
                throw new FailureHandledError($"Error while performing action with '{stabilityPolicyName}' policy.");
            }
            return result;
        }
    }
}
