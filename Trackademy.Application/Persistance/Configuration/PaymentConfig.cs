using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Trackademy.Domain.Users;

namespace Trackademy.Application.Persistance.Configuration;

public class PaymentConfig : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> b)
    {
        b.HasKey(x => x.Id);
     
        b.HasIndex(x => new { x.StudentId, x.PeriodEnd });

        b.HasOne(x => x.Student)
            .WithMany(u => u.Payments)
            .HasForeignKey(x => x.StudentId);

        b.HasOne(x => x.Group)
            .WithMany(g => g.Payments)
            .HasForeignKey(x => x.GroupId);

        b.Property(x => x.PaymentPeriod)
            .HasMaxLength(500)
            .IsRequired();

        b.Property(x => x.OriginalAmount)
            .HasPrecision(18, 2);

        b.Property(x => x.DiscountValue)
            .HasPrecision(18, 2);

        b.Property(x => x.Amount)
            .HasPrecision(18, 2);

        b.Property(x => x.DiscountReason)
            .HasMaxLength(200);

        b.Property(x => x.CancelReason)
            .HasMaxLength(500);
    }
}