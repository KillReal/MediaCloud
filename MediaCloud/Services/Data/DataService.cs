using MediaCloud.Data;
using MediaCloud.Data.Models;
using MediaCloud.Repositories;
using MediaCloud.WebApp.Services.ActorProvider;
using MediaCloud.WebApp.Services.DataService.Entities.Base;
using MediaCloud.WebApp.Services.Statistic;
using Microsoft.EntityFrameworkCore;
using NLog;

namespace MediaCloud.WebApp.Services.DataService
{
    public class DataService : IDataService
    {
        private readonly IServiceScope _serviceScope;

        private readonly RepositoryContext _repositoryContext;

        public DataService(IServiceScopeFactory scopeFactory, IActorProvider actorProvider, IStatisticService statisticService)
        {
            _serviceScope = scopeFactory.CreateScope();

            var logger = LogManager.GetLogger("DataService");
            var dbContext = _serviceScope.ServiceProvider.GetRequiredService<AppDbContext>();
            var currentActor = actorProvider.GetCurrent(dbContext);
            _repositoryContext = new RepositoryContext(dbContext, statisticService, logger, currentActor);

            Actors = new(_repositoryContext.DbContext);
            Collections = new(_repositoryContext);
            Medias = new(_repositoryContext);
            Previews = new(_repositoryContext);
            Tags = new(_repositoryContext);
            StatisticSnapshots = new(_repositoryContext);

            logger.Debug("Initialized DataService instance by {currentActor.Name}", currentActor?.Name);
        }

        public ActorRepository Actors { get; } 
        public CollectionRepository Collections { get; }
        public MediaRepository Medias { get; }
        public PreviewRepository Previews { get; }
        public TagRepository Tags { get; }
        public StatisticSnapshotRepository StatisticSnapshots { get; }  

        public Actor GetCurrentActor() => _repositoryContext.Actor ?? new();

        public long GetDbSize()
        {
            using var command = _repositoryContext.DbContext.Database.GetDbConnection().CreateCommand();
            command.CommandText = $"SELECT pg_database_size('{_repositoryContext.DbContext.Database.GetDbConnection().Database}');";
            _repositoryContext.DbContext.Database.OpenConnection();

            using var reader = command.ExecuteReader();
            reader.Read();
            return reader.GetInt64(0);
        }
    }
}
