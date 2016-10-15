using System;
using System.Security.Authentication;
using System.Threading.Tasks;
using CandidateDocuments.Application.Core;
using Microsoft.AspNetCore.Authorization;

namespace CandidateDocuments.API.Core.Authorization
{
    public class ApiKeyRequirement : IAuthorizationRequirement
    {
        public Func<string, bool> Filter { get; protected set; }

        public ApiKeyRequirement()
        {            
        }
        public ApiKeyRequirement(Func<string, bool> filter)
        {
            Filter = filter;
        }
    }

    /// <summary>
    /// Handles authorization based on provided API key.
    /// </summary>
    public class ApiKeyAuthorizationHandler : AuthorizationHandler<ApiKeyRequirement>
    {
        private readonly IWorkContext _workContext;

        public ApiKeyAuthorizationHandler(IWorkContext workContext)
        {
            _workContext = workContext;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
            ApiKeyRequirement requirement)
        {
            var apiKey = _workContext.ApiKey;
            if (!requirement.Filter(apiKey)) throw new AuthenticationException();
            context.Succeed(requirement);
            return Task.FromResult(0);
        }
    }
}
