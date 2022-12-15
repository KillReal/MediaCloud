using MediaCloud.Data;
using MediaCloud.Repositories;
using MediaCloud.WebApp.Services.Repository.Entities.Base;

namespace MediaCloud.WebApp.Services.Repository
{
    public class Repository : IRepository
    {
        private ActorRepository ActorRepository { get; set; }
        private CollectionRepository CollectionRepository { get; set; }
        private MediaRepository MediaRepository { get; set; }
        private PreviewRepository PreviewRepository { get; set; }
        private TagRepository TagRepository { get; set; }

        public Repository(AppDbContext context, ILogger<Repository> logger, IActorProvider actorProvider)
        {
            var repositoryContext = new RepositoryContext(context, logger, actorProvider.GetCurrent());

            ActorRepository = new(repositoryContext.Context);
            CollectionRepository = new(repositoryContext);
            MediaRepository = new(repositoryContext);
            PreviewRepository = new(repositoryContext);
            TagRepository = new(repositoryContext);
        }

        public ActorRepository Actors => ActorRepository;

        public CollectionRepository Collections => CollectionRepository;

        public MediaRepository Medias => MediaRepository;

        public PreviewRepository Previews => PreviewRepository;

        public TagRepository Tags => TagRepository;

        public void SaveChanges() => CollectionRepository.SaveChanges();
    }
}
