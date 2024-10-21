using MediaCloud.Data;
using MediaCloud.Data.Models;
using MediaCloud.Repositories;
using MediaCloud.TaskScheduler.Tasks;
using MediaCloud.WebApp.Services.UserProvider;
using MediaCloud.WebApp.Services.ConfigProvider;
using MediaCloud.WebApp.Services.Statistic;
using Task = MediaCloud.TaskScheduler.Tasks.Task;

namespace MediaCloud.WebApp;

public class AutotagCollectionTask(User actor, Guid collectionId) : Task(actor), ITask
{
    private readonly Guid _collectionId = collectionId;
    private double _aproximateExecutionTime;

    public override int GetWorkCount() 
    {
        if (ExecutedAt == DateTime.MinValue)
        {
            return 100;
        }

        var time = (DateTime.Now - ExecutedAt).TotalSeconds;

        if (time > _aproximateExecutionTime)
        {
            return 99;
        }

        var progress = (double)(time / _aproximateExecutionTime) * 100;

        return 100 - (int)Math.Clamp(progress, 0, 100);
    }

    public override void DoTheTask(IServiceProvider serviceProvider, IUserProvider actorProvider)
    {
        var context = serviceProvider.GetRequiredService<AppDbContext>();
        
        var statisticProvider = new StatisticProvider(context, actorProvider);
        var collectionRepository = new CollectionRepository(context, statisticProvider, actorProvider);
        var tagRepository = new TagRepository(context, statisticProvider, actorProvider);
        var autotagService = serviceProvider.GetRequiredService<IAutotagService>();
        var configProvider = serviceProvider.GetRequiredService<IConfigProvider>();

        var collection = collectionRepository.Get(_collectionId);
        var titlePreview = collection?.Previews.FirstOrDefault(p => p.Order == 0);

        if (collection != null && titlePreview != null)
        {
            var previews = collection.Previews.ToList();
            var tags = new List<Tag>();

            _aproximateExecutionTime = autotagService.GetAverageExecutionTime(previews.Count);

            ExecutedAt = DateTime.Now;
            tags = autotagService.AutocompleteTagsForCollection(collection.Previews, tagRepository);           
            tagRepository.UpdatePreviewLinks(tags, titlePreview);
        }
    }

}
