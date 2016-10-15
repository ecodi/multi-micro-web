using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using FluentValidation.AspNetCore;
using Newtonsoft.Json.Serialization;
using RawRabbit.Configuration;
using CandidateDocuments.API.ViewModels;
using CandidateDocuments.API.Core;
using CandidateDocuments.API.Core.Authorization;
using CandidateDocuments.API.Core.Pagination;
using CandidateDocuments.API.Middlewares;
using CandidateDocuments.Application.Core;
using CandidateDocuments.Application.Logging;
using CandidateDocuments.Application.Repositories;
using CandidateDocuments.Application.Services;
using CandidateDocuments.Infrastructure.Connectivity;
using CandidateDocuments.Infrastructure.FailureHandlers;
using CandidateDocuments.Infrastructure.Data;
using CandidateDocuments.Infrastructure.Repositories;

namespace CandidateDocuments.API
{
    public class Startup
    {
        private static bool _isDevelopment;
        private static int _dbProviderType = 0;
        public IConfigurationRoot Configuration { get; set; }

        public Startup(IHostingEnvironment env)
        {
            var contentRootPath = env.ContentRootPath;
            _isDevelopment = env.IsDevelopment();

            var builder = new ConfigurationBuilder().SetBasePath(contentRootPath)
                .AddJsonFile("app.json")
                .AddJsonFile($"app.{env.EnvironmentName}.json", true);
            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            ConfigureDatabase(services);
            ConfigureDependencies(services);

            services.AddCors();
            services.AddMvc()
                .AddJsonOptions(opts =>
                {
                    opts.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                }).AddFluentValidation(fv => fv.RegisterValidatorsFromAssemblyContaining<Startup>());
            services.AddAuthorization(options =>
            {
                options.AddPolicy("AnyApiKey", policy => policy.Requirements.Add(new AnyApiKeyRequirement()));
            });

            services.AddSwaggerGen();
            services.ConfigureSwaggerGen(options =>
            {
                options.SingleApiVersion(new Swashbuckle.Swagger.Model.Info
                {
                    Version = "v1",
                    Title = "Candidate Documents API",
                });
                options.DescribeAllEnumsAsStrings();
                options.OperationFilter<ApiKeyHeaderOperationFilter>();
                options.OperationFilter<PaginationHeaderOperationFilter>();
            });
        }

        public void ConfigureDatabase(IServiceCollection services)
        {
            bool memorydb = false;
            bool.TryParse(Configuration["ASPNETCORE_MEMORYDB"], out memorydb);
            if (!memorydb)
                int.TryParse(Configuration["Data:CandidateDocumentsConnection:ProviderType"], out _dbProviderType);
            services.AddDbContext<EfDbContext>(options =>
            {
                switch (_dbProviderType)
                {
                    case 1:
                        options.UseSqlServer(Configuration["Data:CandidateDocumentsConnection:ConnectionString"],
                            b => b.MigrationsAssembly("CandidateDocuments.Migrations.SqlServer"));
                        break;
                    case 2:
                        options.UseNpgsql(Configuration["Data:CandidateDocumentsConnection:ConnectionString"],
                            b => b.MigrationsAssembly("CandidateDocuments.Migrations.Npgsql"));
                        break;
                    default:
                        options.UseInMemoryDatabase();
                        break;
                }
            });
        }

        public void ConfigureDependencies(IServiceCollection services)
        {
            var stabilityPolicies = new List<string>();
            services.AddSingleton<IAuthorizationHandler, ApiKeyAuthorizationHandler>()
                .AddSingleton<IHttpContextAccessor, HttpContextAccessor>()
                .AddTransient<IWebClient, HttpWebClient>()
                .AddTransient<IFailureHandler, PollyFailureHandler>()
                .AddScoped<ICacheManager, PerRequestCacheManager>()
                .AddScoped<IWorkContext, WorkContext>()
                .AddScoped<IDocumentRepository, DocumentRepository>()
                .AddScoped<IDocumentsService, DocumentsService>();

            var modulePolicyName = Configuration["Services:ModulesService:StabilityPolicy"];
            if (!string.IsNullOrWhiteSpace(modulePolicyName)) stabilityPolicies.Add(modulePolicyName);
            services.AddModulesService(options =>
            {
                options.Endpoint = Configuration["Services:ModulesService:Endpoint"];
                options.Timeout = TimeSpan.FromMilliseconds(Convert.ToInt32(Configuration["Services:ModulesService:TimeoutMilliseconds"]));
                options.StabilityPolicy = modulePolicyName;
            });
            services.AddPollyPolicyProvider(options =>
            {
                options.PoliciesOptions = stabilityPolicies.ToDictionary(name => name, name => new PollyPolicyOptions
                {
                    BreakOnNumberOfExceptions = Convert.ToInt32(Configuration[$"StabilityPolicies:{name}:BreakOnNumberOfExceptions"]),
                    BreakedCircuitPeriod = TimeSpan.FromSeconds(Convert.ToInt32(Configuration[$"StabilityPolicies:{name}:BreakCircuitForSeconds"])),
                    NumberOfRetriesPerRequest = Convert.ToInt32(Configuration[$"StabilityPolicies:{name}:NumberOfRetriesPerRequest"])
                });
            });

            ConfigureServiceBus(services);

            AutoMapperConfiguration.Configure();
        }

        public virtual void ConfigureServiceBus(IServiceCollection services)
        {
            var exchangeName = Configuration["MqLogging:ExchangeName"];
            if (string.IsNullOrWhiteSpace(exchangeName))
            {
                services.AddSingleton<ILogService, NullLogService>();
            }
            else
            {
                services.Configure<RawRabbitConfiguration>(config => Configuration.GetSection("RawRabbit").Bind(config));
                services.AddMqLogService(options =>
                {
                    options.ExchangeName = Configuration["MqLogging:ExchangeName"];
                    options.RoutingKey = Configuration["MqLogging:RoutingKey"];
                });
            }
        }

        public void Configure(IApplicationBuilder app, ILoggerFactory loggerProvider, ILogService logService = null)
        {
            app.UseStaticFiles();
            app.UseCors(builder => builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());

            if (_isDevelopment)
            {
                loggerProvider.AddDebug(LogLevel.Information);
                app.UseDeveloperExceptionPage();
            }
            else
            {
                loggerProvider.AddConsole(LogLevel.Error);
            }
            loggerProvider.AddProvider(
                new ServiceLoggerProvider(
                    (categoryName, logLevel) => categoryName.Contains(nameof(RequestLoggingMiddleware)), logService));

            app.UseGlobalExceptionHanlder(_isDevelopment);
            app.UseMiddleware<RequestLoggingMiddleware>();

            app.UseMvc();
            app.UseSwagger();
            app.UseSwaggerUi();

            InitDatabase(app);
        }

        public virtual void InitDatabase(IApplicationBuilder app)
        {
            if (_dbProviderType < 1 || _dbProviderType > 2 ||
                !bool.Parse(Configuration["Data:CandidateDocumentsConnection:RunMigrationsAtStartup"])) return;
            using (var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
            {
                var dbContext = serviceScope.ServiceProvider.GetService<EfDbContext>();
                dbContext.Database.Migrate();
            }
        }
    }

    public static class ServicesWithOptionsExtensions
    {
        public static IServiceCollection AddModulesService(this IServiceCollection collection, Action<ModulesServiceOptions> setupAction)
        {
            collection.Configure(setupAction);
            return collection.AddScoped<IModulesService, ModulesService>();
        }
        public static IServiceCollection AddPollyPolicyProvider(this IServiceCollection collection, Action<PollyPolicyProviderOptions> setupAction)
        {
            collection.Configure(setupAction);
            return collection.AddSingleton<PollyPolicyProvider>();
        }

        public static IServiceCollection AddMqLogService(this IServiceCollection collection, Action<MqLogServiceOptions> setupAction)
        {
            collection.Configure(setupAction);
            return collection.AddSingleton<ILogService, MqLogService>();
        }
    }
}