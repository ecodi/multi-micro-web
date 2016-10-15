using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using Moq;
using Castle.Core.Internal;
using AutoMapper;
using CandidateDocuments.Application.Models;
using CandidateDocuments.Application.Services;
using CandidateDocuments.API.Controllers;
using CandidateDocuments.API.ViewModels;

namespace CandidateDocuments.Tests.Unit.Controllers
{
    public class DocumentsControllerTest
    {
        private readonly Mock<IDocumentsService> _documentsServiceMock;
        private readonly DocumentsController _documentsController;
        private readonly List<Document> _documents;

        public DocumentsControllerTest()
        {
            _documentsServiceMock = new Mock<IDocumentsService>();

            var responseMock = new Mock<HttpResponse>();
            responseMock.SetupGet(x => x.Headers).Returns(new HeaderDictionary());
            var httpContextMock = new Mock<HttpContext>();
            httpContextMock.SetupGet(x => x.Response).Returns(responseMock.Object);

            _documentsController = new DocumentsController(_documentsServiceMock.Object)
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = httpContextMock.Object
                }
            };
            AutoMapperConfiguration.Configure();

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

        public class GetListMethod : DocumentsControllerTest
        {
            [Fact]
            public void ReturnsObjectsListResult()
            {
                var candidateId = Guid.NewGuid();
                _documentsServiceMock.Setup(
                        x =>
                            x.GetDocuments(candidateId, It.IsAny<int>(), It.IsAny<int>()))
                    .Returns(_documents);

                var result = (ObjectResult) _documentsController.GetList(candidateId.ToString());
                var documentsDto = (IEnumerable<DocumentViewModel>) result.Value;
                Assert.True(_documents.Select(d => d.Id).Except(documentsDto.Select(d => d.Id)).IsNullOrEmpty());
            }

            [Fact]
            public void ReturnsBadRequestForEmptyCandidateId()
            {
                var result = (ObjectResult) _documentsController.GetList(null);
                Assert.Equal((int) HttpStatusCode.BadRequest, result.StatusCode);
            }
        }

        public class GetDocumentMethod : DocumentsControllerTest
        {
            [Fact]
            public void ReturnsObjectResult()
            {
                var document = _documents[0];
                _documentsServiceMock.Setup(x => x.GetDocumentById(document.Id)).Returns(document);

                var result = (ObjectResult) _documentsController.Get(document.Id);
                var documentDto = (DocumentViewModel) result.Value;
                Assert.Equal(document.Id, documentDto.Id);
            }

            [Fact]
            public void ReturnsNotFoundForNotExistingId()
            {
                _documentsServiceMock.Setup(x => x.GetDocumentById(It.IsAny<Guid>())).Returns((Document) null);
                var result = (ObjectResult) _documentsController.Get(_documents[0].Id);
                Assert.Equal((int) HttpStatusCode.NotFound, result.StatusCode);
            }
        }

        public class CreateMethod : DocumentsControllerTest
        {
            [Fact]
            public void ReturnsCreatedObjectWithGeneratedIdAndCreationDate()
            {
                var start = DateTime.Now;
                DocumentSaveModel documentSm = Mapper.Map<Document, DocumentViewModel>(_documents[0]);
                var result = (ObjectResult)_documentsController.Create(documentSm);
                var documentDto = (DocumentViewModel)result.Value;
                Assert.Equal((int)HttpStatusCode.Created, result.StatusCode);
                _documentsServiceMock.Verify(m => m.AddDocument(It.IsAny<Document>()), Times.Once);
                Assert.NotEqual(documentDto.Id, Guid.Empty);
                Assert.True(documentDto.CreationDate.CompareTo(start) > 0);
            }

            [Fact]
            public void ReturnsBadRequestIfRequestNotValid()
            {
                _documentsController.ModelState.AddModelError("", "some error");
                var result = (ObjectResult)_documentsController.Create(null);
                _documentsServiceMock.Verify(m => m.AddDocument(It.IsAny<Document>()), Times.Never);
                Assert.Equal((int)HttpStatusCode.BadRequest, result.StatusCode);
            }
        }

        public class UpdateMethod : DocumentsControllerTest
        {
            [Fact]
            public void ReturnsUpdatedObject()
            {
                var document = _documents[0];
                _documentsServiceMock.Setup(x => x.GetDocumentById(document.Id)).Returns(document);

                DocumentSaveModel documentSm = Mapper.Map<Document, DocumentViewModel>(_documents[1]);
                var result = (ObjectResult)_documentsController.Update(document.Id, documentSm);
                var documentDto = (DocumentViewModel)result.Value;
                Assert.Equal((int)HttpStatusCode.OK, result.StatusCode);
                _documentsServiceMock.Verify(m => m.UpdateDocument(It.IsAny<Document>()), Times.Once);

                Assert.Equal(documentDto.Id, document.Id);
                foreach (PropertyInfo property in typeof(DocumentSaveModel).GetProperties())
                    Assert.Equal(property.GetValue(documentDto, null), property.GetValue(documentSm, null));
            }

            [Fact]
            public void ReturnsBadRequestIfRequestNotValid()
            {
                _documentsController.ModelState.AddModelError("", "some error");
                var result = (ObjectResult)_documentsController.Update(_documents[0].Id, null);
                _documentsServiceMock.Verify(m => m.UpdateDocument(It.IsAny<Document>()), Times.Never);
                Assert.Equal((int)HttpStatusCode.BadRequest, result.StatusCode);
            }

            [Fact]
            public void ReturnsNotFoundForNotExistingId()
            {
                _documentsServiceMock.Setup(x => x.GetDocumentById(It.IsAny<Guid>())).Returns((Document)null);
                var result = (ObjectResult)_documentsController.Update(_documents[0].Id, null);
                _documentsServiceMock.Verify(m => m.UpdateDocument(It.IsAny<Document>()), Times.Never);
                Assert.Equal((int)HttpStatusCode.NotFound, result.StatusCode);
            }
        }

        public class DeleteMethod : DocumentsControllerTest
        {
            [Fact]
            public void ReturnsNoContentResult()
            {
                var document = _documents[0];
                _documentsServiceMock.Setup(x => x.GetDocumentById(document.Id)).Returns(document);

                var result = (StatusCodeResult)_documentsController.Delete(document.Id);
                Assert.Equal((int)HttpStatusCode.NoContent, result.StatusCode);
                _documentsServiceMock.Verify(m => m.DeleteDocument(document), Times.Once);
            }

            [Fact]
            public void ReturnsNotFoundForNotExistingId()
            {
                _documentsServiceMock.Setup(x => x.GetDocumentById(It.IsAny<Guid>())).Returns((Document)null);
                var result = (ObjectResult)_documentsController.Delete(_documents[0].Id);
                _documentsServiceMock.Verify(m => m.DeleteDocument(It.IsAny<Document>()), Times.Never);
                Assert.Equal((int)HttpStatusCode.NotFound, result.StatusCode);
            }
        }
    }
}
