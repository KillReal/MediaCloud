using MediaCloud.Data.Models;
using MediaCloud.Repositories;
using MediaCloud.WebApp.Repositories.Base;

namespace MediaCloud.WebApp.Services.DataService
{
    public interface IDataService
    {
        public ActorRepository Actors { get; init; }

        public CollectionRepository Collections { get; init; }

        public MediaRepository Medias { get; init; }

        public PreviewRepository Previews { get; init; }

        public TagRepository Tags { get; init; }
        public StatisticSnapshotRepository StatisticSnapshots { get; init; }

        public void SaveChanges();

        public Actor GetCurrentActor();

        public void SetCurrentActor(Actor actor);

        public long GetDbSize();
    }
}
