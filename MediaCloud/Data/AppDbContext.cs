using MediaCloud.Data.Models;
using Microsoft.EntityFrameworkCore;
using NLog;

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
                var admin = new User()
                {
                    Name = "Admin",
                    PasswordHash = _superAdminHash,
                    IsActivated = true,
                    IsAdmin = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                };

                Users.Add(admin);
                SaveChangesAsync();

                LogManager.GetLogger("AppDbContext").Warn("Initial admin had inserted");
            }

            LogManager.GetLogger("AppDbContext").Debug("Initialized AppDbContext");
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

        public long GetDbSize()
        {
            using var command = Database.GetDbConnection().CreateCommand();
            var databaseSize = Database.GetDbConnection().Database;

            command.CommandText = $"SELECT pg_database_size('{databaseSize}');";
            Database.OpenConnection();

            using var reader = command.ExecuteReader();
            reader.Read();
            return reader.GetInt64(0);
        }

        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Preview> Previews { get; set; } = null!;
        public DbSet<Blob> Blobs { get; set; } = null!;
        public DbSet<Tag> Tags { get; set; } = null!;
        public DbSet<Collection> Collections { get; set; } = null!;
        public DbSet<StatisticSnapshot> StatisticSnapshots { get; set; } = null!;
    }
}
