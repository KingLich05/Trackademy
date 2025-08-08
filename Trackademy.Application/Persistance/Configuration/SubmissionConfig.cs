using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Trackademy.Domain.Users;

namespace Trackademy.Application.Persistance.Configuration;

public class SubmissionConfig : IEntityTypeConfiguration<Submission>
{
    public void Configure(EntityTypeBuilder<Submission> b)
    {
        b.HasKey(x => x.Id);
     
        b.HasOne(x => x.Assignment)
            .WithMany(a => a.Submissions).HasForeignKey(x => x.AssignmentId);
     
        b.HasOne(x => x.Student)
            .WithMany(u => u.Submissions)
            .HasForeignKey(x => x.StudentId);
    }
}