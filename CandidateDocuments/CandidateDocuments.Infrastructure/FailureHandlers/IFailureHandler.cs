using System;
using System.Threading.Tasks;

namespace CandidateDocuments.Infrastructure.FailureHandlers
{
    public interface IFailureHandler
    {
        /// <summary>
        /// Executes action wrapped with failure handler.
        /// </summary>
        /// <typeparam name="TResult">Type of result</typeparam>
        /// <param name="stabilityPolicyName">Name of the stability policy</param>
        /// <param name="action">Action to execute</param>
        /// <returns>Result</returns>
        Task<TResult> ExecuteAsync<TResult>(string stabilityPolicyName, Func<Task<TResult>> action) where TResult : new();
    }
}
