using CalendarApp.API.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace CalendarApp.API.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<TaskEntity> Tasks { get; set; }
    public DbSet<TodoList> TodoLists { get; set; }
    public DbSet<TodoItem> TodoItems { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<CalendarShare> CalendarShares { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.Username).IsUnique();

            entity.HasMany(e => e.Tasks)
                .WithOne(e => e.User)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.TodoLists)
                .WithOne(e => e.User)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.RefreshTokens)
                .WithOne(e => e.User)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Task configuration
        modelBuilder.Entity<TaskEntity>(entity =>
        {
            entity.HasKey(e => e.TaskId);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.TaskDate);
            entity.HasIndex(e => e.Scope);

            entity.HasCheckConstraint(
                "CHK_Task_Scope",
                "[Scope] IN ('Day', 'Week', 'Month', 'Year')"
            );
        });

        // TodoList configuration
        modelBuilder.Entity<TodoList>(entity =>
        {
            entity.HasKey(e => e.TodoListId);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.ListDate);

            // Ignore the Items alias property
            entity.Ignore(e => e.Items);

            entity.HasMany(e => e.TodoItems)
                .WithOne(e => e.TodoList)
                .HasForeignKey(e => e.TodoListId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasCheckConstraint(
                "CHK_TodoList_Scope",
                "[Scope] IN ('Day', 'Week', 'Month', 'Year')"
            );
        });

        // TodoItem configuration
        modelBuilder.Entity<TodoItem>(entity =>
        {
            entity.HasKey(e => e.TodoItemId);
            entity.HasIndex(e => e.TodoListId);
        });

        // RefreshToken configuration
        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(e => e.TokenId);
            entity.HasIndex(e => e.Token).IsUnique();
            entity.HasIndex(e => e.UserId);
        });

        // CalendarShare configuration
        modelBuilder.Entity<CalendarShare>(entity =>
        {
            entity.HasKey(e => e.CalendarShareId);
            entity.HasIndex(e => e.OwnerId);
            entity.HasIndex(e => e.SpectatorEmail);
            entity.HasIndex(e => e.SpectatorUserId);

            entity.HasOne(e => e.Owner)
                .WithMany(u => u.CalendarShareOwners)
                .HasForeignKey(e => e.OwnerId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.SpectatorUser)
                .WithMany(u => u.CalendarShareSpectatorUsers)
                .HasForeignKey(e => e.SpectatorUserId)
                .OnDelete(DeleteBehavior.NoAction);
        });
    }
}
