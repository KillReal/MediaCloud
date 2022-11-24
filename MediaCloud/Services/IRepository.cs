using MediaCloud.Repositories;

namespace MediaCloud.WebApp.Services
{
    public interface IRepository
    {
        public ActorRepository Actors { get; }

        public CollectionRepository Collections { get; }

        public MediaRepository Medias { get; }

        public PreviewRepository Previews { get; }

        public TagRepository Tags { get; }

        public void SaveChanges();
    }
}
