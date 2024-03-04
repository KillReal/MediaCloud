using MediaCloud.Data.Models;
using MediaCloud.WebApp;
using Microsoft.EntityFrameworkCore;

namespace MediaCloud.Data
{
    public class AppDbContext : DbContext
    {
        private readonly string _superAdminHash = "h5KPDjrv8910000$jy3+sU1D7rHyYTPdyM+UTifqHFdzTBe3zkZQugE6JhvSRpBW";

        public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
        {
            if (Database.EnsureCreated())
            {
                var admin = new Actor()
                {
                    Name = "Admin",
                    PasswordHash = _superAdminHash,
                    IsActivated = true,
                    IsAdmin = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                };

                Actors.Add(admin);
                SaveChangesAsync();
            }
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseLazyLoadingProxies();
        }

        public override int SaveChanges()
        {
            var entries = ChangeTracker.Entries().Where(e => e.Entity is Record && (
                                                             e.State == EntityState.Added ||
                                                             e.State == EntityState.Modified));

            foreach (var entityEntry in entries)
            {
                ((Record)entityEntry.Entity).UpdatedAt = DateTime.Now.ToUniversalTime();

                if (entityEntry.State == EntityState.Added)
                {
                    ((Record)entityEntry.Entity).CreatedAt = DateTime.Now.ToUniversalTime();
                }
            }

            return base.SaveChanges();
        }

        public DbSet<Actor> Actors { get; set; } = null!;
        public DbSet<Preview> Previews { get; set; } = null!;
        public DbSet<Media> Medias { get; set; } = null!;
        public DbSet<Tag> Tags { get; set; } = null!;
        public DbSet<Collection> Collections { get; set; } = null!;
        public DbSet<StatisticSnapshot> StatisticSnapshots { get; set; } = null!;
    }
}
