using CandidateDocuments.Application.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CandidateDocuments.Infrastructure.Data
{
    public abstract class BaseMap<TModel> where TModel : BaseModel
    {
        public abstract void Map(EntityTypeBuilder<TModel> b);

        public void Map(ModelBuilder modelBuilder)
        {
            var entity = modelBuilder.Entity<TModel>();
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            Map(entity);
        }
    }
}
