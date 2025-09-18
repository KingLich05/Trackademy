using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Trackademy.Domain.Users;

namespace Trackademy.Application.Persistance.Configuration;

public class AttendanceConfig : IEntityTypeConfiguration<Attendance>
{
    public void Configure(EntityTypeBuilder<Attendance> b)
    {
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.LessonId, x.StudentId }).IsUnique();
        b.Property(x => x.Date).HasColumnType("date");

        b.HasOne(x => x.Student)
            .WithMany(u => u.Attendances)
            .HasForeignKey(x => x.StudentId);
        
        b.HasOne(x => x.Lesson)
            .WithMany(l => l.Attendances)
            .HasForeignKey(x => x.LessonId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}