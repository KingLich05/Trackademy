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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Username).IsRequired().HasMaxLength(100);
        });

        base.OnModelCreating(modelBuilder);
    }
}