using CandidateDocuments.Application.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CandidateDocuments.Application.Services
{
    public interface IDocumentsService
    {
        IEnumerable<Document> GetDocuments(Guid candidateId, int skip = 0, int take = 0);
        Document GetDocumentById(Guid id);
        void AddDocument(Document document);
        void UpdateDocument(Document document);
        void DeleteDocument(Document document);
    }
}
