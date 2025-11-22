using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Trackademy.Domain.Users;

namespace Trackademy.Application.Persistance.Configuration;

public class RoomConfig : IEntityTypeConfiguration<Room>
{
    public void Configure(EntityTypeBuilder<Room> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.Name).IsRequired();
        b.Property(x => x.Capacity).IsRequired();
        
        // Уникальность имени только в рамках организации
        b.HasIndex(x => new { x.Name, x.OrganizationId })
            .IsUnique();
        
        b.HasOne(r => r.Organization)
            .WithMany(o => o.Rooms)
            .HasForeignKey(u => u.OrganizationId);
    }
}
