using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace CandidateDocuments.Application.Logging
{
    public class LogData
    {
        public virtual LogLevel LogLevel { get; set; }
        public virtual EventId EventId { get; set; }
        public virtual Exception Exception { get; set; }
        public virtual string Message { get; set; }
    }

    public interface ILogService : IDisposable
    {
        /// <summary>
        /// Stores log entry.
        /// </summary>
        /// <param name="data">Data to log.</param>
        Task StoreLog(LogData data);
    }
}
