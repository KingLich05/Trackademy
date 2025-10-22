using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Trackademy.Domain.Users;

namespace Trackademy.Application.Persistance.Configuration;

public class AssignmentConfig : IEntityTypeConfiguration<Assignment>
{
    public void Configure(EntityTypeBuilder<Assignment> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.Description).HasMaxLength(1000);

        b.HasOne(x => x.Group)
            .WithMany(g => g.Assignments)
            .HasForeignKey(x => x.GroupId);
    }
}
