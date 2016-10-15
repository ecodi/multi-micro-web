using Microsoft.AspNetCore.Http;
using CandidateDocuments.Application.Core;

namespace CandidateDocuments.API.Core
{
    /// <summary>
    /// Application context based on request.
    /// </summary>
    public class WorkContext : IWorkContext
    {
        private readonly IHttpContextAccessor _accessor;
        private HttpContext Context => _accessor.HttpContext;

        public string ApiKey => Context.Request.Headers["Api-Key"];

        public WorkContext(IHttpContextAccessor accessor)
        {
            _accessor = accessor;
        }

    }
}
