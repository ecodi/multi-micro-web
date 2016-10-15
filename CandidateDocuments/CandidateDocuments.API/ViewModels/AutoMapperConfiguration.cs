using System.Linq;
using AutoMapper;

namespace CandidateDocuments.API.ViewModels
{
    /// <summary>
    /// Responsible for registering mappings between view and domain models.
    /// </summary>
    public class AutoMapperConfiguration
    {
        private static readonly object InitLock = new object();
        public static void Configure()
        {
            lock (InitLock)
            {
                if (!Mapper.GetAllTypeMaps().Any())
                {
                    Mapper.Initialize(c =>
                    {
                        c.AddProfile<DocumentModelMap>();
                    });
                }
            }
        }
    }
}
