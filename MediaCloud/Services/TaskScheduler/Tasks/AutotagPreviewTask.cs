using MediaCloud.Data;
using MediaCloud.Data.Models;
using MediaCloud.Repositories;
using MediaCloud.TaskScheduler.Tasks;
using MediaCloud.WebApp.Repositories;
using MediaCloud.WebApp.Services.UserProvider;
using MediaCloud.WebApp.Services.Statistic;
using Microsoft.Extensions.Caching.Memory;
using Task = MediaCloud.TaskScheduler.Tasks.Task;

namespace MediaCloud.WebApp.Services.TaskScheduler.Tasks
{
    public class AutotagPreviewTask(User actor, List<Guid> previewsIds) : Task(actor), ITask
    {
        private int _workCount = previewsIds.Count;

        public override int GetWorkCount()
        {
            return _workCount;
        }

        public override void DoTheTask(IServiceProvider serviceProvider, IUserProvider userProvider, StatisticProvider statisticProvider)
        {
            var context = serviceProvider.GetRequiredService<AppDbContext>();
            var autotagService = serviceProvider.GetRequiredService<IAutotagService>();
            var memoryCache = serviceProvider.GetRequiredService<IMemoryCache>();
            
            var previewRepository = new PreviewRepository(context, statisticProvider, userProvider, memoryCache);
            var tagRepository = new TagRepository(context, statisticProvider, userProvider);

            var successfullyProceededCount = 0;
            var message = "";

            var previews = previewsIds.Select(id => previewRepository.Get(id))
                                            .OfType<Preview>()
                                            .ToList();
            
            foreach (var preview in previews)
            {
                var result = autotagService.AutotagPreview(preview, tagRepository);

                if (result.IsSuccess)
                {
                    var tags = preview.Tags.Union(result.Tags).ToList();
                    tagRepository.UpdatePreviewLinks(tags, preview);
                    
                    if (preview.Rating != result.Rating)
                    {
                        preview.Rating = result.Rating;
                        previewRepository.Update(preview);
                    }
                    
                    successfullyProceededCount++;
                    message += $"\nPreview {preview.Id}\nSuggested aliases: {result.SuggestedAliases}\nRating: {result.Rating}";
                }
                else 
                {
                    message += $"\nPreview {preview.Id} failed to proceed due to: {result.ErrorMessage}";
                }

                _workCount--;
            }

            CompletionMessage = $"Proceeded {successfullyProceededCount} previews [ {message} ]";
        }

    }
}
