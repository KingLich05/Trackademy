using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Trackademy.Domain.Users;

namespace Trackademy.Application.Persistance.Configuration;

public class AssignmentConfig : IEntityTypeConfiguration<Assignment>
{
    public void Configure(EntityTypeBuilder<Assignment> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.Description).HasMaxLength(800);

        b.HasOne(x => x.Subject)
            .WithMany(s => s.Assignments)
            .HasForeignKey(x => x.SubjectId);
    }
}
