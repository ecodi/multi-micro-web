using System;
using System.Collections.Generic;
using System.Net;
using Newtonsoft.Json;
using Xunit;
using CandidateDocuments.API.ViewModels;
using CandidateDocuments.Application.Models;
using CandidateDocuments.Tests.Integration.Data;
using CandidateDocuments.Tests.Integration.Fixtures;

namespace CandidateDocuments.Tests.Integration.Components
{
    [Collection(CollectionNames.DocumentsApp)]
    public class DocumentsComponent
    {
        private readonly TestAppFixture _testApp;
        private readonly string _apiKey;

        public DocumentsComponent(TestAppFixture testApp, TestSettingsFixture testSettings)
        {
            _testApp = testApp;
            _apiKey = testSettings.Configuration["ApiKey"];
        }

        public Dictionary<string, string> HeadersWithApiKey(Dictionary<string, string> headers = null)
        {
            if (headers == null) headers = new Dictionary<string, string>();
            headers.Add("Api-Key", _apiKey);
            return headers;
        }

        [Fact]
        public async void AnyRequestReturnsUnauthorizedtIfApiKeyHeaderNotProvided()
        {
            var response = await _testApp.MakeJsonRequest("/api/documents", "GET");
            Assert.Equal(response.StatusCode, HttpStatusCode.Unauthorized);
        }

        public class GetListRequest : DocumentsComponent
        {
            public GetListRequest(TestAppFixture testApp, TestSettingsFixture testSettings)
                : base(testApp, testSettings) { }

            [Fact]
            public async void ReturnsDocumentsListWithPaginationHeader()
            {
                var response = await _testApp.MakeJsonRequest("/api/documents", "GET",
                    HeadersWithApiKey(new Dictionary <string, string>
                        {{"CandidateId", DocumentsTestData.Documents[0].CandidateId.ToString()}}));
                response.EnsureSuccessStatusCode();
                Assert.True(response.Headers.Contains("Pagination"));

                var responseJson = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<IEnumerable<DocumentViewModel>>(responseJson);
                Assert.NotEmpty(result);
            }

            [Fact]
            public async void ReturnsBadRequestIfCandidateIdHeaderNotProvided()
            {
                var response = await _testApp.MakeJsonRequest("/api/documents", "GET", HeadersWithApiKey());
                Assert.Equal(response.StatusCode, HttpStatusCode.BadRequest);
            }
        }

        public class GetDocumentRequest : DocumentsComponent
        {
            public GetDocumentRequest(TestAppFixture testApp, TestSettingsFixture testSettings)
                : base(testApp, testSettings) { }

            [Fact]
            public async void ReturnsRequestedDocument()
            {
                var documentId = DocumentsTestData.Documents[1].Id;
                var response = await _testApp.MakeJsonRequest($"/api/documents/{documentId}", "GET", HeadersWithApiKey());
                response.EnsureSuccessStatusCode();

                var responseJson = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<DocumentViewModel>(responseJson);
                Assert.Equal(result.Id, documentId);
            }
        }

        public class PostDocumentRequest : DocumentsComponent
        {
            public PostDocumentRequest(TestAppFixture testApp, TestSettingsFixture testSettings)
                : base(testApp, testSettings) { }

            [Fact]
            public async void ReturnsCreatedDocument()
            {
                var response = await _testApp.MakeJsonRequest("/api/documents", "POST", HeadersWithApiKey(),
                    JsonConvert.SerializeObject(new DocumentSaveModel
                    {
                        CandidateId = Guid.NewGuid(),
                        Filename = "portfolio.pdf",
                        DocumentType = (int) DocumentType.Patent
                    }));
                response.EnsureSuccessStatusCode();

                var responseJson = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<DocumentViewModel>(responseJson);
                Assert.NotNull(result.Id);
                Assert.NotEqual(result.Id, Guid.Empty);
            }

            [Fact]
            public async void ReturnsBadRequestIfInvalidContent()
            {
                var response = await _testApp.MakeJsonRequest("/api/documents", "POST", HeadersWithApiKey(),
                    JsonConvert.SerializeObject(new DocumentSaveModel
                    {
                        ReviewerId = Guid.NewGuid(),                        
                    }));
                Assert.Equal(response.StatusCode, HttpStatusCode.BadRequest);
            }
        }

        public class PutDocumentRequest : DocumentsComponent
        {
            public PutDocumentRequest(TestAppFixture testApp, TestSettingsFixture testSettings)
                : base(testApp, testSettings) { }

            [Fact]
            public async void ReturnsUpdatedDocument()
            {
                var documentId = DocumentsTestData.Documents[1].Id;
                var response = await _testApp.MakeJsonRequest($"/api/documents/{documentId}", "PUT", HeadersWithApiKey(),
                    JsonConvert.SerializeObject(new DocumentSaveModel
                    {
                        CandidateId = Guid.NewGuid(),
                        Filename = "portfolio.pdf",
                        DocumentType = (int)DocumentType.Patent
                    }));
                response.EnsureSuccessStatusCode();

                var responseJson = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<DocumentViewModel>(responseJson);
                Assert.Equal(result.Id, documentId);
            }

            [Fact]
            public async void ReturnsBadRequestIfInvalidContent()
            {
                var documentId = DocumentsTestData.Documents[1].Id;
                var response = await _testApp.MakeJsonRequest($"/api/documents/{documentId}", "PUT", HeadersWithApiKey(),
                    JsonConvert.SerializeObject(new DocumentSaveModel
                    {
                        Filename = "",
                    }));
                Assert.Equal(response.StatusCode, HttpStatusCode.BadRequest);
            }
        }

        public class DeleteDocumentRequest : DocumentsComponent
        {
            public DeleteDocumentRequest(TestAppFixture testApp, TestSettingsFixture testSettings)
                : base(testApp, testSettings) { }

            [Fact]
            public async void ReturnsSuccessStatusCode()
            {
                var documentId = DocumentsTestData.Documents[1].Id;
                var response = await _testApp.MakeJsonRequest($"/api/documents/{documentId}", "DELETE", HeadersWithApiKey());
                response.EnsureSuccessStatusCode();
            }
        }
    }
}
