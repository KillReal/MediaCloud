using MediaCloud.Builders.List;
using MediaCloud.Data.Models;
using MediaCloud.Repositories;
using MediaCloud.TaskScheduler;
using MediaCloud.WebApp.Services.ActorProvider;
using MediaCloud.WebApp.Services.ConfigProvider;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MediaCloud.WebApp.Controllers
{
    [Authorize]
    public class GalleryController(IConfigProvider configProvider, TagRepository tagRepository,
        PreviewRepository previewRepository, CollectionRepository collectionRepository,
        ITaskScheduler taskScheduler, IActorProvider actorProvider, IAutotagService actorService) : Controller
    {
        private readonly IConfigProvider _configProvider = configProvider;
        private readonly TagRepository _tagRepository = tagRepository;
        private readonly PreviewRepository _previewRepository = previewRepository;
        private readonly ITaskScheduler _taskScheduler = taskScheduler;
        private readonly IActorProvider _actorProvider = actorProvider;
        private readonly IAutotagService _autotagService = actorService;
        private readonly CollectionRepository _collectionRepository = collectionRepository;

        public List<string> GetSuggestions(string searchString, int limit = 10)
        {
            return _tagRepository.GetSuggestionsByString(searchString, limit);
        }

        public List<string> GetAliasSuggestions(string searchString, int limit = 10)
        {
            return _autotagService.GetSuggestionsByString(searchString, limit);
        }

        public async Task<List<object>> PreviewsBatchAsync(ListRequest listRequest)
        {
            var ListBuilder = new ListBuilder<Preview>(listRequest, _configProvider.ActorSettings);
            var previews = await ListBuilder.BuildAsync(_previewRepository);

            var jsonPreviews = new List<object>();
            foreach (var preview in previews)
            {
                jsonPreviews.Add(new
                {
                    preview.Id,
                    Collection = new
                    {
                        preview.Collection?.Id,
                        preview.Collection?.Count
                    },
                    preview.Content,
                });
            }

            return jsonPreviews;
        }

        public async Task<ActionResult> GetBatchAsync(ListRequest listRequest)
        {
            var ListBuilder = new ListBuilder<Preview>(listRequest, _configProvider.ActorSettings);
            var previews = await ListBuilder.BuildAsync(_previewRepository);

            return PartialView("/Pages/Medias/_Gallery.cshtml", new _GalleryPageModel(previews));
        }

        public FileContentResult Preview(Guid id)
        {
            var preview = _previewRepository.Get(id);

            if (preview != null)
            {
                return new FileContentResult(preview.Content, "image/jpeg");
            }

            return new FileContentResult(System.IO.File.ReadAllBytes("wwwroot/noimg.jpg"), "image/jpeg");
        }

        public FileContentResult Source(Guid id)
        {
            var preview = _previewRepository.Get(id);

            if (preview != null)
            {
                return new FileContentResult(preview.Media.Content, "image/jpeg");
            }

            return new FileContentResult(System.IO.File.ReadAllBytes("wwwroot/noimg.jpg"), "image/jpeg");
        }

        public Guid AutocompleteTagForMedia(Guid previewId)
        {
            var task = new AutotagPreviewTask(_actorProvider.GetCurrent(), [previewId]);

            return _taskScheduler.AddTask(task);
        }

        public Guid AutocompleteTagForCollection(Guid collectionId)
        {
            var task = new AutotagCollectionTask(_actorProvider.GetCurrent(), collectionId);

            return _taskScheduler.AddTask(task);
        }

        public double GetAverageAutocompleteTagExecution()
        {
            return _autotagService.GetAverageExecutionTime();
        }

        public double GetAverageAutocompleteTagForCollectionExecution(int previewsCount)
        {
            return _autotagService.GetAverageExecutionTime(previewsCount);
        }

        public bool IsPreviewAutotaggingExecuted(Guid previewId)
        {
            return _autotagService.IsPreviewIsProceeded(previewId);
        }

        public bool IsCollectionAutotaggingExecuted(Guid collectionId)
        {
            var previewIds = _collectionRepository.Get(collectionId)?.Previews.Select(x => x.Id).ToList();

            if (previewIds == null || previewIds.Any() == false)
            {
                return false;
            }

            foreach (var previewId in previewIds)
            {
               if (_autotagService.IsPreviewIsProceeded(previewId))
               {
                    return true;
               }
            }

            return false;
        }
    }
}
