using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CandidateDocuments.Application.Models;
using CandidateDocuments.Application.Repositories;

namespace CandidateDocuments.Application.Services
{
    public class DocumentsService : IDocumentsService
    {
        private readonly IDocumentRepository _documentRepository;

        public DocumentsService(IDocumentRepository documentRepository)
        {
            _documentRepository = documentRepository;
        }

        public IEnumerable<Document> GetDocuments(Guid candidateId, int skip = 0, int take = 0)
        {
            return _documentRepository.FindBy(s => s.CandidateId == candidateId, s => s.Id, skip, take);
        }

        public Document GetDocumentById(Guid id)
        {
            return _documentRepository.GetSingle(id);
        }

        public void AddDocument(Document document)
        {
            _documentRepository.Add(document);
            _documentRepository.Commit();
        }

        public void UpdateDocument(Document document)
        {
            _documentRepository.Update(document);
            _documentRepository.Commit();
        }

        public void DeleteDocument(Document document)
        {
            _documentRepository.Delete(document);
            _documentRepository.Commit();
        }
    }
}
