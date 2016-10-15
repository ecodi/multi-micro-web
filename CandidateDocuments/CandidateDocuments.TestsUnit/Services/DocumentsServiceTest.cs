using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Moq;
using Xunit;
using CandidateDocuments.Application.Models;
using CandidateDocuments.Application.Repositories;
using CandidateDocuments.Application.Services;

namespace CandidateDocuments.TestsUnit.Services
{
    public class DocumentsServiceTest
    {
        private readonly Mock<IDocumentRepository> _documentRepositoryMock;
        private readonly DocumentsService _documentsService;
        private readonly List<Document> _documents;

        public DocumentsServiceTest()
        {
            _documentRepositoryMock = new Mock<IDocumentRepository>();
            _documentsService = new DocumentsService(_documentRepositoryMock.Object);

            _documents = new List<Document>
            {
                new Document
                {
                    Id = Guid.NewGuid(),
                    CandidateId = Guid.NewGuid(),
                    Filename = "MyCV.pdf",
                    DocumentType = DocumentType.BusinessPlan,
                    ReviewerId = Guid.NewGuid()
                },
                new Document
                {
                    Id = Guid.NewGuid(),
                    CandidateId = Guid.NewGuid(),
                    Filename = "MyDoc.docx",
                    DocumentType = DocumentType.License
                },
                new Document
                {
                    Id = Guid.NewGuid(),
                    CandidateId = Guid.NewGuid(),
                    Filename = "TestPdf.pdf",
                    DocumentType = DocumentType.Other
                }
            };
        }

        public class GetDocumentsMethod : DocumentsServiceTest
        {
            [Fact]
            public void ReturnsDocumentList()
            {
                var candidateId = Guid.NewGuid();
                _documentRepositoryMock.Setup(
                        x =>
                            x.FindBy(It.IsAny<Expression<Func<Document, bool>>>(),
                                It.IsAny<Expression<Func<Document, object>>>(), It.IsAny<int>(), It.IsAny<int>()))
                    .Returns(_documents);

                var result = _documentsService.GetDocuments(candidateId);
                Assert.Equal(_documents, result);
            }
        }

        public class GetDocumentByIdMethod : DocumentsServiceTest
        {
            [Fact]
            public void ReturnsDocument()
            {
                var document = _documents[0];
                _documentRepositoryMock.Setup(x => x.GetSingle(document.Id)).Returns(document);

                var result = _documentsService.GetDocumentById(document.Id);
                Assert.Equal(document, result);
            }
        }

        public class AddDocumentMethod : DocumentsServiceTest
        {
            [Fact]
            public void AddsDocument()
            {
                var document = _documents[0];
                _documentsService.AddDocument(document);
                _documentRepositoryMock.Verify(m => m.Add(document), Times.Once);
                _documentRepositoryMock.Verify(m => m.Commit(), Times.Once);
            }
        }

        public class UpdateDocumentMethod : DocumentsServiceTest
        {
            [Fact]
            public void UpdatesDocument()
            {
                var document = _documents[0];
                _documentsService.UpdateDocument(document);
                _documentRepositoryMock.Verify(m => m.Update(document), Times.Once);
                _documentRepositoryMock.Verify(m => m.Commit(), Times.Once);
            }
        }

        public class DeleteDocumentMethod : DocumentsServiceTest
        {
            [Fact]
            public void DeletesDocument()
            {
                var document = _documents[0];
                _documentsService.DeleteDocument(document);
                _documentRepositoryMock.Verify(m => m.Delete(document), Times.Once);
                _documentRepositoryMock.Verify(m => m.Commit(), Times.Once);
            }
        }
    }
}
