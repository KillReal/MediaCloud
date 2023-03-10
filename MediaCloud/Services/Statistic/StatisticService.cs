using MediaCloud.Data;
using MediaCloud.Data.Models;
using MediaCloud.WebApp.Services.Repository;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace MediaCloud.WebApp.Services.Statistic
{
    public class StatisticService : IStatisticService
    {
        private StatisticServiceHelper ServiceHelper { get; set; }
        private ILogger Logger { get; set; }
        private StatisticSnapshot CurrentSnapshot { get; set; }
        private StatisticServiceStatusType Status { get; set; } = StatisticServiceStatusType.ListenForEvents;

        public StatisticService(AppDbContext context, ILogger<StatisticService> logger) 
        {
            ServiceHelper = new(context);
            Logger = logger;
            CurrentSnapshot = ServiceHelper.GetLastOrNew();

            if (CurrentSnapshot == null)
            {
                CurrentSnapshot = new StatisticSnapshot();
                ServiceHelper.AppendOrUpdate(CurrentSnapshot);
            }
        }

        public void NotifyMediasCountChanged(int affectedCount)
        {
            CurrentSnapshot.MediasCount += affectedCount;
            Logger.LogInformation($"Affected {affectedCount} medias");
            ServiceHelper.AppendOrUpdate(CurrentSnapshot);
        }

        public void NotifyTagsCountChanged(int affectedCount)
        {
            CurrentSnapshot.TagsCount += affectedCount;
            ServiceHelper.AppendOrUpdate(CurrentSnapshot);
        }
        public void NotifyActorsCountChanged(int affectedCount)
        {
            CurrentSnapshot.ActorsCount += affectedCount;
            ServiceHelper.AppendOrUpdate(CurrentSnapshot);
        }

        public void NotifyActivityFactorRaised()
        {
            CurrentSnapshot.ActivityFactor += 1;
            ServiceHelper.AppendOrUpdate(CurrentSnapshot);
        }

        public StatisticSnapshot GetCurrentStatistic()
        {
            return CurrentSnapshot;
        }
        public List<StatisticSnapshot> GetStatistic()
        {
            var startDate = ServiceHelper.GetFirstOrNowDate();
            var endDate = DateTime.Now;

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
            Console.WriteLine($"Statistic recalculation started");

            var totalDaysCalculated = 0;
            var prevSnapshot = new StatisticSnapshot();
            var date = startDate;

            Task.Run(() =>
            {
                do
                {
                    var stopwatch = DateTime.Now;
                    Console.WriteLine($"Calculating statistic for {date.Date}");
                    var statisticSnapshot = ServiceHelper.CalculateAsync(date).Result;
                    ServiceHelper.AppendOrUpdate(statisticSnapshot.AppendParameters(prevSnapshot), date);

                    prevSnapshot = statisticSnapshot;
                    date = date.AddDays(1);
                    totalDaysCalculated++;
                    Console.WriteLine($"Calculating done by {(DateTime.Now - stopwatch).TotalSeconds}");
                } while (date.Date <= DateTime.Now);
            }).Wait();

            Status = StatisticServiceStatusType.ListenForEvents;
            Console.WriteLine($"Statistic recalculation completed with {totalDaysCalculated} days");
        }

        public StatisticServiceStatusType GetStatus() => Status;
    }
}
