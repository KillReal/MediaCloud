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
        var pictureService = serviceProvider.GetRequiredService<IPictureService>();
        
        var statisticProvider = new StatisticProvider(context, actorProvider);
        var previewRepository = new PreviewRepository(context, statisticProvider, actorProvider);
        var tagRepository = new TagRepository(context, statisticProvider, actorProvider);
        var autotagService = new AutotagService(pictureService, previewRepository);

        foreach (var previewId in _previewIds)
        {
            var tagAliases = autotagService.AutocompleteTagsForImage(previewId);
            var suggestedTags = tagRepository.GetRangeByAliasString(string.Join(" ", tagAliases));

            if (suggestedTags.Any() == false)
            {
                _workCount -= 1;
                continue;
            }
            
            var preview = previewRepository.Get(previewId);

            if (preview == null)
            {
                _workCount -= 1;
                continue;
            }

            var tags = preview.Tags.Union(suggestedTags).ToList();
            tagRepository.UpdatePreviewLinks(tags, preview);

            _workCount -= 1;
        }
    }

}
