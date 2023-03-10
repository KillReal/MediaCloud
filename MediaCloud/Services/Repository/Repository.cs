using MediaCloud.Data;
using MediaCloud.Data.Models;
using MediaCloud.Repositories;
using MediaCloud.WebApp.Services.Repository.Entities.Base;
using MediaCloud.WebApp.Services.Statistic;

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
        private StatisticSnapshotRepository StatisticSnapshotRepository { get; set; }

        private void SetContext(RepositoryContext repositoryContext)
        {
            ActorRepository = new(repositoryContext.DbContext);
            CollectionRepository = new(repositoryContext);
            MediaRepository = new(repositoryContext);
            PreviewRepository = new(repositoryContext);
            TagRepository = new(repositoryContext);
        }

        public Repository(AppDbContext context, ILogger<Repository> logger, IActorProvider actorProvider, IStatisticService statisticService)
        {
            RepositoryContext = new RepositoryContext(context, statisticService, logger, actorProvider.GetCurrent());

            SetContext(RepositoryContext);
        }

        public ActorRepository Actors => ActorRepository;

        public CollectionRepository Collections => CollectionRepository;

        public MediaRepository Medias => MediaRepository;

        public PreviewRepository Previews => PreviewRepository;

        public TagRepository Tags => TagRepository;

        StatisticSnapshotRepository IRepository.StatisticSnapshots => StatisticSnapshotRepository;

        public void SaveChanges() => RepositoryContext.DbContext.SaveChanges();

        public Actor? GetCurrentActor() => RepositoryContext.Actor;

        public void SetCurrentActor(Actor actor)
        {
            RepositoryContext.Actor = actor;
            SetContext(RepositoryContext);
        }
    }
}
