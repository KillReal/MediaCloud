using MediaCloud.Data;
using MediaCloud.Data.Models;
using MediaCloud.WebApp.Services.DataService;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace MediaCloud.WebApp.Services.Statistic
{
    public partial class StatisticService : IStatisticService
    {
        private readonly StatisticServiceHelper _serviceHelper;
        private readonly ILogger _logger;
        private readonly StatisticSnapshot _currentSnapshot;
        private StatisticServiceStatusType _status = StatisticServiceStatusType.ListenForEvents;

        public StatisticService(IServiceProvider serviceProvider, ILogger<StatisticService> logger) 
        {
            _serviceHelper = new(serviceProvider.CreateScope().ServiceProvider.GetRequiredService<AppDbContext>());
            _logger = logger;
            _currentSnapshot = _serviceHelper.GetLastOrNew();

            if (_currentSnapshot == null)    
            {
                _currentSnapshot = new StatisticSnapshot();
                _serviceHelper.SaveOrUpdate(_currentSnapshot);
            }

            MediasCountChanged += MediasCountChangedAction;
            TagsCountChanged += TagsCountChangedAction;
            ActorsCountChanged += ActorsCountChangedAction;
            ActivityFactorRaised += ActivityFactorRaisedAction;
        }

        public StatisticSnapshot GetTodayStatistic()
        {
            return _currentSnapshot;
        }
        public List<StatisticSnapshot> GetStatistic()
        {
            var startDate = _serviceHelper.GetFirstOrNowDate();
            var endDate = DateTime.Now.ToUniversalTime();

            return _serviceHelper.GetList(startDate, endDate);
        }

        public List<StatisticSnapshot> GetStatistic(DateTime startDate, DateTime endDate)
        {
            return _serviceHelper.GetList(startDate, endDate);
        }

        public void ProceedRecalculaton()
        {
            ProceedRecalculaton(DateTime.MinValue);
        }

        public void ProceedRecalculaton(int lastDaysCount)
        {
            ProceedRecalculaton(DateTime.Now.AddDays(-lastDaysCount));
        }

        public void ProceedRecalculaton(DateTime startDate)
        {
            if (_status == StatisticServiceStatusType.Recalculating)
            {
                return;
            }

            _status = StatisticServiceStatusType.Recalculating;

            if (startDate == DateTime.MinValue)
            {
                startDate = _serviceHelper.GetFirstOrNowDate();
            }
            _logger.LogInformation($"Statistic recalculation started");

            _serviceHelper.RemoveAllSnapshots(startDate); 

            var totalDaysCalculated = 0;
            var totalDaysInserted = 0;
            var prevSnapshot = _serviceHelper.GetLastOrNew();
            prevSnapshot.TakenAt = DateTime.MinValue;
            var date = startDate;

            var stopwatchTotal = DateTime.Now;
            Task.Run(() =>
            {
                do
                {
                    var stopwatch = DateTime.Now;
                    _logger.LogDebug($"Calculating statistic for {date.Date}");
                    var snapshot = _serviceHelper.TakeSnapshotAsync(date).Result;
                    
                    if (snapshot.IsEmpty() == false)
                    {
                        _serviceHelper.SaveOrUpdate(snapshot.Merge(prevSnapshot), date);
                        totalDaysInserted++;
                        prevSnapshot = snapshot;
                    }

                    date = date.AddDays(1);
                    totalDaysCalculated++;
                    _logger.LogDebug("Calculating done by {(DateTime.Now - stopwatch).TotalSeconds}", (DateTime.Now - stopwatch).TotalSeconds);
                } while (date.Date < DateTime.Now.Date);
            }).Wait();

            _status = StatisticServiceStatusType.ListenForEvents;
            _logger.LogInformation("Statistic recalculation completed for {totalDaysCalculated} days ({totalDaysInserted} saved) by {(int)(DateTime.Now - stopwatchTotal).TotalSeconds} sec",
                totalDaysCalculated, totalDaysInserted, (int)(DateTime.Now - stopwatchTotal).TotalSeconds);
        }

        public StatisticServiceStatusType GetStatus() => _status;
    }
}
