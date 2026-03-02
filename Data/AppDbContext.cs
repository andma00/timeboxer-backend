using Microsoft.EntityFrameworkCore;
using Timebox.Models;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    public DbSet<Timebox.Models.Task> Tasks { get; set; }
    public DbSet<Goal> Goals { get; set; }
    public DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure one to many relationship between Task and Goals
        modelBuilder.Entity<Timebox.Models.Task>()
        .HasMany(t => t.Goals)
        .WithOne(g => g.Task)
                .HasForeignKey(g => g.TaskId)
                .OnDelete(DeleteBehavior.Cascade);

        // Configure one to many relationship between User and Tasks
        modelBuilder.Entity<User>()
                        .HasMany(u => u.Tasks)
                        .WithOne(t => t.User)
                        .HasForeignKey(t => t.UserId)
                        .OnDelete(DeleteBehavior.Cascade);

    }
}

