using System;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace CandidateDocuments.Tests.Integration.Fixtures
{
    /// <summary>
    /// Tests configuration.
    /// </summary>
    public class TestSettingsFixture : IDisposable
    {
        public IConfigurationRoot Configuration { get; set; }

        public TestSettingsFixture()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("test.json");
            Configuration = builder.Build();
        }

        public void Dispose()
        {
        }
    }
}
