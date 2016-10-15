using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using CandidateDocuments.Application.Services;

namespace CandidateDocuments.Tests.Integration.Fixtures
{
    /// <summary>
    /// Base class for running local web host.
    /// </summary>
    public abstract class BaseWebHostFixture : IDisposable
    {
        public IWebHost Host { get; protected set; }
        private readonly CancellationTokenSource _tokenSource = new CancellationTokenSource();

        protected void InitHost(params string[] urls)
        {
            Host = new WebHostBuilder().UseKestrel().UseUrls(urls)
                .Configure(
                    app => app.Run(PrepareResponse)).Build();
        }

        protected async void RunHost()
        {
            var token = _tokenSource.Token;
            await Task.Run(() => Host.Run(token), token);
        }

        protected abstract Task PrepareResponse(HttpContext context);

        public void Dispose()
        {
            _tokenSource.Cancel();
        }
    }

    /// <summary>
    /// Local web host that in a simplified manner imitates external Modules service.
    /// </summary>
    public class ModulesWebHostFixture : BaseWebHostFixture
    {
        public ModulesWebHostFixture()
        {
            var config = new TestSettingsFixture().Configuration;
            if (bool.Parse(config["RunLocalWebHost"]))
            {
                InitHost(config["LocalWebHostAddress"]);
                RunHost();
            }
        }

        protected override async Task PrepareResponse(HttpContext context)
        {
            var response = context.Response;
            if (context.Request.Headers.ContainsKey("Api-Key"))
            {
                response.ContentType = new MediaTypeHeaderValue("application/json").ToString();
                var body = JsonConvert.SerializeObject(new ModulesListDto
                {
                    Modules = new List<ModuleDto>()
                });
                await response.WriteAsync(body, Encoding.UTF8).ConfigureAwait(false);
            }
            else
            {
                response.StatusCode = (int) HttpStatusCode.Unauthorized;
            }
        }
    }
}
