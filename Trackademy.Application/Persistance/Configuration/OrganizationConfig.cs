using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Trackademy.Domain.Users;

namespace Trackademy.Application.Persistance.Configuration;

public class OrganizationConfig : IEntityTypeConfiguration<Organization>
{
    public void Configure(EntityTypeBuilder<Organization> b)
    {
        b.HasKey(x => x.Id);
        
        b.Property(x => x.Name).IsRequired();
        b.Property(x => x.Address).IsRequired();
        b.Property(x => x.Phone).IsRequired();

        b.HasIndex(x => x.Address).IsUnique();
    }
}