using MediaCloud.WebApp.Services.Repository;
using StatisticSnapshotModel = MediaCloud.Data.Models.StatisticSnapshot;

namespace MediaCloud.WebApp.Builders.StatisticSnapshot
{
    public class StatisticSnapshotBuilder
    {
        private IRepository Repository { get; set; }
        private StatisticSnapshotModel? CurrentSnapshot { get; set; }

        private async Task<StatisticSnapshotModel> RecalculateSnapshot()
        {
            var snapshot = new StatisticSnapshotModel();
            snapshot.MediasCount = await Repository.Previews.GetListCountAsync(new(new()));
            snapshot.TagsCount = await Repository.Tags.GetListCountAsync(new(new()));
            snapshot.ActorsCount = await Repository.Actors.GetListCountAsync(new(new()));

            return snapshot;
        }

        public StatisticSnapshotBuilder(IRepository repository) 
        {
            Repository = repository;
            CurrentSnapshot = repository.StatisticSnapshots.GetLast();
        }

        public async Task CommitSnapshotAsync()
        {
            if (CurrentSnapshot == null)
            {
                CurrentSnapshot = await RecalculateSnapshot();
            }

            Repository.StatisticSnapshots.Append(CurrentSnapshot);
        }
    }
}
