namespace MediaCloud.WebApp.Services.Statistic
{
    public partial class StatisticProvider
    {
        public Action<int, long> MediasCountChanged { get; set; }
        public Action<int> TagsCountChanged { get; set; }
        public Action<int> ActorsCountChanged { get; set; }
        public Action ActivityFactorRaised { get; set; }

        private void MediasCountChangedAction(int affectedCount, long affectedSize)
        {
            var snapshot = GetTodaySnapshot();

            snapshot.ActivityFactor += 1;
            snapshot.MediasCount += affectedCount;
            snapshot.MediasSize += affectedSize;

            CreateOrUpdateSnapshot(snapshot);
        }

        private void TagsCountChangedAction(int affectedCount)
        {
            var snapshot = GetTodaySnapshot();

            snapshot.ActivityFactor += 1;
            snapshot.TagsCount += affectedCount;

            CreateOrUpdateSnapshot(snapshot);
        }
        private void ActorsCountChangedAction(int affectedCount)
        {
            var snapshot = GetTodaySnapshot();

            snapshot.ActivityFactor += 1;
            snapshot.ActorsCount += affectedCount;

            CreateOrUpdateSnapshot(snapshot);
        }

        private void ActivityFactorRaisedAction()
        {
            var snapshot = GetTodaySnapshot();

            snapshot.ActivityFactor += 1;
            
            CreateOrUpdateSnapshot(snapshot);
        }

    }
}
