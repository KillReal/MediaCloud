using MediaCloud.Data;
using MediaCloud.Data.Models;
using MediaCloud.MediaUploader;
using MediaCloud.MediaUploader.Tasks;
using MediaCloud.WebApp.Services.ActorProvider;
using MediaCloud.WebApp.Services.DataService;
using Microsoft.EntityFrameworkCore;
using NLog;
using System.Net.NetworkInformation;
using ILogger = NLog.ILogger;

namespace MediaCloud.WebApp.Services.Statistic
{
    public partial class StatisticProvider
    {
        private readonly ILogger _logger;
        private readonly AppDbContext _context;
        private readonly Actor _actor;

        public StatisticProvider(AppDbContext context, Actor? actor)
        {
            _logger = LogManager.GetLogger("StatisticProvider");
            _context = context;

            if (actor == null)
            {
                throw new ArgumentException("Cannot initialize statisticProvider with unknown actor context");
            }

            _actor = actor;

            MediasCountChanged += MediasCountChangedAction;
            TagsCountChanged += TagsCountChangedAction;
            ActorsCountChanged += ActorsCountChangedAction;
            ActivityFactorRaised += ActivityFactorRaisedAction;
        }

        private StatisticSnapshot? GetSnapshotByDate(DateTime dateTime)
        {
            return _context.StatisticSnapshots.Where(x => x.TakenAt.Date == dateTime.Date
                                                        && x.CreatorId == _actor.Id)
                                                .FirstOrDefault();
        }

        private DateTime GetOldelstSnapshotDate()
        {
            var dates = new List<DateTime?>
            {
                _context.Tags.OrderBy(x => x.CreatedAt)
                                .Where(x => x.CreatorId == _actor.Id)
                                .FirstOrDefault()?.CreatedAt,
                _context.Previews.OrderBy(x => x.CreatedAt)
                                .Where(x => x.CreatorId == _actor.Id)
                                .FirstOrDefault()?.CreatedAt
            };

            if (_actor.IsAdmin)
            {
                dates.Add(_context.Actors.OrderBy(x => x.CreatedAt).FirstOrDefault()?.CreatedAt);
            }

            DateTime minDate = dates.Where(x => x != DateTime.MinValue.ToUniversalTime()).Min()
                ?? DateTime.MinValue.ToUniversalTime();

            return (minDate == DateTime.MinValue.ToUniversalTime())
                ? DateTime.Now.ToUniversalTime()
                : minDate.ToUniversalTime();
        }

        public StatisticSnapshot GetTodaySnapshot()
        {
            var snapshot = _context.StatisticSnapshots.OrderByDescending(x => x.TakenAt)
                        .Where(x => x.CreatorId == _actor.Id)
                        .FirstOrDefault();

            snapshot ??= new();

            if (DateTime.Now.Date != snapshot.TakenAt.Date)
            {
                return CreateInitialSnapshot();
            }

            return snapshot;
        }

        public List<StatisticSnapshot> GetSnapshotsByDate(DateTime start, DateTime end)
        {
            var snapshots = _context.StatisticSnapshots.Where(x => x.TakenAt.Date >= start.Date
                                        && x.TakenAt.Date <= end.Date
                                        && x.CreatorId == _actor.Id)
                                .OrderBy(x => x.TakenAt.Date)
                                .ToList();

            if (snapshots.Any() == false)
            {
                snapshots.Add(CreateInitialSnapshot());
            }

            return snapshots;
        }

        public List<StatisticSnapshot> GetAllSnapshots()
        {
            var snapshots = _context.StatisticSnapshots.Where(x => x.CreatorId == _actor.Id)
                                .OrderBy(x => x.TakenAt.Date)
                                .ToList();

            if (snapshots.Any() == false)
            {
                snapshots.Add(CreateInitialSnapshot());
            }

            return snapshots;
        }

        public StatisticSnapshot TakeSnapshot(DateTime dateTime)
        {
            return new()
            {
                Creator = _context.Actors.First(x => x.Id == _actor.Id),
                Updator = _context.Actors.First(x => x.Id == _actor.Id),
                TakenAt = dateTime,
                ActorsCount = _context.Actors.Where(x => x.CreatedAt.Date == dateTime.Date
                                                        && _actor.IsAdmin)
                                                    .Count(),
                TagsCount = _context.Tags.Where(x => x.CreatedAt.Date == dateTime.Date
                                                        && x.CreatorId == _actor.Id)
                                                    .Count(),
                MediasCount = _context.Previews.Where(x => x.CreatedAt.Date == dateTime.Date
                                                        && x.CreatorId == _actor.Id)
                                                    .Count(),
                MediasSize = _context.Medias.Where(x => x.CreatedAt.Date == dateTime.Date
                                                && x.CreatorId == _actor.Id)
                                            .Select(x => x.Size)
                                            .ToList()
                                            .Sum()
            };
        }

        private StatisticSnapshot CreateInitialSnapshot()
        {
            var snapshot = new StatisticSnapshot();

            snapshot.TakenAt = DateTime.Now.Date;
            snapshot.UpdatedAt = DateTime.Now.Date;
            snapshot.CreatedAt = DateTime.Now.Date;

            snapshot.Creator = _context.Actors.First(x => x.Id == _actor.Id);
            snapshot.Updator = snapshot.Creator;

            _context.StatisticSnapshots.Add(snapshot);
            _context.SaveChanges();

            return snapshot;
        }

        public void CreateOrUpdateSnapshotByDate(StatisticSnapshot statisticSnapshot, DateTime takenAt)
        {
            var snapshot = GetSnapshotByDate(takenAt);

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

        public void RemoveAllSnapshots()
        {
            var snapshots = _context.StatisticSnapshots.Where(x => x.CreatorId == _actor.Id);

            _context.StatisticSnapshots.RemoveRange(snapshots);
            _context.SaveChanges();
        }

        public RecalculateTask GetRecalculationTask()
        {
            return GetRecalculationTask(DateTime.MinValue);
        }

        public RecalculateTask GetRecalculationTask(int lastDaysCount = 0)
        {
            if (lastDaysCount <= 0)
            {
                return GetRecalculationTask();
            }

            return GetRecalculationTask(DateTime.Now.AddDays(-lastDaysCount));
        }

        public RecalculateTask GetRecalculationTask(DateTime startDate)
        {
            if (startDate == DateTime.MinValue)
            {
                startDate = GetOldelstSnapshotDate();
            }

            return new RecalculateTask(_actor, startDate);
        }

        public void Recalculate(DateTime startTime, ref int progressCount)
        {
            _logger.Info("Statistic recalculation started");

            RemoveAllSnapshots();

            var totalDaysCalculated = 0;
            var totalDaysInserted = 0;
            var prevSnapshot = TakeSnapshot(startTime);
            var date = startTime.AddDays(1);
            progressCount = DateTime.Now.Subtract(date).Days;

            var stopwatchTotal = DateTime.Now;
            do
            {
                var stopwatch = DateTime.Now;
                _logger.Debug("Calculating statistic for {date.Date}", date.Date);
                var snapshot = TakeSnapshot(date);

                if (snapshot.IsEmpty() == false)
                {
                    CreateOrUpdateSnapshotByDate(snapshot.Merge(prevSnapshot), date);
                    totalDaysInserted++;
                    prevSnapshot = snapshot;
                }

                date = date.AddDays(1);
                totalDaysCalculated++;
                progressCount--;

                var elapsedTime = (DateTime.Now - stopwatch).TotalSeconds;
                _logger.Debug("Calculating done by {elapsedTime}", elapsedTime);
            } while (date.Date <= DateTime.Now.Date);

            _logger.Info("Statistic recalculation completed for {totalDaysCalculated} days " +
              "({totalDaysInserted} saved) by {(int)(DateTime.Now - stopwatchTotal).TotalSeconds} sec",
              totalDaysCalculated, totalDaysInserted, (int)(DateTime.Now - stopwatchTotal).TotalSeconds);
        }
    }
}
