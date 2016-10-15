using System;
using System.Collections.Generic;
using CandidateDocuments.Application.Models;

namespace CandidateDocuments.Tests.Integration.Data
{
    public static class DocumentsTestData
    {
        public static List<Document> Documents = new List<Document>
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
}
