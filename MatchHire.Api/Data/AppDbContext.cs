using MatchHire.Api.Auth;
using Microsoft.EntityFrameworkCore;
using MatchHire.Api.Jobs;

namespace MatchHire.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Job> Jobs => Set<Job>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(e =>
        {
            e.HasIndex(u => u.Email).IsUnique();
            e.Property(u => u.Email).HasMaxLength(256);
            e.Property(u => u.FirstName).HasMaxLength(100);
            e.Property(u => u.LastName).HasMaxLength(100);
            e.Property(u => u.UserType).HasConversion<string>().HasMaxLength(20);
        });

        modelBuilder.Entity<Job>(e =>
        {
            e.Property(j => j.Title).HasMaxLength(100);
            e.Property(j => j.Location).HasMaxLength(100);
            e.Property(j => j.Description).HasMaxLength(2000);
            e.Property(j => j.JobType).HasConversion<string>().HasMaxLength(20);
            e.Property(j => j.MinSalary).HasPrecision(12, 2);
            e.Property(j => j.MaxSalary).HasPrecision(12, 2);

            // Foreign key to Users
            e.HasOne<User>()
                .WithMany()
                .HasForeignKey(j => j.EmployerId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasIndex(j => j.EmployerId);
        });
    }
}