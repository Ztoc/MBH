using MBH.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace MBH.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<ScheduledTask> ScheduledTasks => Set<ScheduledTask>();

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder mb)
        {
            var e = mb.Entity<ScheduledTask>();
            e.HasIndex(x => new { x.IsExecuted, x.ScheduledTime });

            var converter = new ValueConverter<DateTime, DateTime>(
                v => DateTime.SpecifyKind(v, DateTimeKind.Utc),
                v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

            var nullableConverter = new ValueConverter<DateTime?, DateTime?>(
                v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v,
                v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v);

            foreach (var prop in mb.Model.GetEntityTypes()
                     .SelectMany(t => t.GetProperties()))
            {
                if (prop.ClrType == typeof(DateTime)) prop.SetValueConverter(converter);
                if (prop.ClrType == typeof(DateTime?)) prop.SetValueConverter(nullableConverter);
            }
        }

        public override Task<int> SaveChangesAsync(CancellationToken ct = default)
        {
            var now = DateTime.UtcNow;
            foreach (var e in ChangeTracker.Entries<ScheduledTask>())
            {
                if (e.State == EntityState.Added)
                {
                    e.Entity.CreatedAt = now;
                    e.Entity.UpdatedAt = now;
                }
                if (e.State == EntityState.Modified)
                {
                    e.Entity.UpdatedAt = now;
                }
            }
            return base.SaveChangesAsync(ct);
        }
    }
}
