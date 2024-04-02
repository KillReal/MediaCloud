using MediaCloud.Data.Models;
using MediaCloud.Repositories;
using MediaCloud.WebApp.Repositories.Base;
using MediaCloud.WebApp.Services.ConfigurationProvider;
using MediaCloud.WebApp.Services.Statistic;

namespace MediaCloud.WebApp.Services.DataService
{
    public interface IDataService
    {
        public ActorRepository Actors { get;  }
        public CollectionRepository Collections { get; }
        public MediaRepository Medias { get; }
        public PreviewRepository Previews { get; }
        public TagRepository Tags { get; }
        public StatisticProvider StatisticProvider { get; }
        public ActorSettings ActorSettings { get; }
        public EnvironmentSettings EnvironmentSettings { get; }

        public Actor GetCurrentActor();
        public long GetDbSize();
        public void SaveActorSettings(ActorSettings settings);
        public void SaveEnvironmentSettings(EnvironmentSettings settings);
    }
}
