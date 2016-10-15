using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using CandidateDocuments.API;
using CandidateDocuments.Application.Repositories;
using CandidateDocuments.Tests.Integration.Data;
using CandidateDocuments.Application.Logging;

namespace CandidateDocuments.Tests.Integration.Fixtures
{
    /// <summary>
    /// Runs tested project on local web host.
    /// Allows thoroughly test application functionality.
    /// </summary>
    public class TestAppFixture : IDisposable
    {
        public class TestStartup : Startup
        {
            public TestStartup(IHostingEnvironment env) : base(env)
            {
            }

            public override void ConfigureServiceBus(IServiceCollection services)
            {
                services.AddSingleton<ILogService, NullLogService>();
            }

            public override void InitDatabase(IApplicationBuilder app)
            {
                base.InitDatabase(app);
                using (var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
                {
                    SeedDatabase(serviceScope.ServiceProvider);
                }
            }

            private static void SeedDatabase(IServiceProvider serviceProvider)
            {
                var documentRepository = serviceProvider.GetService<IDocumentRepository>();
                foreach (var document in DocumentsTestData.Documents)
                    documentRepository.Add(document);
                documentRepository.Commit();
            }
        }
        public TestServer Server { get; }
        public HttpClient Client { get; }

        public TestAppFixture()
        {
            Server = new TestServer(new WebHostBuilder()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseEnvironment(EnvironmentName.Staging)
                .UseStartup<TestStartup>());
            Client = Server.CreateClient();
        }

        public async Task<HttpResponseMessage> MakeJsonRequest(string path, string method,
            IDictionary<string, string> headers = null, string body = null)
        {
            var requestBuilder = Server.CreateRequest(path)
                .AddHeader("Accept", new MediaTypeWithQualityHeaderValue("application/vnd.coreapi+json").MediaType);
            if (headers != null)
                foreach (var header in headers)
                    requestBuilder.AddHeader(header.Key, header.Value);
            if (!string.IsNullOrEmpty(body))
                requestBuilder.And(requestMessage => requestMessage.Content = new StringContent(body, Encoding.UTF8, new MediaTypeWithQualityHeaderValue("application/json").MediaType));
            return await requestBuilder.SendAsync(method);
        }

        public void Dispose()
        {
            Client.Dispose();
            Server.Dispose();
        }
    }
}
