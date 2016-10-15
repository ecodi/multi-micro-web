using System;
using System.IO;

namespace CandidateDocuments.Application.Models
{
    public enum DocumentType
    {
        BusinessPlan = 0,
        License = 1,
        Patent = 2,
        Other = 3
    }

    public class Document : BaseModel
    {
        public Guid CandidateId { get; set; }
        public string Filename { get; set; }
        public string FileExtension {
            get { return Path.GetExtension(Filename ?? "").Replace(".", ""); }
            private set {}
        }
        public DocumentType DocumentType { get; set; }
        public DateTime CreationDate { get; private set; }
        public Guid? ReviewerId { get; set; }

        public Document()
        {
            CreationDate = DateTime.Now;
        }
    }
}
