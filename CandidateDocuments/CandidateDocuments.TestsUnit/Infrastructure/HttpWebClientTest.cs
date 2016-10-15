using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CandidateDocuments.Infrastructure.Connectivity;
using CandidateDocuments.Infrastructure.FailureHandlers;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Xunit;

namespace CandidateDocuments.Tests.Unit.Infrastructure
{
    public class HttpWebClientTest
    {
        private readonly Mock<IFailureHandler> _failureHandlerMock;
        private readonly HttpWebClient _webClient;
        private readonly HttpRequestMessage _request = new HttpRequestMessage(HttpMethod.Get, "http://localhost");

        private class TestDto {
            public string TestProp { get; set; }
        }

        public HttpWebClientTest()
        {
            _failureHandlerMock = new Mock<IFailureHandler>();
            var loggerMock = new Mock<ILogger<HttpWebClient>>();
            _webClient = new HttpWebClient(_failureHandlerMock.Object, loggerMock.Object);
        }

        public class MakeRequestAsyncMethod : HttpWebClientTest
        {
            [Fact]
            public async void InvokesFailureHandler()
            {
                const string policyName = "some policy";
                await _webClient.MakeRequestAsync<TestDto>(policyName, _request, TimeSpan.Zero);
                _failureHandlerMock.Verify(m => m.ExecuteAsync(policyName, It.IsAny<Func<Task<TestDto>>>()), Times.Once);
            }
        }

        public class RequestAsyncMethod : HttpWebClientTest
        {
            private readonly Mock<HttpClientHandler> _messageHandlerMock = new Mock<HttpClientHandler>();

            [Fact]
            public async void ReturnsExpectedDto()
            {
                const string expectedValue = "value";
                var expectedResponse = $"{{ testProp: '{expectedValue}' }}";
                _messageHandlerMock.Protected()
                    .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.Is<HttpRequestMessage>(m => m == _request), ItExpr.IsAny<CancellationToken>())
                    .Returns(Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(expectedResponse) }));

                var result = await HttpWebClient.RequestAsync<TestDto>(_request,
                    httpClient: new HttpClient(_messageHandlerMock.Object));
                Assert.Equal(result.TestProp, expectedValue);
            }

            [Fact]
            public async void ThrowsHttpRequestExceptionIfResponseNotSucceed()
            {
                _messageHandlerMock.Protected()
                    .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.Is<HttpRequestMessage>(m => m == _request), ItExpr.IsAny<CancellationToken>())
                    .Returns(Task.FromResult(new HttpResponseMessage(HttpStatusCode.BadGateway)));

                await Assert.ThrowsAsync<HttpRequestException>(() => HttpWebClient.RequestAsync<TestDto>(_request,
                    httpClient: new HttpClient(_messageHandlerMock.Object)));
            }
        }
    }
}
