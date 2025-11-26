using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Trackademy.Domain.Users;

namespace Trackademy.Application.Persistance.Configuration;

public class MaterialConfig : IEntityTypeConfiguration<Material>
{
    public void Configure(EntityTypeBuilder<Material> b)
    {
        b.HasKey(x => x.Id);
        
        b.Property(x => x.Title)
            .IsRequired()
            .HasMaxLength(255);
            
        b.Property(x => x.Description)
            .HasMaxLength(1000);
            
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
            
        b.Property(x => x.CreatedAt)
            .IsRequired();
            
        b.Property(x => x.UpdatedAt)
            .IsRequired();
        
        // Relationships
        b.HasOne(x => x.Group)
            .WithMany()
            .HasForeignKey(x => x.GroupId)
            .OnDelete(DeleteBehavior.Cascade);
            
        b.HasOne(x => x.UploadedBy)
            .WithMany()
            .HasForeignKey(x => x.UploadedById)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
