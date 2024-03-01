using MediaCloud.Data;
using MediaCloud.Data.Models;
using MediaCloud.WebApp.Services.Repository;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace MediaCloud.WebApp.Services.Statistic
{
    public partial class StatisticService : IStatisticService
    {
        private StatisticServiceHelper ServiceHelper { get; set; }
        private ILogger Logger { get; set; }
        private StatisticSnapshot CurrentSnapshot { get; set; }
        private StatisticServiceStatusType Status { get; set; } = StatisticServiceStatusType.ListenForEvents;

        public StatisticService(IServiceProvider serviceProvider, ILogger<StatisticService> logger) 
        {
            ServiceHelper = new(serviceProvider.CreateScope().ServiceProvider.GetRequiredService<AppDbContext>());
            Logger = logger;
            CurrentSnapshot = ServiceHelper.GetLastOrNew();

            if (CurrentSnapshot == null)    
            {
                CurrentSnapshot = new StatisticSnapshot();
                ServiceHelper.SaveOrUpdate(CurrentSnapshot);
            }

            InitListeners();
        }

        public StatisticSnapshot GetTodayStatistic()
        {
            return CurrentSnapshot;
        }
        public List<StatisticSnapshot> GetStatistic()
        {
            var startDate = ServiceHelper.GetFirstOrNowDate();
            var endDate = DateTime.Now.ToUniversalTime();

            return ServiceHelper.GetList(startDate, endDate);
        }

        public List<StatisticSnapshot> GetStatistic(DateTime startDate, DateTime endDate)
        {
            return ServiceHelper.GetList(startDate, endDate);
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
            if (Status == StatisticServiceStatusType.Recalculating)
            {
                return;
            }

            Status = StatisticServiceStatusType.Recalculating;

            if (startDate == DateTime.MinValue)
            {
                startDate = ServiceHelper.GetFirstOrNowDate();
            }
            Logger.LogInformation($"Statistic recalculation started");

            ServiceHelper.RemoveAllSnapshots(startDate); 

            var totalDaysCalculated = 0;
            var totalDaysInserted = 0;
            var prevSnapshot = ServiceHelper.GetLastOrNew();
            prevSnapshot.TakenAt = DateTime.MinValue;
            var date = startDate;

            var stopwatchTotal = DateTime.Now;
            Task.Run(() =>
            {
                do
                {
                    var stopwatch = DateTime.Now;
                    Logger.LogDebug($"Calculating statistic for {date.Date}");
                    var snapshot = ServiceHelper.TakeSnapshotAsync(date).Result;
                    
                    if (snapshot.IsEmpty() == false)
                    {
                        ServiceHelper.SaveOrUpdate(snapshot.Merge(prevSnapshot), date);
                        totalDaysInserted++;
                        prevSnapshot = snapshot;
                    }

                    date = date.AddDays(1);
                    totalDaysCalculated++;
                    Logger.LogDebug($"Calculating done by {(DateTime.Now - stopwatch).TotalSeconds}");
                } while (date.Date < DateTime.Now.Date);
            }).Wait();

            Status = StatisticServiceStatusType.ListenForEvents;
            Logger.LogInformation($"Statistic recalculation completed for {totalDaysCalculated} days ({totalDaysInserted} saved) by {(int)(DateTime.Now - stopwatchTotal).TotalSeconds} sec");
        }

        public StatisticServiceStatusType GetStatus() => Status;
    }
}
