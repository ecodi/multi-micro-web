using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Extensions.Options;
using CandidateDocuments.Application.Core;
using System.Net.Http;

namespace CandidateDocuments.Application.Services
{
    public class ModulesServiceOptions
    {
        public virtual string Endpoint { get; set; }
        public virtual TimeSpan Timeout { get; set; }
        public virtual string StabilityPolicy { get; set; }
    }

    public class ModuleDto
    {
        public int ModuleId { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public bool IsVisible { get; set; }
    }

    public class ModulesListDto
    {
        public ICollection<ModuleDto> Modules { get; set; }
    }

    /// <summary>
    /// Provides modules information based on connection with external service.
    /// </summary>
    public class ModulesService : IModulesService
    {
        public static string AllModulesCacheKey = "Modules:All";
        private readonly ICacheManager _cache;
        private readonly IWebClient _webclient;
        private readonly IWorkContext _workContext;
        private readonly ModulesServiceOptions _options;
        private readonly SemaphoreSlim _modulesCacheSemaphore = new SemaphoreSlim(1);

        public ModulesService(IOptions<ModulesServiceOptions> options, IWorkContext workContext,
            ICacheManager cache, IWebClient webclient)
        {
            _options = options.Value;
            _workContext = workContext;
            _cache = cache;
            _webclient = webclient;
        }

        public async Task<bool> IsActive(string moduleName)
        {
            var modulesDto = await GetModulesList();
            var isactive = modulesDto.Modules.Where(m => m.Name == moduleName).Select(m => m.IsActive).FirstOrDefault();
            return isactive;
        }

        public async Task<ModulesListDto> GetModulesList()
        {
            ModulesListDto modulesDto;
            await _modulesCacheSemaphore.WaitAsync();
            try
            {
                modulesDto = await _cache.GetAsync(AllModulesCacheKey, RequestModulesList);
            }
            finally
            {
                _modulesCacheSemaphore.Release();
            }
            return modulesDto;
        }

        private async Task<ModulesListDto> RequestModulesList()
        {
            var apiKey = _workContext.ApiKey;
            if (string.IsNullOrWhiteSpace(apiKey)) throw new IncompleteRequest("Empty Api-Key");

            var request = new HttpRequestMessage(HttpMethod.Get, _options.Endpoint);
            request.Headers.Add("Api-Key", apiKey);
            var modulesDto =
                await _webclient.MakeRequestAsync<ModulesListDto>(_options.StabilityPolicy, request, _options.Timeout);
            return modulesDto;
        }
    }
}
