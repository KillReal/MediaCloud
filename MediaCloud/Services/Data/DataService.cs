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

        private readonly RepositoriesContext _repositoriesContext;

        public DataService(IServiceScopeFactory scopeFactory, IActorProvider actorProvider, IStatisticService statisticService)
        {
            _serviceScope = scopeFactory.CreateScope();
            var dbContext = _serviceScope.ServiceProvider.GetRequiredService<AppDbContext>();
            _repositoriesContext = new RepositoriesContext(dbContext, statisticService, LogManager.GetLogger("DataService"), actorProvider.GetCurrent(dbContext));

            Actors = new(_repositoriesContext.DbContext);
            Collections = new(_repositoriesContext);
            Medias = new(_repositoriesContext);
            Previews = new(_repositoriesContext);
            Tags = new(_repositoriesContext);
            StatisticSnapshots = new(_repositoriesContext);
        }

        public ActorRepository Actors { get; init; }

        public CollectionRepository Collections { get; init; }

        public MediaRepository Medias { get; init; }

        public PreviewRepository Previews { get; init; }

        public TagRepository Tags { get; init; }

        public StatisticSnapshotRepository StatisticSnapshots { get; init; }

        public void SaveChanges() => _repositoriesContext.DbContext.SaveChanges();

        public Actor GetCurrentActor() => _repositoriesContext.Actor ?? new();

        public void SetCurrentActor(Actor actor)
        {
            _repositoriesContext.Actor = actor;
        }

        public long GetDbSize()
        {
            using var command = _repositoriesContext.DbContext.Database.GetDbConnection().CreateCommand();
            command.CommandText = $"SELECT pg_database_size('{_repositoriesContext.DbContext.Database.GetDbConnection().Database}');";
            _repositoriesContext.DbContext.Database.OpenConnection();

            using var reader = command.ExecuteReader();
            reader.Read();
            return reader.GetInt64(0);
        }
    }
}
