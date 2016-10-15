using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AutoMapper;
using CandidateDocuments.Application.Models;
using CandidateDocuments.Application.Services;
using CandidateDocuments.API.ViewModels;
using CandidateDocuments.API.Core;
using CandidateDocuments.API.Core.Authorization;
using CandidateDocuments.API.Core.Pagination;

namespace CandidateDocuments.API.Controllers
{
    [Route("api/[controller]")]
    [Authorize(Policy= "AnyApiKey")]
    [ApiKeyHeader]
    public class DocumentsController : BaseController
    {
        private readonly IDocumentsService _documentsService;

        public DocumentsController(IDocumentsService documentsService)
        {
            _documentsService = documentsService;
        }

        [HttpGet("")]
        [PaginationActionFilter]
        [ProducesResponseType(typeof(IEnumerable<DocumentViewModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(Dictionary<string, string>), (int)HttpStatusCode.BadRequest)]
        public IActionResult GetList([FromHeader(Name = "CandidateId"), Required]string candidateIdStr)
        {
            Guid candidateId;
            Guid.TryParse(candidateIdStr, out candidateId);
            if (candidateId == Guid.Empty) return BadRequest(new Dictionary<string, string> { { "CandidateId", ResponseMessageCodes.EmptyRequiredAttribute} });

            var totalDocuments = _documentsService.GetDocuments(candidateId).Count();
            PaginationHeader.TotalItems = totalDocuments;

            var documents = _documentsService.GetDocuments(candidateId, PaginationHeader.Offset, PaginationHeader.ItemsPerPage).ToList();
            var documentsVm = Mapper.Map<IEnumerable<Document>, IEnumerable<DocumentViewModel>>(documents);
            return new OkObjectResult(documentsVm);
        }

        [HttpGet("{id}", Name = "GetDocument")]
        [ProducesResponseType(typeof(DocumentViewModel), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(Dictionary<string, string>), (int)HttpStatusCode.NotFound)]
        public IActionResult Get(Guid id)
        {
            var document = _documentsService.GetDocumentById(id);
            if (document == null) return new NotFoundObjectResult(new {id});
            var documentVm = Mapper.Map<Document, DocumentViewModel>(document);
            return new OkObjectResult(documentVm);
        }

        [HttpPost("")]
        [ProducesResponseType(typeof(DocumentViewModel), (int)HttpStatusCode.Created)]
        [ProducesResponseType(typeof(ModelStateDictionary), (int)HttpStatusCode.BadRequest)]
        public IActionResult Create([FromBody]DocumentSaveModel documentSm)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var document = Mapper.Map<DocumentSaveModel, Document>(documentSm);
            _documentsService.AddDocument(document);

            var documentVm = Mapper.Map<Document, DocumentViewModel>(document);
            return CreatedAtRoute("GetDocument", new { controller = "Documents", id = document.Id }, documentVm);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(typeof(DocumentViewModel), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ModelStateDictionary), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(Dictionary<string, string>), (int)HttpStatusCode.NotFound)]
        public IActionResult Update(Guid id, [FromBody]DocumentSaveModel documentSm)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var document = _documentsService.GetDocumentById(id);
            if (document == null) return new NotFoundObjectResult(new { id });

            document = Mapper.Map(documentSm, document);
            _documentsService.UpdateDocument(document);

            var documentVm = Mapper.Map<Document, DocumentViewModel>(document);
            return new OkObjectResult(documentVm);
        }

        [HttpDelete("{id}", Name = "RemoveDocument")]
        [ProducesResponseType(typeof(void), (int)HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(Dictionary<string, string>), (int)HttpStatusCode.NotFound)]
        public IActionResult Delete(Guid id)
        {
            var document = _documentsService.GetDocumentById(id);
            if (document == null) return new NotFoundObjectResult(new { id });

            _documentsService.DeleteDocument(document);

            return new NoContentResult();
        }
    }
}
