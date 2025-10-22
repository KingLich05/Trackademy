using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Trackademy.Domain.Enums;
using Trackademy.Domain.Users;

namespace Trackademy.Application.Persistance.Configuration;

public class ScoreConfig : IEntityTypeConfiguration<Score>
{
    public void Configure(EntityTypeBuilder<Score> builder)
    {
        builder.ToTable("Scores");
        
        builder.HasKey(s => s.Id);
        
        // Relationships
        builder.HasOne(s => s.Submission)
            .WithMany(sub => sub.Scores)
            .HasForeignKey(s => s.SubmissionId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasOne(s => s.Teacher)
            .WithMany()
            .HasForeignKey(s => s.TeacherId)
            .OnDelete(DeleteBehavior.Restrict);
        
        // Self-referencing relationship for version history
        builder.HasOne(s => s.PreviousVersion)
            .WithMany(s => s.NextVersions)
            .HasForeignKey(s => s.PreviousVersionId)
            .OnDelete(DeleteBehavior.Restrict);
        
        // Properties        
        builder.Property(s => s.Status)
            .HasConversion<int>()
            .IsRequired();
        
        builder.Property(s => s.Feedback)
            .HasMaxLength(2000);
        
        builder.Property(s => s.MaxPoints)
            .IsRequired();
        
        builder.Property(s => s.Version)
            .IsRequired();
        
        builder.Property(s => s.AwardedAt)
            .IsRequired();
        
        builder.Property(s => s.UpdatedAt)
            .IsRequired();
        
        // Indexes
        builder.HasIndex(s => s.SubmissionId)
            .HasDatabaseName("IX_Scores_SubmissionId");
        
        builder.HasIndex(s => s.TeacherId)
            .HasDatabaseName("IX_Scores_TeacherId");
        
        builder.HasIndex(s => new { s.SubmissionId, s.Status })
            .HasDatabaseName("IX_Scores_SubmissionId_Status");
        
        builder.HasIndex(s => s.AwardedAt)
            .HasDatabaseName("IX_Scores_AwardedAt");
        
        // Check constraints
        builder.ToTable(t => t.HasCheckConstraint(
            "CK_Scores_NumericValue_Range", 
            "\"NumericValue\" IS NULL OR (\"NumericValue\" >= 0 AND \"NumericValue\" <= \"MaxPoints\")"
        ));
        
        builder.ToTable(t => t.HasCheckConstraint(
            "CK_Scores_MaxPoints_Positive", 
            "\"MaxPoints\" > 0"
        ));
        
        builder.ToTable(t => t.HasCheckConstraint(
            "CK_Scores_Version_Positive", 
            "\"Version\" > 0"
        ));
    }
}