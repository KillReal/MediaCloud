using MediaCloud.Data.Models;
using MediaCloud.WebApp.Services.DataService.Entities.Base;

namespace MediaCloud.Repositories
{
    public class StatisticSnapshotDataService : DataService<StatisticSnapshot>
    {
        public StatisticSnapshotDataService(DataServiceContext DataServiceContext) : base(DataServiceContext)
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
