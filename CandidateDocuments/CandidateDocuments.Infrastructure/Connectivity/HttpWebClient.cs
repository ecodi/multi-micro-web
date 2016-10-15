using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;
using CandidateDocuments.Application.Core;
using CandidateDocuments.Infrastructure.FailureHandlers;

namespace CandidateDocuments.Infrastructure.Connectivity
{
    /// <summary>
    /// Performs web requests with a usage of HttpClient and failure handling.
    /// </summary>
    public class HttpWebClient : IWebClient
    {
        private readonly IFailureHandler _failureHandler;
        private readonly ILogger<HttpWebClient> _logger;

        public HttpWebClient(IFailureHandler failureHandler, ILogger<HttpWebClient> logger)
        {
            _failureHandler = failureHandler;
            _logger = logger;
        }

        public async Task<TResult> MakeRequestAsync<TResult>(string stabilityPolicyName, HttpRequestMessage request, TimeSpan timeout)
            where TResult : new()
        {
            return await _failureHandler.ExecuteAsync(stabilityPolicyName,
                () => RequestAsync<TResult>(request, timeout, logger: _logger));
        }        

        public static async Task<TResult> RequestAsync<TResult>(HttpRequestMessage request, TimeSpan? timeout = null,
            HttpClient httpClient = null, ILogger logger = null) where TResult : new()
        {
            TResult result;
            using (var client = httpClient ?? new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Clear();
                if (timeout != null) client.Timeout = (TimeSpan)timeout;
                var response = await client.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    result = JsonConvert.DeserializeObject<TResult>(responseJson);
                }
                else
                {
                    logger?.LogWarning(response.ReasonPhrase);
                    throw new HttpRequestException(response.ReasonPhrase);
                }
            }
            return result;
        }
    }
}
