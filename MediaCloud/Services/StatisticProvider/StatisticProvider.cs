﻿using System.Runtime.InteropServices.JavaScript;
using MediaCloud.Data;
using MediaCloud.Data.Models;
using MediaCloud.TaskScheduler.Tasks;
using MediaCloud.WebApp.Services.UserProvider;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.Extensions.Caching.Memory;
using NLog;
using ILogger = NLog.ILogger;

namespace MediaCloud.WebApp.Services.Statistic
{
    public partial class StatisticProvider
    {
        private const int CacheExpirationInMinutes = 120;
        
        private IMemoryCache _memoryCache;
        private MemoryCacheEntryOptions _memoryCacheOptions;
        private readonly ILogger _logger;
        private readonly AppDbContext _context;
        private readonly User _currentUser;

        public StatisticProvider(AppDbContext context, IUserProvider userProvider, IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
            _memoryCacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(CacheExpirationInMinutes));
            _logger = LogManager.GetLogger("StatisticProvider");
            _context = context;
            _currentUser = userProvider.GetCurrent();

            if (_currentUser == null)
            {
                throw new ArgumentException("Cannot initialize statisticProvider with unknown actor context");
            }

            MediasCountChanged += MediasCountChangedAction;
            TagsCountChanged += TagsCountChangedAction;
            ActorsCountChanged += ActorsCountChangedAction;
            ActivityFactorRaised += ActivityFactorRaisedAction;

            _logger.Debug("Initialized StatisticProvider by actor: {0}", _currentUser.Name);
        }

        // Select existing snapshot by date.
        private StatisticSnapshot? GetSnapshotByDate(DateTime dateTime)
        {
            if (_memoryCache.TryGetValue(_currentUser.Name + dateTime.Date, out StatisticSnapshot? snapshot))
            {
                if (snapshot != null)
                {
                    return snapshot;
                }
            }
            
            return _context.StatisticSnapshots
                .FirstOrDefault(x => x.TakenAt.Date == dateTime.Date
                    && x.CreatorId == _currentUser.Id);
        }

        // Select date of oldest snapshot of current user.
        private DateTime GetOldestSnapshotDate()
        {
            var dates = new List<DateTime?>
            {
                _context.Tags.OrderBy(x => x.CreatedAt)
                    .FirstOrDefault(x => x.CreatorId == _currentUser.Id)?.CreatedAt,
                _context.Previews.OrderBy(x => x.CreatedAt)
                    .FirstOrDefault(x => x.CreatorId == _currentUser.Id)?.CreatedAt
            };

            if (_currentUser.IsAdmin)
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
            if (_memoryCache.TryGetValue(_currentUser.Name + DateTime.Today, out StatisticSnapshot? snapshot))
            {
                if (snapshot != null)
                {
                    return snapshot;
                }
            }
            
            snapshot = _context.StatisticSnapshots.OrderByDescending(x => x.TakenAt)
                .FirstOrDefault(x => x.CreatorId == _currentUser.Id);

            if (snapshot == null) 
            {
                return CreateInitialSnapshot();
            }

            if (DateTime.Now.Date == snapshot.TakenAt.Date)
            {
                return snapshot;
            }
            
            snapshot = new StatisticSnapshot().Merge(snapshot);
            snapshot.TakenAt = DateTime.Now.Date;

            CreateOrUpdateSnapshot(snapshot);

            return snapshot;
        }

        /// <summary>
        /// Select all snapshots for current user in certain data range. Return's at least one.
        /// If there is no snapshots for user, a new one will be created <see cref="GetTodaySnapshot()"/>
        /// </summary>
        /// <param name="start"> Start date (include). </param>
        /// <param name="end"> End date (include). </param>
        /// <returns> List of existing snapshots. </returns>
        public List<StatisticSnapshot> GetSnapshotsByDate(DateTime start, DateTime end)
        {
            var snapshots = _context.StatisticSnapshots.Where(x => x.TakenAt.Date >= start.Date
                                        && x.TakenAt.Date <= end.Date
                                        && x.CreatorId == _currentUser.Id)
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
            var snapshots = _context.StatisticSnapshots.Where(x => x.CreatorId == _currentUser.Id)
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
            return new StatisticSnapshot
            {
                Creator = _context.Users.First(x => x.Id == _currentUser.Id),
                Updator = _context.Users.First(x => x.Id == _currentUser.Id),
                TakenAt = date,
                ActorsCount = _context.Users.Count(x => x.CreatedAt.Date == date.Date && _currentUser.IsAdmin),
                TagsCount = _context.Tags.Count(x => x.CreatedAt.Date == date.Date && x.CreatorId == _currentUser.Id),
                MediasCount = _context.Previews.Count(x => x.CreatedAt.Date == date.Date && x.CreatorId == _currentUser.Id),
                MediasSize = _context.Blobs.Where(x => x.CreatedAt.Date == date.Date && x.CreatorId == _currentUser.Id)
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

                Creator = _context.Users.First(x => x.Id == _currentUser.Id)
            };
            snapshot.Updator = snapshot.Creator;

            _context.StatisticSnapshots.Add(snapshot);
            _context.SaveChanges();
            
            _memoryCache.Set(_currentUser.Name + DateTime.Today, snapshot, _memoryCacheOptions);

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
                
                _memoryCache.Set(_currentUser.Name + DateTime.Today, snapshot, _memoryCacheOptions);
                
                return;
            }

            _context.StatisticSnapshots.Add(snapshot);
            _context.SaveChanges();
            
            _memoryCache.Set(_currentUser.Name + DateTime.Today, snapshot, _memoryCacheOptions);
        }

        /// <summary>
        /// Remove all snapshots for current user.
        /// </summary>
        public void RemoveAllSnapshots()
        {
            var snapshots = _context.StatisticSnapshots.Where(x => x.CreatorId == _currentUser.Id);

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
            return lastDaysCount <= 0 
                ? GetRecalculationTask() 
                : GetRecalculationTask(DateTime.Now.AddDays(-lastDaysCount));
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
                startDate = GetOldestSnapshotDate();
            }

            return new RecalculateTask(_currentUser, startDate);
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
