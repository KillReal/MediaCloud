using MediaCloud.Data;
using MediaCloud.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;

namespace MediaCloud.WebApp.Services.Statistic
{
    public class StatisticServiceHelper
    {
        private readonly AppDbContext _context;

        public StatisticServiceHelper(AppDbContext context) 
        {
            _context = context; 
        }

        public StatisticSnapshot? Get(DateTime dateTime)
        {
            return _context.StatisticSnapshots.Where(x => x.TakenAt.Date == dateTime.Date).FirstOrDefault();
        }

        public StatisticSnapshot GetLastOrNew()
        {
            var snapshot = _context.StatisticSnapshots.OrderByDescending(x => x.TakenAt).FirstOrDefault();

            snapshot ??= new();

            if (DateTime.Now.Date != snapshot.TakenAt.Date)
            {
                snapshot = new StatisticSnapshot().Merge(snapshot);

                snapshot.TakenAt = DateTime.Now.Date;
                snapshot.UpdatedAt = DateTime.Now.Date;
                snapshot.CreatedAt = DateTime.Now.Date;

                var rootAdmin = _context.Actors.Where(x => x.IsAdmin).OrderBy(y => y.CreatedAt).First();
                snapshot.Creator = rootAdmin;
                snapshot.Updator = rootAdmin;

                _context.StatisticSnapshots.Add(snapshot);
                _context.SaveChanges();
            }

            return snapshot;
        }

        public List<StatisticSnapshot> GetList(DateTime startDate, DateTime endDate) 
        {
            return _context.StatisticSnapshots.Where(x => x.TakenAt.Date >= startDate.Date && x.TakenAt.Date <= endDate.Date)
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
                _context.StatisticSnapshots.Update(lastSnapshot);
                _context.SaveChanges();

                return;
            }

            _context.StatisticSnapshots.Add(statisticSnapshot);
            _context.SaveChanges();
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
                _context.StatisticSnapshots.Update(snapshot);
                _context.SaveChanges();

                return;
            }

            _context.StatisticSnapshots.Add(statisticSnapshot);
            _context.SaveChanges();
        }

        public DateTime GetFirstOrNowDate()
        {
            var dates = new List<DateTime?>
            {
                _context.Actors.OrderBy(x => x.CreatedAt).FirstOrDefault()?.CreatedAt,
                _context.Tags.OrderBy(x => x.CreatedAt).FirstOrDefault()?.CreatedAt,
                _context.Previews.OrderBy(x => x.CreatedAt).FirstOrDefault()?.CreatedAt
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
                ActorsCount = await _context.Actors.Where(x => x.CreatedAt.Date == dateTime.Date).CountAsync(),
                TagsCount = await _context.Tags.Where(x => x.CreatedAt.Date == dateTime.Date).CountAsync(),
                MediasCount = await _context.Previews.Where(x => x.CreatedAt.Date == dateTime.Date).CountAsync(),
                MediasSize = _context.Medias.Where(x => x.CreatedAt.Date == dateTime.Date).Select(x => x.Size).ToList().Sum()
            };
        }

        public void RemoveAllSnapshots(DateTime startDate)
        {
            _context.StatisticSnapshots.RemoveRange(_context.StatisticSnapshots.Where(x => x.TakenAt.Date >= startDate.Date));
            _context.SaveChanges();
        }
    }
}
