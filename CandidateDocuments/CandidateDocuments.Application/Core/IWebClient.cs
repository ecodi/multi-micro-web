using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace CandidateDocuments.Application.Core
{
    public interface IWebClient
    {
        /// <summary>
        /// Makes web request with failure handling based on policy.
        /// </summary>
        /// <typeparam name="TResult">Result type</typeparam>
        /// <param name="stabilityPolicyName">Name of the stability policy</param>
        /// <param name="request">Request</param>
        /// <param name="timeout">Timeout</param>
        /// <returns>Result of performed request</returns>
        Task<TResult> MakeRequestAsync<TResult>(string stabilityPolicyName, HttpRequestMessage request, TimeSpan timeout) where TResult : new();
    }
}
