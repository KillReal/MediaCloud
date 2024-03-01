using MediaCloud.Data;
using MediaCloud.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;

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
            var snapshot = Context.StatisticSnapshots.OrderByDescending(x => x.TakenAt).FirstOrDefault();

            snapshot ??= new();

            if (DateTime.Now.Date != snapshot.TakenAt.Date)
            {
                snapshot = new StatisticSnapshot().Merge(snapshot);

                snapshot.TakenAt = DateTime.Now.Date;
                snapshot.UpdatedAt = DateTime.Now.Date;
                snapshot.CreatedAt = DateTime.Now.Date;

                var rootAdmin = Context.Actors.Where(x => x.IsAdmin).OrderBy(y => y.CreatedAt).First();
                snapshot.Creator = rootAdmin;
                snapshot.Updator = rootAdmin;

                Context.StatisticSnapshots.Add(snapshot);
                Context.SaveChanges();
            }

            return snapshot;
        }

        public List<StatisticSnapshot> GetList(DateTime startDate, DateTime endDate) 
        {
            return Context.StatisticSnapshots.Where(x => x.TakenAt.Date >= startDate.Date && x.TakenAt.Date <= endDate.Date)
                                             .OrderBy(x => x.TakenAt.Date)
                                             .ToList();
        }

        public void SaveOrUpdate(StatisticSnapshot statisticSnapshot)
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

        public void SaveOrUpdate(StatisticSnapshot statisticSnapshot, DateTime takenAt)
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

            DateTime minDate = dates.Where(x => x != DateTime.MinValue.ToUniversalTime()).Min() ?? DateTime.MinValue.ToUniversalTime();

            return (minDate == DateTime.MinValue.ToUniversalTime()) 
                ? DateTime.Now.ToUniversalTime()
                : minDate.ToUniversalTime();
        }

        public async Task<StatisticSnapshot> TakeSnapshotAsync(DateTime dateTime)
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

        public void RemoveAllSnapshots(DateTime startDate)
        {
            Context.StatisticSnapshots.RemoveRange(Context.StatisticSnapshots.Where(x => x.TakenAt.Date >= startDate.Date));
            Context.SaveChanges();
        }
    }
}
