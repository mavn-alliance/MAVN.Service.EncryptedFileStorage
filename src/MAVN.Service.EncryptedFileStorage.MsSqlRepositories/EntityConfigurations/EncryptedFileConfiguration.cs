using MAVN.Service.EncryptedFileStorage.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MAVN.Service.EncryptedFileStorage.MsSqlRepositories.EntityConfigurations
{
    public class EncryptedFileConfiguration : IEntityTypeConfiguration<EncryptedFile>
    {
        public void Configure(EntityTypeBuilder<EncryptedFile> builder)
        {
            builder.ToTable("encrypted_files");

            builder.HasKey(x => x.FileId);

            builder.Property(x => x.FileId).ValueGeneratedOnAdd();
            builder.Property(x => x.Origin).IsRequired().HasMaxLength(63);
            builder.Property(x => x.FileName).IsRequired().HasMaxLength(255);
            builder.Property(x => x.BlobName).HasMaxLength(1024);
            builder.Property(x => x.IsUploadCompleted).IsRequired().HasDefaultValue(false);

            builder.Ignore(x => x.Content);

            builder.HasIndex(entity => new { entity.Origin, entity.FileName }).IsUnique();
        }
    }
}
