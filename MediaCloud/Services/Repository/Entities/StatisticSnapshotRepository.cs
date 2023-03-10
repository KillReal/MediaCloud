using MediaCloud.Data.Models;
using MediaCloud.WebApp.Services.Repository.Entities.Base;

namespace MediaCloud.Repositories
{
    public class StatisticSnapshotRepository : Repository<StatisticSnapshot>
    {
        public StatisticSnapshotRepository(RepositoryContext repositoryContext) : base(repositoryContext)
        {
        }

        public StatisticSnapshot? GetLast()
        {
            var lastSnapshot = _context.StatisticSnapshots.OrderBy(x => x.CreatedAt).FirstOrDefault();

            if (lastSnapshot == null) 
            { 
                return null;
            }

            return (DateTime.Now - lastSnapshot.CreatedAt).TotalHours < 24
                ? lastSnapshot
                : null;
        }

        public void Append(StatisticSnapshot statisticSnapshot)
        {
            _context.StatisticSnapshots.Append(statisticSnapshot);
            SaveChanges();
        }
    }
}
