using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace CandidateDocuments.API.Middlewares
{
    /// <summary>
    /// Middleware responsible for logging all requests.
    /// </summary>
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;

        public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            var data = JsonConvert.SerializeObject(new
            {
                RemoteIpAddress = context.Connection.RemoteIpAddress?.ToString(),
                context.Request.Path,
                context.Request.ContentType,
                context.Request.ContentLength,
                context.Request.Headers
            });
            _logger.LogInformation(data);
            await _next.Invoke(context);
        }
    }
}
