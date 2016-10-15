using System;
using System.ComponentModel.DataAnnotations;
using AutoMapper;
using CandidateDocuments.Application.Models;

namespace CandidateDocuments.API.ViewModels
{
    public class DocumentViewModel : DocumentSaveModel
    {
        [Required]
        public Guid Id { get; set; }
        [Required]
        public string FileExtension { get; set; }
        [Required]
        public DateTime CreationDate { get; set; }
    }

    public class DocumentSaveModel
    {
        [Required]
        public Guid CandidateId { get; set; }
        [Required]
        [MaxLength(200)]
        public string Filename { get; set; }
        [Required]
        [EnumDataType(typeof(DocumentType))]
        public int DocumentType { get; set; }
        public Guid? ReviewerId { get; set; }
    }

    public class DocumentModelMap : Profile
    {
        protected override void Configure()
        {
            Mapper.CreateMap<Document, DocumentViewModel>();
            Mapper.CreateMap<DocumentSaveModel, Document>().IgnoreAllPropertiesWithAnInaccessibleSetter();
        }
    }
}
