using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Trackademy.Domain.Users;

namespace Trackademy.Application.Persistance.Configuration;

public class SubjectConfig : IEntityTypeConfiguration<Subject>
{
    public void Configure(EntityTypeBuilder<Subject> b)
    {;
        b.HasKey(x => x.Id);
        b.Property(x => x.Name).IsRequired();
        
        b.HasOne(u => u.Organization)
            .WithMany(o => o.Subjects)
            .HasForeignKey(u => u.OrganizationId);
    }
}