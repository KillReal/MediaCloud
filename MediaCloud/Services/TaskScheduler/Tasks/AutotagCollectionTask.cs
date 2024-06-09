using MediaCloud.Data;
using MediaCloud.Data.Models;
using MediaCloud.Repositories;
using MediaCloud.Services;
using MediaCloud.TaskScheduler.Tasks;
using MediaCloud.WebApp.Services.ActorProvider;
using MediaCloud.WebApp.Services.ConfigurationProvider;
using MediaCloud.WebApp.Services.Statistic;
using Task = MediaCloud.TaskScheduler.Tasks.Task;

namespace MediaCloud.WebApp;

public class AutotagCollectionTask : Task, ITask
{
    private Guid _collectionId = Guid.Empty;
    private int _parallelMaxDegree = 1;
    private int _workCount;
    
    public override int GetWorkCount() => _workCount;

    public AutotagCollectionTask(Actor actor, Guid collectionId) : base(actor)
    {
        _collectionId = collectionId;
    }
    public override void DoTheTask(IServiceProvider serviceProvider, IActorProvider actorProvider)
    {
        var context = serviceProvider.GetRequiredService<AppDbContext>();
        
        var statisticProvider = new StatisticProvider(context, actorProvider);
        var collectionRepository = new CollectionRepository(context, statisticProvider, actorProvider);
        var tagRepository = new TagRepository(context, statisticProvider, actorProvider);
        var autotagService = serviceProvider.GetRequiredService<IAutotagService>();
        var configProvider = serviceProvider.GetRequiredService<IConfigProvider>();

        _parallelMaxDegree = configProvider.EnvironmentSettings.TaskSchedulerAutotaggingWorkerCount / 2;

        var collection = collectionRepository.Get(_collectionId);
        var titlePreview = collection?.Previews.FirstOrDefault(p => p.Order == 0);

        if (collection != null && titlePreview != null)
        {
            var previews = collection.Previews.ToList();
            var tags = new List<Tag>();

            _workCount = previews.Count;

            tags = autotagService.AutocompleteTagsForCollection(collection, tagRepository, _parallelMaxDegree);           
            tagRepository.UpdatePreviewLinks(tags, titlePreview);
        }
    }

}
