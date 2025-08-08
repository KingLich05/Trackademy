using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Trackademy.Domain.Users;

namespace Trackademy.Application.Persistance.Configuration;

public class ScoreConfig : IEntityTypeConfiguration<Score>
{
    public void Configure(EntityTypeBuilder<Score> b)
    {
        b.HasKey(x => x.Id);

        b.HasOne(x => x.Submission)
            .WithMany(s => s.Scores)
            .HasForeignKey(x => x.SubmissionId);
    }
}