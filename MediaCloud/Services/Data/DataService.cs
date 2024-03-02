using MediaCloud.Data;
using MediaCloud.Data.Models;
using MediaCloud.Repositories;
using MediaCloud.WebApp.Services.ActorProvider;
using MediaCloud.WebApp.Services.DataService.Entities.Base;
using MediaCloud.WebApp.Services.Statistic;
using Microsoft.EntityFrameworkCore;

namespace MediaCloud.WebApp.Services.DataService
{
    public class DataService : IDataService
    {
        private readonly IServiceScope _serviceScope;

        private readonly DataServiceContext _dataServiceContext;
        private ActorDataService _actorDataService;
        private CollectionDataService _collectionDataService;
        private MediaDataService _mediaDataService;
        private PreviewDataService _previewDataService;
        private TagDataService _tagDataService;
        private StatisticSnapshotDataService _statisticSnapshotDataService;

        private void SetContext(DataServiceContext dataServiceContext)
        {
            _actorDataService = new(dataServiceContext.DbContext);
            _collectionDataService = new(dataServiceContext);
            _mediaDataService = new(dataServiceContext);
            _previewDataService = new(dataServiceContext);
            _tagDataService = new(dataServiceContext);
            _statisticSnapshotDataService = new(dataServiceContext);
        }

        public DataService(IServiceScopeFactory scopeFactory, ILogger<DataService> logger, IActorProvider actorProvider, IStatisticService statisticService)
        {
            _serviceScope = scopeFactory.CreateScope();
            var context = _serviceScope.ServiceProvider.GetRequiredService<AppDbContext>();
            _dataServiceContext = new DataServiceContext(context, statisticService, logger, actorProvider.GetCurrent());

            _actorDataService = new(_dataServiceContext.DbContext);
            _collectionDataService = new(_dataServiceContext);
            _mediaDataService = new(_dataServiceContext);
            _previewDataService = new(_dataServiceContext);
            _tagDataService = new(_dataServiceContext);
            _statisticSnapshotDataService = new(_dataServiceContext);
        }

        public ActorDataService Actors => _actorDataService;

        public CollectionDataService Collections => _collectionDataService;

        public MediaDataService Medias => _mediaDataService;

        public PreviewDataService Previews => _previewDataService;

        public TagDataService Tags => _tagDataService;

        StatisticSnapshotDataService IDataService.StatisticSnapshots => _statisticSnapshotDataService;

        public void SaveChanges() => _dataServiceContext.DbContext.SaveChanges();

        public Actor GetCurrentActor() => _dataServiceContext.Actor ?? new();

        public void SetCurrentActor(Actor actor)
        {
            _dataServiceContext.Actor = actor;
            SetContext(_dataServiceContext);
        }

        public long GetDbSize()
        {
            using var command = _dataServiceContext.DbContext.Database.GetDbConnection().CreateCommand();
            command.CommandText = $"SELECT pg_database_size('{_dataServiceContext.DbContext.Database.GetDbConnection().Database}');";
            _dataServiceContext.DbContext.Database.OpenConnection();

            using var reader = command.ExecuteReader();
            reader.Read();
            return reader.GetInt64(0);
        }
    }
}
