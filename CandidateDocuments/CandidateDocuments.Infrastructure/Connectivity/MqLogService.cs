using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using RawRabbit.Configuration;
using RawRabbit.vNext;
using RawRabbit.vNext.Disposable;
using CandidateDocuments.Application.Logging;
using System;
using Microsoft.Extensions.Options;

namespace CandidateDocuments.Infrastructure.Connectivity
{
    public class MqLogServiceOptions
    {
        public virtual string ExchangeName { get; set; }
        public virtual string RoutingKey { get; set; }
    }

    /// <summary>
    /// Logging service with the purpose of storing logs by external service.
    /// Logs are sent by message broker.
    /// </summary>
    public class MqLogService : ILogService
    {
        private readonly IBusClient _mqClient;
        private readonly MqLogServiceOptions _options;

        public MqLogService(IOptions<RawRabbitConfiguration> config, IOptions<MqLogServiceOptions> options)
        {
            _mqClient = BusClientFactory.CreateDefault(config.Value);
            _options = options.Value;
        }

        public Task StoreLog(LogData data)
        {
            return _mqClient.PublishAsync(data, Guid.NewGuid(),
                config => config.WithExchange(exc => exc.WithName(_options.ExchangeName)).WithRoutingKey(_options.RoutingKey));
        }

        public void Dispose()
        {
            _mqClient.Dispose();
        }
    }
}
