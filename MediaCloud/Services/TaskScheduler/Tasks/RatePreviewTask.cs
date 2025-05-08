using DynamicExpression.Extensions;
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
    public class RatePreviewTask : Task
    {
        public RatePreviewTask(User user, List<Guid> previewsIds) : base(user)
        {
            AffectedEntities = previewsIds;
            _workCount = AffectedEntities.Count;
        }

        public RatePreviewTask(User user) : base(user)
        {
            AffectedEntities = new List<Guid>();
        }
        
        public new List<Guid> AffectedEntities;
        private int _workCount;

        public override int GetWorkCount()
        {
            return _workCount;
        }

        public override void DoTheTask(IServiceProvider serviceProvider, IUserProvider userProvider, StatisticProvider statisticProvider)
        {
            var context = serviceProvider.GetRequiredService<AppDbContext>();
            var autotagService = serviceProvider.GetRequiredService<IAutotagService>();
            
            var previewRepository = new PreviewRepository(context, statisticProvider, userProvider);
            var tagRepository = new TagRepository(context, statisticProvider, userProvider);

            var successfullyProceededCount = 0;
            var message = "";

            var previews = new List<Preview>();

            if (_workCount == 0)
            {
                AffectedEntities = context.Previews.Where(x => x.BlobType.ToLower().Contains("image")
                                                            && x.Creator == userProvider.GetCurrent())
                                                    .Select(x => x.Id).ToList();
                _workCount = AffectedEntities.Count;
            }

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

                if (result.IsSuccess)
                {
                    if (preview.Rating != result.Rating)
                    {
                        preview.Rating = result.Rating;
                        previewRepository.Update(preview);
                    }
                    
                    successfullyProceededCount++;
                    message += $"\nPreview {preview.Id}\nSuggested Rating: {result.Rating}";
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
