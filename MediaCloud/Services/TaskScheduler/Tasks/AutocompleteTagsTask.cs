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

public class AutocompleteTagsTask : Task, ITask
{
    private List<Guid> _previewIds = new();
    private int _workCount;
    
    public override int GetWorkCount() => _workCount;

    public AutocompleteTagsTask(Actor actor, List<Guid> previewsIds) : base(actor)
    {
        _previewIds = previewsIds;
        _workCount = _previewIds.Count;
    }

    public override void DoTheTask(IServiceProvider serviceProvider, IActorProvider actorProvider)
    {
        var context = serviceProvider.GetRequiredService<AppDbContext>();
        
        var statisticProvider = new StatisticProvider(context, actorProvider);
        var previewRepository = new PreviewRepository(context, statisticProvider, actorProvider);
        var tagRepository = new TagRepository(context, statisticProvider, actorProvider);
        var autotagService = serviceProvider.GetRequiredService<IAutotagService>();

        while (_previewIds.Any())
        {
            var preview = previewRepository.Get(_previewIds.First());

            if (preview == null)
            {
                continue;
            }

            var suggestedTags = autotagService.AutocompleteTagsForPreview(preview, tagRepository);

            if (suggestedTags.Any() == false)
            {
                continue;
            }

            var tags = preview.Tags.Union(suggestedTags).ToList();
            tagRepository.UpdatePreviewLinks(tags, preview);

            _previewIds.Remove(preview.Id);
        }
    }

}
