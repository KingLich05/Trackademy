using Microsoft.EntityFrameworkCore;
using Trackademy.Application.Persistance.Configuration;
using Trackademy.Domain.Users;

namespace Trackademy.Application.Persistance;

public class TrackademyDbContext : DbContext
{
    public TrackademyDbContext(DbContextOptions<TrackademyDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Organization> Organizations { get; set; }

    public DbSet<Groups> Groups { get; set; }
    public DbSet<Subject> Subjects { get; set; }
    public DbSet<Room> Rooms { get; set; }
    public DbSet<Domain.Users.Schedule> Schedules { get; set; }
    public DbSet<Attendance> Attendances { get; set; }
    public DbSet<Assignment> Assignments { get; set; }
    public DbSet<Submission> Submissions { get; set; }
    public DbSet<SubmissionFile> SubmissionFiles { get; set; }
    public DbSet<Score> Scores { get; set; }
    public DbSet<Payment> Payments { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<Lesson> Lessons { get; set; }
    public DbSet<GroupStudent> GroupStudents { get; set; }
    public DbSet<Material> Materials { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new UserConfig());
        modelBuilder.ApplyConfiguration(new GroupConfig());
        modelBuilder.ApplyConfiguration(new SubjectConfig());
        modelBuilder.ApplyConfiguration(new RoomConfig());
        modelBuilder.ApplyConfiguration(new ScheduleConfig());
        modelBuilder.ApplyConfiguration(new AttendanceConfig());
        modelBuilder.ApplyConfiguration(new AssignmentConfig());
        modelBuilder.ApplyConfiguration(new SubmissionConfig());
        modelBuilder.ApplyConfiguration(new SubmissionFileConfig());
        modelBuilder.ApplyConfiguration(new ScoreConfig());
        modelBuilder.ApplyConfiguration(new PaymentConfig());
        modelBuilder.ApplyConfiguration(new NotificationConfig());
        modelBuilder.ApplyConfiguration(new OrganizationConfig());
        modelBuilder.ApplyConfiguration(new LessonConfig());
        modelBuilder.ApplyConfiguration(new GroupStudentConfig());
        modelBuilder.ApplyConfiguration(new MaterialConfig());
    }
}