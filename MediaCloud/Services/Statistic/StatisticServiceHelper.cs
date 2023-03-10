using MediaCloud.Data;
using MediaCloud.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace MediaCloud.WebApp.Services.Statistic
{
    public class StatisticServiceHelper
    {
        private AppDbContext Context { get; set; }

        public StatisticServiceHelper(AppDbContext context) 
        {
            Context = context; 
        }

        public StatisticSnapshot? Get(DateTime dateTime)
        {
            return Context.StatisticSnapshots.Where(x => x.TakenAt.Date == dateTime.Date).FirstOrDefault();
        }

        public StatisticSnapshot GetLastOrNew()
        {
            var lastSnapshot = Context.StatisticSnapshots.OrderByDescending(x => x.TakenAt).FirstOrDefault();

            return DateTime.Now.Date == lastSnapshot?.TakenAt.Date
                ? lastSnapshot
                : new();
        }

        public List<StatisticSnapshot> GetList(DateTime startDate, DateTime endDate) 
        {
            return Context.StatisticSnapshots.Where(x => x.TakenAt >= startDate && x.TakenAt.Date <= endDate).ToList();
        }

        public void AppendOrUpdate(StatisticSnapshot statisticSnapshot)
        {
            var lastSnapshot = GetLastOrNew();

            if (lastSnapshot != null)
            {
                lastSnapshot.ActorsCount = statisticSnapshot.ActorsCount;
                lastSnapshot.TagsCount = statisticSnapshot.TagsCount;
                lastSnapshot.MediasCount = statisticSnapshot.MediasCount;
                lastSnapshot.MediasSize = statisticSnapshot.MediasSize;
                lastSnapshot.ActivityFactor = statisticSnapshot.ActivityFactor;
                Context.StatisticSnapshots.Update(lastSnapshot);
                Context.SaveChanges();

                return;
            }

            Context.StatisticSnapshots.Add(statisticSnapshot);
            Context.SaveChanges();
        }

        public void AppendOrUpdate(StatisticSnapshot statisticSnapshot, DateTime takenAt)
        {
            var snapshot = Get(takenAt);

            if (snapshot != null)
            {
                snapshot.ActorsCount = statisticSnapshot.ActorsCount;
                snapshot.TagsCount = statisticSnapshot.TagsCount;
                snapshot.MediasCount = statisticSnapshot.MediasCount;
                snapshot.MediasSize = statisticSnapshot.MediasSize;
                Context.StatisticSnapshots.Update(snapshot);
                Context.SaveChanges();

                return;
            }

            Context.StatisticSnapshots.Add(statisticSnapshot);
            Context.SaveChanges();
        }

        public DateTime GetFirstOrNowDate()
        {
            var dates = new List<DateTime?>
            {
                Context.Actors.OrderBy(x => x.CreatedAt).FirstOrDefault()?.CreatedAt,
                Context.Tags.OrderBy(x => x.CreatedAt).FirstOrDefault()?.CreatedAt,
                Context.Previews.OrderBy(x => x.CreatedAt).FirstOrDefault()?.CreatedAt
            };

            DateTime minDate = dates.Where(x => x != DateTime.MinValue).Min() ?? DateTime.MinValue;

            return (minDate == DateTime.MinValue) 
                ? DateTime.Now
                : minDate;
        }

        public async Task<StatisticSnapshot> CalculateAsync(DateTime dateTime)
        {
            return new()
            {
                TakenAt = dateTime,
                ActorsCount = await Context.Actors.Where(x => x.CreatedAt.Date == dateTime.Date).CountAsync(),
                TagsCount = await Context.Tags.Where(x => x.CreatedAt.Date == dateTime.Date).CountAsync(),
                MediasCount = await Context.Previews.Where(x => x.CreatedAt.Date == dateTime.Date).CountAsync(),
                MediasSize = Context.Medias.Where(x => x.CreatedAt.Date == dateTime.Date).Select(x => x.Size).ToList().Sum()
            };
        }
    }
}
