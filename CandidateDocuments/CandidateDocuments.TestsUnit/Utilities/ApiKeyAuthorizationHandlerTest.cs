using System.Collections.Generic;
using System.Security.Authentication;
using CandidateDocuments.API.Core.Authorization;
using Microsoft.AspNetCore.Authorization;
using Moq;
using Xunit;
using CandidateDocuments.Application.Core;

namespace CandidateDocuments.Tests.Unit.Utilities
{
    public class ApiKeyAuthorizationHandlerTest
    {
        private readonly Mock<IWorkContext> _workContextMock;
        private readonly ApiKeyAuthorizationHandler _authHandler;

        public ApiKeyAuthorizationHandlerTest()
        {
            _workContextMock = new Mock<IWorkContext>();
            _authHandler = new ApiKeyAuthorizationHandler(_workContextMock.Object);
        }

        public class LogMethod : ApiKeyAuthorizationHandlerTest
        {
            [Theory,
                InlineData("key 1", true),
                InlineData("key 2", false),
                InlineData(null, false)]
            public async void HandleRequirementSucceedBasedOnFilter(string apiKey, bool expectedSuccess)
            {
                _workContextMock.SetupGet(m => m.ApiKey).Returns(apiKey);
                var requirement = new ApiKeyRequirement(key => key == "key 1");
                var context = new AuthorizationHandlerContext(new List<IAuthorizationRequirement> { requirement }, null, null);
                if (expectedSuccess)
                {
                    await _authHandler.HandleAsync(context);
                    Assert.True(context.HasSucceeded);
                }
                else
                {
                    await Assert.ThrowsAsync<AuthenticationException>(() => _authHandler.HandleAsync(context));
                    Assert.False(context.HasSucceeded);
                }
            }
        }
    }
}
