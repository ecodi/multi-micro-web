using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CandidateDocuments.Application.Models;
using Microsoft.EntityFrameworkCore;

namespace CandidateDocuments.Infrastructure.Data
{
    public class DocumentMap : BaseMap<Document>
    {
        public override void Map(EntityTypeBuilder<Document> entity)
        {
            entity.ToTable("Document");
            entity.Property(s => s.Filename).HasMaxLength(200).IsRequired();
            entity.Property(s => s.FileExtension).HasMaxLength(5).IsRequired();
            entity.Property(s => s.DocumentType).IsRequired();
            entity.Property(s => s.CreationDate).ValueGeneratedOnAdd();
        }
    }
}