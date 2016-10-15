using System;
using System.Collections.Generic;
using System.Net.Http;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using CandidateDocuments.Application.Core;
using CandidateDocuments.Application.Services;

namespace CandidateDocuments.Tests.Unit.Services
{
    public class ModulesServiceTest
    {
        private readonly Mock<ICacheManager> _cacheMock;
        private readonly Mock<IWebClient> _webClientMock;

        private readonly ModulesServiceOptions _serviceOptions = new ModulesServiceOptions
        {
            StabilityPolicy = "test policy",
            Endpoint = "http://localhost",
            Timeout = TimeSpan.Zero
        };

        private readonly ModulesService _modulesService;
        public string ModulesCacheKey = ModulesService.AllModulesCacheKey;

        public ModulesServiceTest()
        {
            var workContextMock = new Mock<IWorkContext>();
            workContextMock.SetupGet(m => m.ApiKey).Returns("key");
            _cacheMock = new Mock<ICacheManager>();
            _webClientMock = new Mock<IWebClient>();
            var optionsMock = new Mock<IOptions<ModulesServiceOptions>>();
            optionsMock.SetupGet(m => m.Value).Returns(_serviceOptions);

            var modules = new ModulesListDto
            {
                Modules = new List<ModuleDto>
                {
                    new ModuleDto
                    {
                        Name = "Active 1",
                        IsActive = true
                    },
                    new ModuleDto
                    {
                        Name = "Active 2",
                        IsActive = true
                    },
                    new ModuleDto
                    {
                        Name = "Inactive",
                        IsActive = false
                    }
                }
            };
            _cacheMock.Setup(x => x.IsSet(ModulesCacheKey)).Returns(true);
            _cacheMock.Setup(x => x.Get<ModulesListDto>(ModulesCacheKey)).Returns(modules);

            _modulesService = new ModulesService(optionsMock.Object, workContextMock.Object, _cacheMock.Object, _webClientMock.Object);
        }

        public class IsActiveMethod : ModulesServiceTest
        {
            [Fact]
            public async void ReturnsTrueIfModuleIsActive()
            {
                Assert.True(await _modulesService.IsActive("Active 1"));
            }

            [Fact]
            public async void ReturnsFalseIfModuleIsInactive()
            {
                Assert.False(await _modulesService.IsActive("Inactive"));
            }

            [Fact]
            public async void ReturnsFalseIfModuleNotFound()
            {
                Assert.False(await _modulesService.IsActive("Nonexistent"));
            }
        }

        public class GetModulesListMethod : ModulesServiceTest
        {
            [Fact]
            public async void AfterFirstCallResultOfRequestIsCached()
            {
                _cacheMock.Setup(x => x.IsSet(ModulesCacheKey)).Returns(false);
                _cacheMock.Setup(x => x.Set(ModulesCacheKey, It.IsAny<ModulesListDto>(), It.IsAny<TimeSpan>())).Callback(
                    () => _cacheMock.Setup(x => x.IsSet(ModulesCacheKey)).Returns(true));
                for (var i = 0; i <= 2; i++)
                {
                    await _modulesService.GetModulesList();
                    _webClientMock.Verify(m => m.MakeRequestAsync<ModulesListDto>(
                        _serviceOptions.StabilityPolicy, It.IsAny<HttpRequestMessage>(), _serviceOptions.Timeout), Times.Once);
                }
            }
        }
    }
}
