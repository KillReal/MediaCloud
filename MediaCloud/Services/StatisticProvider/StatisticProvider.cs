using MediaCloud.Data;
using MediaCloud.Data.Models;
using MediaCloud.MediaUploader;
using MediaCloud.MediaUploader.Tasks;
using MediaCloud.WebApp.Services.ActorProvider;
using MediaCloud.WebApp.Services.DataService;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
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

        // Select existing snapshot by date.
        private StatisticSnapshot? GetSnapshotByDate(DateTime dateTime)
        {
            return _context.StatisticSnapshots.Where(x => x.TakenAt.Date == dateTime.Date
                                                        && x.CreatorId == _actor.Id)
                                                .FirstOrDefault();
        }

        // Select date of oldest snapshot of current user.
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

        /// <summary>
        /// Select today snapshot for current user. 
        /// If no snapshots exist, creates one, <see cref="CreateInitialSnapshot()"/>
        /// If no snapshot for today, create copy of latest.
        /// </summary>
        /// <returns> Today existing snapshot. </returns>
        public StatisticSnapshot GetTodaySnapshot()
        {
            var snapshot = _context.StatisticSnapshots.OrderByDescending(x => x.TakenAt)
                        .Where(x => x.CreatorId == _actor.Id)
                        .FirstOrDefault();

            if (snapshot == null) 
            {
                return CreateInitialSnapshot();
            }

            if (DateTime.Now.Date != snapshot.TakenAt.Date)
            {
                snapshot = new StatisticSnapshot().Merge(snapshot);
                snapshot.TakenAt = DateTime.Now.Date;
                
                CreateOrUpdateSnapshot(snapshot);
            }

            return snapshot;
        }

        /// <summary>
        /// Select all snapshots for current user in certain data range. Return's at least one.
        /// If there is no snapshots for user, a new one will be created <see cref="CreateInitialSnapshot()"/>
        /// </summary>
        /// <param name="start"> Start date (include). </param>
        /// <param name="end"> End date (include). </param>
        /// <returns> List of existing snapshots. </returns>
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

        /// <summary>
        /// Select all snapshots for current user. Return's at least one. 
        /// If there is no snapshots for user, a new one will be created <see cref="CreateInitialSnapshot()"/>
        /// </summary>
        /// <returns> List of all existing snapshots. </returns>
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

        /// <summary>
        /// Capture snapshot by date for current user.
        /// </summary>
        /// <param name="date"> Date to capture. </param>
        /// <returns> New snapshot with changes for certain day. This snapshot need to be inserted. </returns>
        public StatisticSnapshot CaptureSnapshot(DateTime date)
        {
            return new()
            {
                Creator = _context.Actors.First(x => x.Id == _actor.Id),
                Updator = _context.Actors.First(x => x.Id == _actor.Id),
                TakenAt = date,
                ActorsCount = _context.Actors.Where(x => x.CreatedAt.Date == date.Date
                                                        && _actor.IsAdmin)
                                                    .Count(),
                TagsCount = _context.Tags.Where(x => x.CreatedAt.Date == date.Date
                                                        && x.CreatorId == _actor.Id)
                                                    .Count(),
                MediasCount = _context.Previews.Where(x => x.CreatedAt.Date == date.Date
                                                        && x.CreatorId == _actor.Id)
                                                    .Count(),
                MediasSize = _context.Medias.Where(x => x.CreatedAt.Date == date.Date
                                                && x.CreatorId == _actor.Id)
                                            .Select(x => x.Size)
                                            .ToList()
                                            .Sum()
            };
        }

        // Creates new snapshot for today for current user.
        private StatisticSnapshot CreateInitialSnapshot()
        {
            var snapshot = new StatisticSnapshot
            {
                TakenAt = DateTime.Now.Date,
                UpdatedAt = DateTime.Now.Date,
                CreatedAt = DateTime.Now.Date,

                Creator = _context.Actors.First(x => x.Id == _actor.Id)
            };
            snapshot.Updator = snapshot.Creator;

            _context.StatisticSnapshots.Add(snapshot);
            _context.SaveChanges();

            return snapshot;
        }

        /// <summary>
        /// Update or insert snapshot.
        /// </summary>
        /// <param name="snapshot"> Snapshot for update or create. </param>
        public void CreateOrUpdateSnapshot(StatisticSnapshot snapshot)
        {
            var existingSnapshot = GetSnapshotByDate(snapshot.TakenAt);

            if (existingSnapshot != null)
            {
                existingSnapshot.ActorsCount = snapshot.ActorsCount;
                existingSnapshot.TagsCount = snapshot.TagsCount;
                existingSnapshot.MediasCount = snapshot.MediasCount;
                existingSnapshot.MediasSize = snapshot.MediasSize;
                _context.StatisticSnapshots.Update(snapshot);
                _context.SaveChanges();

                return;
            }

            _context.StatisticSnapshots.Add(snapshot);
            _context.SaveChanges();
        }

        /// <summary>
        /// Remove all snapshots for current user.
        /// </summary>
        public void RemoveAllSnapshots()
        {
            var snapshots = _context.StatisticSnapshots.Where(x => x.CreatorId == _actor.Id);

            _context.StatisticSnapshots.RemoveRange(snapshots);
            _context.SaveChanges();
        }

        /// <summary>
        /// Get recalculation task for current user. <see cref="Task"/>
        /// </summary>
        /// <returns> Task for user statistic recalculation. </returns>
        public RecalculateTask GetRecalculationTask()
        {
            return GetRecalculationTask(DateTime.MinValue);
        }

        /// <summary>
        /// Get recalculation task for current user for last amount of days. <see cref="GetRecalculationTask()"/>
        /// </summary>
        /// <param name="lastDaysCount"> Amount of days to recalculate. </param>
        /// <returns> Task for user statistic recalculation. </returns>
        public RecalculateTask GetRecalculationTask(int lastDaysCount = 0)
        {
            if (lastDaysCount <= 0)
            {
                return GetRecalculationTask();
            }

            return GetRecalculationTask(DateTime.Now.AddDays(-lastDaysCount));
        }

        /// <summary>
        /// Get recalculation task for current user for date range after certain date. <see cref="GetRecalculationTask()"/>
        /// </summary>
        /// <param name="startDate"> Start date to recalculate. </param>
        /// <returns> Task for user statistic recalculation. </returns>
        public RecalculateTask GetRecalculationTask(DateTime startDate)
        {
            if (startDate == DateTime.MinValue)
            {
                startDate = GetOldelstSnapshotDate();
            }

            return new RecalculateTask(_actor, startDate);
        }

        /// <summary>
        /// Start recalculation for current user from certain date with progress watching.
        /// </summary>
        /// <param name="startTime"> Date to start recalculate from. </param>
        /// <param name="progressCount"> Progress count. Can be used for external monitoring. </param>
        public void Recalculate(DateTime startTime, ref int progressCount)
        {
            _logger.Info("Statistic recalculation started");

            RemoveAllSnapshots();

            var totalDaysCalculated = 0;
            var totalDaysInserted = 0;
            var prevSnapshot = CaptureSnapshot(startTime);
            var date = startTime.AddDays(1);
            progressCount = DateTime.Now.Subtract(date).Days;

            var stopwatchTotal = DateTime.Now;
            do
            {
                var stopwatch = DateTime.Now;
                _logger.Debug("Calculating statistic for {date.Date}", date.Date);
                var snapshot = CaptureSnapshot(date);

                if (snapshot.IsEmpty() == false)
                {
                    CreateOrUpdateSnapshot(snapshot.Merge(prevSnapshot));
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
