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

        public override void DoTheTask(IServiceProvider serviceProvider, IUserProvider actorProvider)
        {
            var context = serviceProvider.GetRequiredService<AppDbContext>();

            var statisticProvider = new StatisticProvider(context, actorProvider);
            var previewRepository = new PreviewRepository(context, statisticProvider, actorProvider);
            var tagRepository = new TagRepository(context, statisticProvider, actorProvider);
            var autotagService = serviceProvider.GetRequiredService<IAutotagService>();

            while (AffectedEntities.Count != 0)
            {
                _aproximateExecutionTime = autotagService.GetAverageExecutionTime();
                var preview = previewRepository.Get(AffectedEntities.First());

                if (preview == null)
                {
                    AffectedEntities.Remove(AffectedEntities.First());
                    continue;
                }

                var result = autotagService.AutocompleteTags(preview, tagRepository);

                if (result.IsSuccess && result.Tags.Count != 0)
                {
                    var tags = preview.Tags.Union(result.Tags).ToList();
                    tagRepository.UpdatePreviewLinks(tags, preview);
                }

                AffectedEntities.Remove(AffectedEntities.First());

                if (result.IsSuccess == false)
                {
                    throw new Exception($"Autotagging failed due to: {result.ErrorMessage}");
                }
            }
        }

    }
}
