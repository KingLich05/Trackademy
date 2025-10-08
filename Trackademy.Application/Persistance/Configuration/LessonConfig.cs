using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Trackademy.Domain.Users;

namespace Trackademy.Application.Persistance.Configuration;

public class LessonConfig : IEntityTypeConfiguration<Lesson>
{
    public void Configure(EntityTypeBuilder<Lesson> b)
    {
        b.HasKey(x => x.Id);

        b.Property(x => x.Date).HasColumnType("date");

        b.HasIndex(x => new { x.Date, x.GroupId });
        b.HasIndex(x => new { x.Date, x.TeacherId });
        b.HasIndex(x => new { x.Date, x.RoomId });
        b.HasIndex(x => new { x.Date, x.RoomId, x.StartTime, x.EndTime });

        b.HasOne(x => x.Schedule)
            .WithMany()
            .HasForeignKey(x => x.ScheduleId)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasOne(x => x.Group).WithMany().HasForeignKey(x => x.GroupId);
        b.HasOne(x => x.Teacher).WithMany().HasForeignKey(x => x.TeacherId);
        b.HasOne(x => x.Room).WithMany().HasForeignKey(x => x.RoomId);
    }
}