using MediaCloud.Data.Models;
using MediaCloud.Repositories;
using MediaCloud.WebApp.Repositories.Base;

namespace MediaCloud.WebApp.Services.DataService
{
    public interface IDataService
    {
        public ActorRepository Actors { get;  }

        public CollectionRepository Collections { get; }

        public MediaRepository Medias { get; }

        public PreviewRepository Previews { get; }

        public TagRepository Tags { get; }
        public StatisticSnapshotRepository StatisticSnapshots { get; }

        public Actor GetCurrentActor();

        public long GetDbSize();
    }
}
