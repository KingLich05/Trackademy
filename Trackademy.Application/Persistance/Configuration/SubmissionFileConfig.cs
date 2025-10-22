using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Trackademy.Domain.Users;

namespace Trackademy.Application.Persistance.Configuration;

public class SubmissionFileConfig : IEntityTypeConfiguration<SubmissionFile>
{
    public void Configure(EntityTypeBuilder<SubmissionFile> b)
    {
        b.HasKey(x => x.Id);
        
        b.Property(x => x.OriginalFileName)
            .IsRequired()
            .HasMaxLength(255);
            
        b.Property(x => x.StoredFileName)
            .IsRequired()
            .HasMaxLength(255);
            
        b.Property(x => x.ContentType)
            .IsRequired()
            .HasMaxLength(100);
            
        b.Property(x => x.FilePath)
            .IsRequired()
            .HasMaxLength(500);
            
        b.Property(x => x.FileSize)
            .IsRequired();
            
        b.Property(x => x.IsImage)
            .IsRequired()
            .HasDefaultValue(false);
        
        // Relationships
        b.HasOne(x => x.Submission)
            .WithMany(s => s.Files)
            .HasForeignKey(x => x.SubmissionId)
            .OnDelete(DeleteBehavior.Cascade);
            
        // Indexes
        b.HasIndex(x => x.SubmissionId);
        b.HasIndex(x => x.StoredFileName).IsUnique();
    }
}