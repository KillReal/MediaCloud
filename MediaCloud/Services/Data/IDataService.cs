using MediaCloud.Data.Models;
using MediaCloud.Repositories;
using MediaCloud.WebApp.Repositories.Base;

namespace MediaCloud.WebApp.Services.DataService
{
    public interface IDataService
    {
        public ActorDataService Actors { get; }

        public CollectionDataService Collections { get; }

        public MediaDataService Medias { get; }

        public PreviewDataService Previews { get; }

        public TagDataService Tags { get; }
        public StatisticSnapshotDataService StatisticSnapshots { get; }

        public void SaveChanges();

        public Actor GetCurrentActor();

        public void SetCurrentActor(Actor actor);

        public long GetDbSize();
    }
}
