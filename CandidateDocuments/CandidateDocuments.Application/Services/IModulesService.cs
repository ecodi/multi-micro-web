using System.Threading.Tasks;

namespace CandidateDocuments.Application.Services
{
    public static class Modules
    {
        public static string AccountManagement = "Account Management";
    }
    public interface IModulesService
    {
        /// <summary>
        /// Checks if module is active.
        /// </summary>
        /// <param name="moduleName">Name of the module</param>
        /// <returns>Result</returns>
        Task<bool> IsActive(string moduleName);
    }
}
