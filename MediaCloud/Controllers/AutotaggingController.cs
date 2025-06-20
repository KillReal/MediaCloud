using MediaCloud.Repositories;
using MediaCloud.TaskScheduler;
using MediaCloud.TaskScheduler.Tasks;
using MediaCloud.WebApp.Repositories;
using MediaCloud.WebApp.Services.ConfigProvider;
using MediaCloud.WebApp.Services.TaskScheduler.Tasks;
using MediaCloud.WebApp.Services.UserProvider;
using Microsoft.AspNetCore.Mvc;

namespace MediaCloud.WebApp.Controllers;

public class AutotaggingController(IUserProvider userProvider, IConfigProvider configProvider, IAutotagService autotagService, 
    ITaskScheduler taskScheduler, CollectionRepository collectionRepository) 
    : Controller
{
    private bool IsAutotaggingAllowed()
    {
        var currentUser = userProvider.GetCurrent();

        return configProvider.EnvironmentSettings.AutotaggingEnabled 
               && currentUser.IsAutotaggingAllowed;
    }
    
    public List<string> GetAliasSuggestions(string searchString, int limit = 10)
    {
        if (IsAutotaggingAllowed() == false)
        {
            return [];
        }

        return autotagService.GetSuggestionsByString(configProvider, searchString, limit);
    }
    
    public Guid GetTagsForPreview(Guid previewId)
        {
            if (IsAutotaggingAllowed() == false)
            {
                return Guid.Empty;
            }

            var task = new AutotagPreviewTask(userProvider.GetCurrent(), [previewId]);

            return taskScheduler.AddTask(task);
        }

        public Guid GetTagsForPreviewRange(List<Guid> previewsIds)
        {
            if (IsAutotaggingAllowed() == false)
            {
                return Guid.Empty;
            }

            var task = new AutotagPreviewTask(userProvider.GetCurrent(), previewsIds);

            return taskScheduler.AddTask(task);
        }

        public List<Guid> GetTagsForCollection(Guid collectionId)
        {
            if (IsAutotaggingAllowed() == false)
            {
                return [];
            }

            var previewIds = collectionRepository.Get(collectionId)?.Previews.Select(x => x.Id);
            
            if (previewIds == null || !previewIds.Any())
            {
                return [];
            }
            
            var task = new AutotagPreviewTask(userProvider.GetCurrent(), previewIds.ToList());

            return [taskScheduler.AddTask(task)];
        }

        public double GetPreviewTagsProcessingTime()
        {
            return autotagService.GetAverageExecutionTime();
        }

        public double GetCollectionTagsProcessingTime(int previewsCount)
        {
            return autotagService.GetAverageExecutionTime(previewsCount);
        }

        public bool IsPreviewProcessing(Guid previewId)
        {
            var taskStatuses = taskScheduler.GetStatus().TaskStatuses;

            return taskStatuses.Any(x => x.IsCompleted == false && x.AffectedEntities.Contains(previewId));
        }

        public bool IsCollectionProcessing(Guid collectionId)
        {
            var previewIds = collectionRepository.Get(collectionId)?.Previews.Select(x => x.Id).ToList();

            if (previewIds == null || previewIds.Count == 0)
            {
                return false;
            }

            var taskStatuses = taskScheduler.GetStatus().TaskStatuses;

            foreach (var previewId in previewIds)
            {
               if (taskStatuses.Any(x => x.IsCompleted == false && x.AffectedEntities.Contains(previewId)))
               {
                    return true;
               }
            }

            return false;
        }
        
        public Guid UpdateRatingsForAllPreviews()
        {
            if (userProvider.GetCurrent().IsAdmin == false)
            {
                return Guid.Empty;
            }
            
            if (IsAutotaggingAllowed() == false)
            {
                return Guid.Empty;
            }
            
            var task = new RatePreviewTask(userProvider.GetCurrent());

            return taskScheduler.AddTask(task);
        }
}