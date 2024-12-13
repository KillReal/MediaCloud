using MediaCloud.Data;
using MediaCloud.Data.Models;
using MediaCloud.Repositories;
using MediaCloud.TaskScheduler.Tasks;
using MediaCloud.WebApp.Services.UserProvider;
using MediaCloud.WebApp.Services.Statistic;
using Task = MediaCloud.TaskScheduler.Tasks.Task;

namespace MediaCloud.WebApp.Services.TaskScheduler.Tasks
{
    public class AutotagPreviewTask(User actor, List<Guid> previewsIds) : Task(actor), ITask
    {
        public new List<Guid> AffectedEntities = previewsIds;
        private double _aproximateExecutionTime;

        public override int GetWorkCount()
        {
            if (IsCompleted)
            {
                return 0;
            }

            if (ExecutedAt == DateTime.MinValue)
            {
                return 100;
            }

            var time = (DateTime.Now - ExecutedAt).TotalSeconds;

            if (time > _aproximateExecutionTime)
            {
                return 1;
            }

            var progress = (double)(time / _aproximateExecutionTime) * 100;

            return 100 - (int)Math.Clamp(progress, 0, 100);
        }

        public override void DoTheTask(IServiceProvider serviceProvider, IUserProvider userProvider)
        {
            var context = serviceProvider.GetRequiredService<AppDbContext>();

            var statisticProvider = new StatisticProvider(context, userProvider);
            var previewRepository = new PreviewRepository(context, statisticProvider, userProvider);
            var tagRepository = new TagRepository(context, statisticProvider, userProvider);
            var autotagService = serviceProvider.GetRequiredService<IAutotagService>();

            var successfullyProceededCount = 0;
            var message = "";

            var previews = new List<Preview>();

            foreach (var id in AffectedEntities)
            {
                var preview = previewRepository.Get(id);

                if (preview == null)
                {
                    continue;
                }

                previews.Add(preview);
            }

            var results = autotagService.AutotagPreviewRange(previews, tagRepository);

            foreach (var result in results)
            {
                var preview = previews.First(x => x.Id == result.PreviewId);

                if (result.IsSuccess && result.Tags.Count != 0)
                {
                    var tags = preview.Tags.Union(result.Tags).ToList();
                    tagRepository.UpdatePreviewLinks(tags, preview);
                    successfullyProceededCount++;
                    message += $"\nPreview {preview.Id}\nSuggested aliases: {result.SuggestedAliases}";
                }
                else 
                {
                    message += $"\nPreview {preview.Id} failed to proceed due to: {result.ErrorMessage}";
                }
            }

            CompletionMessage = $"Proceeded {successfullyProceededCount} previews [ {message} ]";
        }

    }
}
