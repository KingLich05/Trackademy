using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Trackademy.Domain.Users;

namespace Trackademy.Application.Persistance.Configuration;

public class GroupStudentConfig : IEntityTypeConfiguration<GroupStudent>
{
    public void Configure(EntityTypeBuilder<GroupStudent> builder)
    {
        builder.ToTable("GroupStudents");

        builder.HasKey(gs => gs.Id);

        // Composite index для уникальности пары GroupId-StudentId
        builder.HasIndex(gs => new { gs.GroupId, gs.StudentId })
            .IsUnique();

        builder.Property(gs => gs.DiscountValue)
            .HasPrecision(18, 2);

        builder.Property(gs => gs.DiscountReason)
            .HasMaxLength(500);

        builder.Property(gs => gs.JoinedAt)
            .IsRequired();

        // Связи
        builder.HasOne(gs => gs.Group)
            .WithMany(g => g.GroupStudents)
            .HasForeignKey(gs => gs.GroupId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(gs => gs.Student)
            .WithMany()
            .HasForeignKey(gs => gs.StudentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
