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
        
        b.HasOne(r => r.Organization)
            .WithMany(o => o.Rooms)
            .HasForeignKey(u => u.OrganizationId);
    }
}
