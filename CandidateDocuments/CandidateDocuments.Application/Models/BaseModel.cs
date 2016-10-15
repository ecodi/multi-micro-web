using System;

namespace CandidateDocuments.Application.Models
{
    public abstract class BaseModel
    {
        public Guid Id { get; set; }

        protected BaseModel()
        {
            Id = Guid.NewGuid();
        }
    }
}
