using DynamicExpression.Extensions;
using MediaCloud.Data;
using MediaCloud.Data.Models;
using MediaCloud.Repositories;
using MediaCloud.TaskScheduler.Tasks;
using MediaCloud.WebApp.Data.Types;
using MediaCloud.WebApp.Repositories;
using MediaCloud.WebApp.Services.UserProvider;
using MediaCloud.WebApp.Services.Statistic;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
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
            var memoryCache = serviceProvider.GetRequiredService<IMemoryCache>();
            
            var configProvider = new ConfigProvider.ConfigProvider(serviceProvider.GetRequiredService<IConfiguration>(), userProvider);
            var model = string.IsNullOrWhiteSpace(configProvider.UserSettings.AutotaggingAiModel)
                ? configProvider.EnvironmentSettings.AutotaggingAiModel
                : configProvider.UserSettings.AutotaggingAiModel;
            var confidence = configProvider.UserSettings.AutotaggingAiModelConfidence == 0.0
                ? configProvider.EnvironmentSettings.AutotaggingAiModelConfidence
                : configProvider.UserSettings.AutotaggingAiModelConfidence;
            
            if (model == null)
            {
                CompletionMessage = $"Need to select AI model in user or environment settings before run the task.";
                return;
            }
            
            var previewRepository = new PreviewRepository(context, statisticProvider, userProvider, memoryCache);
            var tagRepository = new TagRepository(context, statisticProvider, userProvider);

            var successfullyProceededCount = 0;
            var message = "";

            if (_workCount == 0)
            {
                AffectedEntities = context.Previews.Where(x => x.BlobType.ToLower().Contains("image")
                                                            && x.Rating == PreviewRatingType.Unknown
                                                            && x.Creator == userProvider.GetCurrent())
                                                    .Select(x => x.Id)
                                                    .ToList();
                _workCount = AffectedEntities.Count;
            }

            foreach (var id in AffectedEntities)
            {
                var preview = previewRepository.Get(id);

                if (preview == null)
                {
                    continue;
                }

                var result = autotagService.AutotagPreview(preview, tagRepository, model, confidence);

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
            
            CompletionMessage = $"Proceeded {successfullyProceededCount} previews <{model}> and <{confidence}> confidence [ {message} ]";
        }

    }
}
