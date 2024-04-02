using MediaCloud.Data;
using MediaCloud.Data.Models;
using MediaCloud.Repositories;
using MediaCloud.Services;
using MediaCloud.WebApp.Services.ActorProvider;
using MediaCloud.WebApp.Services.ConfigurationProvider;
using MediaCloud.WebApp.Services.DataService.Entities.Base;
using MediaCloud.WebApp.Services.Statistic;
using Microsoft.EntityFrameworkCore;
using NLog;

namespace MediaCloud.WebApp.Services.DataService
{
    public class DataService : IDataService
    {
        private readonly IActorProvider _actorProvider;
        private readonly RepositoryContext _repositoryContext;
        private readonly IConfigProvider _configProvider;

        public DataService(IServiceScopeFactory scopeFactory, IActorProvider actorProvider)
        {
            var logger = LogManager.GetLogger("DataService");

            var serviceScope = scopeFactory.CreateScope();
            var dbContext = serviceScope.ServiceProvider.GetRequiredService<AppDbContext>();
            _configProvider = serviceScope.ServiceProvider.GetRequiredService<IConfigProvider>();
            var pictureService = serviceScope.ServiceProvider.GetRequiredService<IPictureService>();

            _actorProvider = actorProvider;
            var currentActor = _actorProvider.GetCurrent();
            StatisticProvider = new StatisticProvider(dbContext, currentActor);
            _repositoryContext = new RepositoryContext(dbContext, StatisticProvider, pictureService, logger, currentActor);

            Actors = new(_repositoryContext.DbContext);
            Collections = new(_repositoryContext);
            Medias = new(_repositoryContext);
            Previews = new(_repositoryContext);
            Tags = new(_repositoryContext);

            ActorSettings = _configProvider.ActorSettings;
            EnvironmentSettings = _configProvider.EnvironmentSettings;

            logger.Debug("Initialized DataService instance by {currentActor.Name}", currentActor?.Name);
        }

        public ActorRepository Actors { get; } 
        public CollectionRepository Collections { get; }
        public MediaRepository Medias { get; }
        public PreviewRepository Previews { get; }
        public TagRepository Tags { get; }
        public StatisticProvider StatisticProvider { get; }

        public ActorSettings ActorSettings { get; }
        public EnvironmentSettings EnvironmentSettings { get; }

        public Actor GetCurrentActor() => _repositoryContext.Actor 
            ?? throw new NullReferenceException("Cannot get unknown actor");

        public long GetDbSize()
        {
            using var command = _repositoryContext.DbContext.Database.GetDbConnection().CreateCommand();
            var databaseSize = _repositoryContext.DbContext.Database.GetDbConnection().Database;

            command.CommandText = $"SELECT pg_database_size('{databaseSize}');";
            _repositoryContext.DbContext.Database.OpenConnection();

            using var reader = command.ExecuteReader();
            reader.Read();
            return reader.GetInt64(0);
        }

        public void SaveActorSettings(ActorSettings settings)
        {
            _configProvider.ActorSettings = settings;
            _configProvider.SaveActorSettings(_actorProvider);
        }

        public void SaveEnvironmentSettings(EnvironmentSettings settings)
        {

        }
    }
}
