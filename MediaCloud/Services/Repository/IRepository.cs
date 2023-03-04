using MediaCloud.Data.Models;
using MediaCloud.Repositories;
using MediaCloud.WebApp.Repositories.Base;

namespace MediaCloud.WebApp.Services.Repository
{
    public interface IRepository
    {
        public ActorRepository Actors { get; }

        public CollectionRepository Collections { get; }

        public MediaRepository Medias { get; }

        public PreviewRepository Previews { get; }

        public TagRepository Tags { get; }
        public StatisticSnapshotRepository StatisticSnapshots { get; }

        public void SaveChanges();

        public Actor GetCurrentActor();

        public void SetCurrentActor(Actor actor);
    }
}
