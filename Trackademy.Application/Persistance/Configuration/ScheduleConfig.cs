using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Trackademy.Domain.Users;

namespace Trackademy.Application.Persistance.Configuration;

public class ScheduleConfig : IEntityTypeConfiguration<Domain.Users.Schedule>
{
    public void Configure(EntityTypeBuilder<Domain.Users.Schedule> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.StartTime).IsRequired();
        b.Property(x => x.EndTime).IsRequired();
        
        b.HasIndex(x => x.GroupId); 
        b.HasIndex(x => x.TeacherId);
        b.HasIndex(x => x.RoomId);
        b.HasCheckConstraint("CK_Schedule_Time", "\"StartTime\" < \"EndTime\"");

        b.Property(x => x.DaysOfWeek).HasColumnType("integer[]");

        b.Property(x => x.EffectiveFrom).HasColumnType("date");
        b.Property(x => x.EffectiveTo).HasColumnType("date");

        b.HasOne(x => x.Group)
            .WithMany(g => g.Schedules)
            .HasForeignKey(x => x.GroupId);
        
        b.HasOne(x => x.Teacher)
            .WithMany(u => u.Schedules)
            .HasForeignKey(x => x.TeacherId);
        
        b.HasOne(x => x.Room)
            .WithMany(r => r.Schedules)
            .HasForeignKey(x => x.RoomId);
    }
}