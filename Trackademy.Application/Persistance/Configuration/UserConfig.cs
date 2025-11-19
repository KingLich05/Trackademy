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
        b.Property(x => x.Login).IsRequired();
        b.Property(x => x.PasswordHash).IsRequired();
        b.Property(x => x.CreatedDate).HasColumnType("timestamptz");

        // Уникальность логина в рамках организации
        b.HasIndex(x => new { x.Login, x.OrganizationId }).IsUnique();

        b.HasMany(u => u.Groups).WithMany(g => g.Students);
        
        b.HasOne(u => u.Organization)
            .WithMany(o => o.Users)
            .HasForeignKey(u => u.OrganizationId);
    }
}