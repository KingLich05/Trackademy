using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Trackademy.Domain.Users;

namespace Trackademy.Application.Persistance.Configuration;

public class UserConfig: IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.FullName).IsRequired();
        b.Property(x => x.Email).IsRequired();
        b.Property(x => x.PasswordHash).IsRequired();
        b.Property(x => x.CreatedDate).HasColumnType("timestamptz");

        b.HasIndex(x => x.Email).IsUnique();

        b.HasMany(u => u.Groups).WithMany(g => g.Students);
        
        b.HasOne(u => u.Organizations) // у User одна организация
            .WithMany(o => o.Users)      // у Organization много Users
            .HasForeignKey(u => u.OrganizationId);
    }
}