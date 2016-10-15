using CandidateDocuments.Application.Models;
using CandidateDocuments.Application.Repositories;
using CandidateDocuments.Infrastructure.Data;

namespace CandidateDocuments.Infrastructure.Repositories
{
    public class DocumentRepository : BaseModelRepository<Document>, IDocumentRepository
    {
        public DocumentRepository(EfDbContext context) : base(context)
        {            
        }
    }
}
