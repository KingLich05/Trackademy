
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Trackademy.Domain.Users;

namespace Trackademy.Application.Persistance.Configuration;

public class GroupConfig : IEntityTypeConfiguration<Groups>
{
    public void Configure(EntityTypeBuilder<Groups> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(200);
        
        b.Property(x => x.Code)
            .IsRequired()
            .HasMaxLength(50);
        
        b.Property(x => x.CreatedAt)
            .HasColumnType("timestamptz");

        b.HasIndex(x => x.OrganizationId);
        b.HasIndex(x => x.Code).IsUnique();

        b.HasOne(g => g.Subject)
            .WithMany(s => s.Groups)
            .HasForeignKey(g => g.SubjectId)
            .OnDelete(DeleteBehavior.Restrict);
        
        
        b.HasOne(g => g.Organization)
            .WithMany(o => o.Groups)
            .HasForeignKey(g => g.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);
        
        b.HasMany(g => g.Schedules)
            .WithOne(s => s.Group)
            .HasForeignKey(s => s.GroupId)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasMany(g => g.Students)
           .WithMany(u => u.Groups)
           .UsingEntity<GroupStudent>(
               j => j.HasOne(gs => gs.Student)
                   .WithMany()
                   .HasForeignKey(gs => gs.StudentId),
               j => j.HasOne(gs => gs.Group)
                   .WithMany(g => g.GroupStudents)
                   .HasForeignKey(gs => gs.GroupId)
           );
    }
}