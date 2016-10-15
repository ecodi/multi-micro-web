using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using CandidateDocuments.Infrastructure.Data;

namespace CandidateDocuments.Migrations.SqlServer.Migrations
{
    [DbContext(typeof(EfDbContext))]
    partial class EfDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "1.0.1")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("CandidateDocuments.Application.Models.Document", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<Guid>("CandidateId");

                    b.Property<DateTime>("CreationDate")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("DocumentType");

                    b.Property<string>("FileExtension")
                        .IsRequired()
                        .HasAnnotation("MaxLength", 5);

                    b.Property<string>("Filename")
                        .IsRequired()
                        .HasAnnotation("MaxLength", 200);

                    b.Property<Guid?>("ReviewerId");

                    b.HasKey("Id");

                    b.ToTable("Document");
                });
        }
    }
}
