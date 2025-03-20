using System.Runtime.InteropServices.JavaScript;
using MediaCloud.Data;
using MediaCloud.Data.Models;
using MediaCloud.TaskScheduler.Tasks;
using MediaCloud.WebApp.Services.UserProvider;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.Extensions.Caching.Memory;
using NLog;

namespace MediaCloud.WebApp.Services.Statistic
{
    public partial class StatisticProvider
    {
        private readonly Logger _logger;
        private readonly AppDbContext _context;
        private readonly IMemoryCache _cache;
        private readonly IUserProvider _userProvider;
        private readonly MemoryCacheEntryOptions _memoryCacheOptions;
        
        private const int CacheDurationInMinutes = 120;

        public StatisticProvider(IServiceScopeFactory scopeFactory, IUserProvider userProvider, IMemoryCache cache)
        {
            _logger = LogManager.GetLogger("StatisticProvider");
            _context = scopeFactory.CreateScope().ServiceProvider.GetRequiredService<AppDbContext>();
            _userProvider = userProvider;
            _cache = cache;
            _memoryCacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(CacheDurationInMinutes));

            MediasCountChanged += MediasCountChangedAction;
            TagsCountChanged += TagsCountChangedAction;
            ActorsCountChanged += ActorsCountChangedAction;
            ActivityFactorRaised += ActivityFactorRaisedAction;

            _logger.Debug("Initialized StatisticProvider by actor: {0}", _userProvider.GetCurrent().Name);
        }

        // Select existing snapshot by date.
        private StatisticSnapshot GetSnapshotByDate(DateTime dateTime)
        {
            if (_cache.TryGetValue($"{_userProvider.GetCurrent().Name}_{DateTime.Now.Date}", out StatisticSnapshot? snapshot))
            {
                if (snapshot != null)
                {
                    return snapshot;
                }
            }

            snapshot = _context.StatisticSnapshots.Include(x => x.Creator)
                .Include(x => x.Updator)
                .FirstOrDefault(x => x.TakenAt.Date == dateTime.Date && x.Creator == _userProvider.GetCurrent());
            if (snapshot == null)
            {
                var lastSnapshot = _context.StatisticSnapshots.Where(x => x.Creator == _userProvider.GetCurrent())
                    .Include(x => x.Creator)
                    .Include(x => x.Updator)
                    .OrderByDescending(x => x.TakenAt)
                    .FirstOrDefault();

                if (lastSnapshot == null)
                {
                    return CreateInitialSnapshot();
                }

                snapshot = new StatisticSnapshot().Merge(lastSnapshot);
                snapshot.TakenAt = dateTime.Date;
                
                _context.StatisticSnapshots.Add(snapshot);
                _context.SaveChanges();
            }

            if (DateTime.Now.Date != snapshot.TakenAt.Date)
            {
                return snapshot;
            }
            
            _cache.Set($"{_userProvider.GetCurrent().Name}_{snapshot.TakenAt.Date}", snapshot, _memoryCacheOptions);

            return snapshot;

        }

        // Select date of oldest snapshot of current user.
        private DateTime GetOldelstSnapshotDate()
        {
            var dates = new List<DateTime?>
            {
                _context.Tags.OrderBy(x => x.CreatedAt)
                    .FirstOrDefault(x => x.Creator == _userProvider.GetCurrent())?.CreatedAt,
                _context.Previews.OrderBy(x => x.CreatedAt)
                    .FirstOrDefault(x => x.Creator == _userProvider.GetCurrent())?.CreatedAt
            };

            if (_userProvider.GetCurrent().IsAdmin)
            {
                dates.Add(_context.Users.OrderBy(x => x.CreatedAt).FirstOrDefault()?.CreatedAt);
            }

            var minDate = dates.Where(x => x != DateTime.MinValue.ToUniversalTime()).Min()
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
            return GetSnapshotByDate(DateTime.Now);
        }

        /// <summary>
        /// Select all snapshots for current user in certain data range. Return's at least one.
        /// If there is no snapshots for user, a new one will be created <see cref="GetTodaySnapshot()"/>
        /// </summary>
        /// <param name="start"> Start date (include). </param>
        /// <param name="end"> End date (include). </param>
        /// <returns> List of existing snapshots. </returns>
        public List<StatisticSnapshot> GetSnapshotsByDateRange(DateTime start, DateTime end)
        {
            var snapshots = _context.StatisticSnapshots.Where(x => x.TakenAt.Date >= start.Date
                                        && x.TakenAt.Date <= end.Date
                                        && x.Creator == _userProvider.GetCurrent())
                                .OrderBy(x => x.TakenAt.Date)
                                .ToList();

            if (snapshots.Count == 0)
            {
                snapshots.Add(GetTodaySnapshot());
            }

            return snapshots;
        }

        /// <summary>
        /// Select all snapshots for current user. Return's at least one. 
        /// If there is no snapshots for user, a new one will be created <see cref="GetTodaySnapshot()"/>
        /// </summary>
        /// <returns> List of all existing snapshots. </returns>
        public List<StatisticSnapshot> GetAllSnapshots()
        {
            var snapshots = _context.StatisticSnapshots.Where(x => x.Creator == _userProvider.GetCurrent())
                                .OrderBy(x => x.TakenAt.Date)
                                .ToList();

            if (snapshots.Count == 0 || snapshots.Last().TakenAt.Date != DateTime.Now.Date)
            {
                snapshots.Add(GetTodaySnapshot());
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
                Creator = _context.Users.First(x => x.Id == _userProvider.GetCurrent().Id),
                Updator = _context.Users.First(x => x.Id == _userProvider.GetCurrent().Id),
                TakenAt = date,
                ActorsCount = _context.Users.Count(x => x.CreatedAt.Date == date.Date 
                                                        && _userProvider.GetCurrent().IsAdmin),
                TagsCount = _context.Tags.Count(x => x.CreatedAt.Date == date.Date 
                                                     && x.Creator == _userProvider.GetCurrent()),
                MediasCount = _context.Previews.Count(x => x.CreatedAt.Date == date.Date
                                                           && x.Creator == _userProvider.GetCurrent()),
                MediasSize = _context.Blobs.Where(x => x.CreatedAt.Date == date.Date
                                                && x.Creator == _userProvider.GetCurrent())
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

                Creator = _context.Users.First(x => x == _userProvider.GetCurrent())
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
        private void UpdateSnapshot(StatisticSnapshot snapshot)
        {
            _context.StatisticSnapshots.Update(snapshot);
            _context.SaveChanges();
        }

        /// <summary>
        /// Remove all snapshots for current user.
        /// </summary>
        public void RemoveAllSnapshots()
        {
            if (_cache.TryGetValue($"{_userProvider.GetCurrent().Name}_{DateTime.Now.Date}", out StatisticSnapshot? _))
            {
                _cache.Remove($"{_userProvider.GetCurrent().Name}_{DateTime.Now.Date}");
            }
            
            var snapshots = _context.StatisticSnapshots.Where(x => x.Creator == _userProvider.GetCurrent());

            _context.StatisticSnapshots.RemoveRange(snapshots);
            _context.SaveChanges();
        }

        /// <summary>
        /// Get recalculation task for current user for last amount of days. <see cref="GetRecalculationTask()"/>
        /// </summary>
        /// <param name="lastDaysCount"> Amount of days to recalculate. </param>
        /// <returns> Task for user statistic recalculation. </returns>
        public RecalculateTask GetRecalculationTask(int lastDaysCount = 0)
        {
            return GetRecalculationTask(lastDaysCount <= 0 
                ? DateTime.MinValue 
                : DateTime.Now.AddDays(-lastDaysCount));
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

            return new RecalculateTask(_userProvider.GetCurrent(), startDate);
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
                    UpdateSnapshot(snapshot.Merge(prevSnapshot));
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
