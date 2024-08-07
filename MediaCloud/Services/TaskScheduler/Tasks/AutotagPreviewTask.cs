﻿using MediaCloud.Data;
using MediaCloud.Data.Models;
using MediaCloud.Repositories;
using MediaCloud.Services;
using MediaCloud.TaskScheduler.Tasks;
using MediaCloud.WebApp.Services.ActorProvider;
using MediaCloud.WebApp.Services.ConfigProvider;
using MediaCloud.WebApp.Services.Statistic;
using Task = MediaCloud.TaskScheduler.Tasks.Task;

namespace MediaCloud.WebApp;

public class AutotagPreviewTask : Task, ITask
{
    private readonly List<Guid> _previewIds = new();
    private double _aproximateExecutionTime;
    
    public override int GetWorkCount() 
    {
        if (ExecutedAt == DateTime.MinValue)
        {
            return 0;
        }

        var time = (DateTime.Now - ExecutedAt).TotalSeconds;

        if (time > _aproximateExecutionTime)
        {
            return 99;
        }

        var progress = (double)(time / _aproximateExecutionTime) * 100;

        return (int)Math.Clamp(progress, 0, 100);
    }

    public AutotagPreviewTask(Actor actor, List<Guid> previewsIds) 
        : base(actor)
    {
        _previewIds = previewsIds;
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
            _aproximateExecutionTime = autotagService.GetAverageExecutionTime();
            var preview = previewRepository.Get(_previewIds.First());

            if (preview == null)
            {
                _previewIds.Remove(_previewIds.First());
                continue;
            }

            ExecutedAt = DateTime.Now;
            var suggestedTags = autotagService.AutocompleteTagsForPreview(preview, tagRepository);

            if (suggestedTags.Any())
            {
                var tags = preview.Tags.Union(suggestedTags).ToList();
                tagRepository.UpdatePreviewLinks(tags, preview);
            }

            _previewIds.Remove(_previewIds.First());
        }
    }

}
