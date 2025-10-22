using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Trackademy.Domain.Enums;
using Trackademy.Domain.Users;

namespace Trackademy.Application.Persistance.Configuration;

public class SubmissionConfig : IEntityTypeConfiguration<Submission>
{
    public void Configure(EntityTypeBuilder<Submission> b)
    {
        b.HasKey(x => x.Id);
        
        // Properties
        b.Property(x => x.TextContent)
            .HasMaxLength(10000); // Ограничение на длину текста
            
        b.Property(x => x.TeacherComment)
            .HasMaxLength(2000);
            
        b.Property(x => x.Status)
            .IsRequired()
            .HasDefaultValue(SubmissionStatus.Draft);
            
        b.Property(x => x.CreatedAt)
            .IsRequired();
            
        b.Property(x => x.UpdatedAt)
            .IsRequired();
        
        // Relationships
        b.HasOne(x => x.Assignment)
            .WithMany(a => a.Submissions)
            .HasForeignKey(x => x.AssignmentId)
            .OnDelete(DeleteBehavior.Cascade);
     
        b.HasOne(x => x.Student)
            .WithMany(u => u.Submissions)
            .HasForeignKey(x => x.StudentId)
            .OnDelete(DeleteBehavior.Cascade);
            
        // Indexes
        b.HasIndex(x => x.AssignmentId);
        b.HasIndex(x => x.StudentId);
        b.HasIndex(x => new { x.AssignmentId, x.StudentId }).IsUnique(); // Один submission на студента на задание
    }
}