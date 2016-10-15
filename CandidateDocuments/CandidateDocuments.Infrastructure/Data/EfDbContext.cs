using System;
using System.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL;
using CandidateDocuments.Application.Core;
using CandidateDocuments.Application.Models;

namespace CandidateDocuments.Infrastructure.Data
{
    /// <summary>
    /// Entity Framework based database context.
    /// </summary>
    public class EfDbContext : DbContext
    {
        public EfDbContext(DbContextOptions options) : base(options)
        {            
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.HasPostgresExtension("uuid-ossp");
            ConfigureMappings(modelBuilder);
        }

        /// <summary>
        /// Responsible for registering mappings between domain models and database entities.
        /// </summary>
        protected void ConfigureMappings(ModelBuilder modelBuilder)
        {
            modelBuilder.AddMapping<Document, DocumentMap>();
        }

        public override int SaveChanges()
        {
            int result = 0;
            try
            {
                result = base.SaveChanges();
            }
            catch (DbUpdateException ex)
            {
                HandleSaveChangesError(ex);
            }
            return result;
        }

        protected void HandleSaveChangesError(DbUpdateException exception)
        {
            if (exception.InnerException?.InnerException == null) throw new SaveDataError(exception.Message);
            var sqlException = (SqlException) exception.InnerException.InnerException;
            if (sqlException.Number == 2627 || sqlException.Number == 547 || sqlException.Number == 2601)
            {
                throw new UniqnessViolation(exception.Message);
            }
            throw new SaveDataError(exception.Message);
        }
    }

    public static class ModelBuilderExtenions
    {
        public static void AddMapping<TModel, TMap>(this ModelBuilder modelBuilder) where TModel : BaseModel where TMap : BaseMap<TModel>
        {
            var config = (TMap)Activator.CreateInstance(typeof(TMap));
            config.Map(modelBuilder);
        }
    }
}
