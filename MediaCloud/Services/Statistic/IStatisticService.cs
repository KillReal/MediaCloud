using MediaCloud.Data.Models;

namespace MediaCloud.WebApp.Services.Statistic
{
    public interface IStatisticService
    {
        public Action<int> MediasCountChanged { get; set; }
        public Action<int> TagsCountChanged { get; set; }
        public Action<int> ActorsCountChanged { get; set; }
        public Action ActivityFactorRaised { get; set; }

        public StatisticSnapshot GetTodayStatistic();
        public List<StatisticSnapshot> GetStatistic();
        public List<StatisticSnapshot> GetStatistic(DateTime startDate, DateTime endDate);
        public void ProceedRecalculaton();
        public void ProceedRecalculaton(DateTime startDate);
        public StatisticServiceStatusType GetStatus();
    }
}