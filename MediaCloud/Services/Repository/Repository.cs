using MediaCloud.Data;
using MediaCloud.Data.Models;
using MediaCloud.Repositories;
using MediaCloud.WebApp.Services.Repository.Entities.Base;

namespace MediaCloud.WebApp.Services.Repository
{
    public class Repository : IRepository
    {
        private RepositoryContext RepositoryContext { get; set; }
        private ActorRepository ActorRepository { get; set; }
        private CollectionRepository CollectionRepository { get; set; }
        private MediaRepository MediaRepository { get; set; }
        private PreviewRepository PreviewRepository { get; set; }
        private TagRepository TagRepository { get; set; }

        private void SetContext(RepositoryContext repositoryContext)
        {
            ActorRepository = new(repositoryContext.DbContext);
            CollectionRepository = new(repositoryContext);
            MediaRepository = new(repositoryContext);
            PreviewRepository = new(repositoryContext);
            TagRepository = new(repositoryContext);
        }

        public Repository(AppDbContext context, ILogger<Repository> logger, IActorProvider actorProvider)
        {
            RepositoryContext = new RepositoryContext(context, logger, actorProvider.GetCurrent());

            SetContext(RepositoryContext);
        }

        public ActorRepository Actors => ActorRepository;

        public CollectionRepository Collections => CollectionRepository;

        public MediaRepository Medias => MediaRepository;

        public PreviewRepository Previews => PreviewRepository;

        public TagRepository Tags => TagRepository;

        public void SaveChanges() => RepositoryContext.DbContext.SaveChanges();

        public Actor? GetCurrentActor() => RepositoryContext.Actor;

        public void SetCurrentActor(Actor actor)
        {
            RepositoryContext.Actor = actor;
            SetContext(RepositoryContext);
        }
    }
}
