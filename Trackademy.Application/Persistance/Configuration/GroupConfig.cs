
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Trackademy.Domain.Users;

namespace Trackademy.Application.Persistance.Configuration;

public class GroupConfig : IEntityTypeConfiguration<Groups>
{
    public void Configure(EntityTypeBuilder<Groups> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.Name).IsRequired();
        b.Property(x => x.CreatedAt).HasColumnType("timestamptz");

        b.HasMany(g => g.Subjects)
            .WithMany(s => s.Groups);
    }
}
