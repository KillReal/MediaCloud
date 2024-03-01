using MediaCloud.Data;
using MediaCloud.Data.Models;
using MediaCloud.Repositories;
using MediaCloud.WebApp.Services.Repository.Entities.Base;
using MediaCloud.WebApp.Services.Statistic;
using Microsoft.EntityFrameworkCore;

namespace MediaCloud.WebApp.Services.Repository
{
    public class Repository : IRepository
    {
        private IServiceScope ServiceScope;

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

        public Repository(IServiceScopeFactory scopeFactory, ILogger<Repository> logger, IActorProvider actorProvider, IStatisticService statisticService)
        {
            ServiceScope = scopeFactory.CreateScope();
            var context = ServiceScope.ServiceProvider.GetRequiredService<AppDbContext>();
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

        public long GetDbSize()
        {
            using (var command = RepositoryContext.DbContext.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = "SELECT pg_database_size('mediacloud');";
                RepositoryContext.DbContext.Database.OpenConnection();
                using (var reader = command.ExecuteReader())
                {
                    reader.Read();
                    return reader.GetInt64(0);
                }
            }
        }
    }
}
