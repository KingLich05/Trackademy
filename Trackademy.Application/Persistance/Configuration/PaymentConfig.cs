using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Trackademy.Domain.Users;

namespace Trackademy.Application.Persistance.Configuration;

public class PaymentConfig : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> b)
    {
        b.HasKey(x => x.Id);
     
        b.HasIndex(x => new { x.StudentId, x.DueDate });

        b.HasOne(x => x.Student)
            .WithMany(u => u.Payments)
            .HasForeignKey(x => x.StudentId);
    }
}