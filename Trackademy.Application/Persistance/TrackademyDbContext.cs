using Microsoft.EntityFrameworkCore;
using Trackademy.Domain.Users;

namespace Trackademy.Application.Persistance;

public class TrackademyDbContext : DbContext
{
    public TrackademyDbContext(DbContextOptions<TrackademyDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; }

    public DbSet<Domain.Users.Roles> Roles { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Domain.Users.Roles>(entity =>
        {
            entity.HasKey(x => x.Id);
        });
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.HasOne(e => e.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(e => e.RoleId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        base.OnModelCreating(modelBuilder);
    }
}