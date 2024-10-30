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

    public override void DoTheTask(IServiceProvider serviceProvider, IUserProvider userProvider)
    {
        var context = serviceProvider.GetRequiredService<AppDbContext>();
        
        var statisticProvider = new StatisticProvider(context, userProvider);
        var previewRepository = new PreviewRepository(context, statisticProvider, userProvider);
        var collectionRepository = new CollectionRepository(context, statisticProvider, userProvider);
        var tagRepository = new TagRepository(context, statisticProvider, userProvider);
        var autotagService = serviceProvider.GetRequiredService<IAutotagService>();
        var configProvider = serviceProvider.GetRequiredService<IConfigProvider>();

        var collection = collectionRepository.Get(_collectionId);
        var titlePreview = collection?.Previews.FirstOrDefault(p => p.Order == 0);

        if (collection != null && titlePreview != null)
        {
            var previews = collection.Previews.ToList();
            var tags = new List<Tag>();

            _aproximateExecutionTime = autotagService.GetAverageExecutionTime(previews.Count);
            var results = autotagService.AutocompleteTagsForCollection(collection.Previews, tagRepository);

            if (results.Any(x => x.IsSuccess) && results.Any(x => x.Tags.Count != 0))
            {
                foreach (var result in results)
                {
                    var preview = previewRepository.Get(result.PreviewId);
                    
                    if (preview == null)
                    {
                        result.IsSuccess = false;
                        result.ErrorMessage = $"Preview with id: {result.PreviewId} not found in database";
                    }
                    else 
                    {
                        tagRepository.UpdatePreviewLinks(result.Tags, preview);
                    }
                }
            }
            else if (results.Any(x => x.IsSuccess) == false)
            {
                var errorsListString = string.Join("], [", results.Select(x => x.ErrorMessage));
                throw new Exception($"Autotagging failed due to errors: [{errorsListString}]");
            }
        }
    }

}
