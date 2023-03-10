using MediaCloud.Data.Models;

namespace MediaCloud.WebApp.Services.Statistic
{
    public interface IStatisticService
    {
        public void NotifyMediasCountChanged(int affectedCount);
        public void NotifyTagsCountChanged(int affectedCount);
        public void NotifyActorsCountChanged(int affectedCount);
        public void NotifyActivityFactorRaised();
        public StatisticSnapshot GetCurrentStatistic();
        public List<StatisticSnapshot> GetStatistic();
        public List<StatisticSnapshot> GetStatistic(DateTime startDate, DateTime endDate);
        public void ProceedRecalculaton();
        public StatisticServiceStatusType GetStatus();
    }
}
