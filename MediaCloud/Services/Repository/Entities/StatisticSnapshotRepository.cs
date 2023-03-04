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
            return _context.StatisticSnapshots.OrderBy(x => x.CreatedAt).FirstOrDefault();
        }

        public void Append(StatisticSnapshot statisticSnapshot)
        {
            _context.StatisticSnapshots.Append(statisticSnapshot);
            SaveChanges();
        }
    }
}
