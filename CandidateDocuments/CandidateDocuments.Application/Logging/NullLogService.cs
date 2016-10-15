using System.Threading.Tasks;

namespace CandidateDocuments.Application.Logging
{
    /// <summary>
    /// Empty logging service.
    /// </summary>
    public class NullLogService : ILogService
    {
        public async Task StoreLog(LogData data)
        {
            await Task.Delay(0);
        }

        public void Dispose()
        {
        }
    }
}
