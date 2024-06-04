using MediaCloud.Builders.List;
using MediaCloud.Data.Models;
using MediaCloud.Repositories;
using MediaCloud.TaskScheduler;
using MediaCloud.WebApp.Services.ActorProvider;
using MediaCloud.WebApp.Services.ConfigurationProvider;
using MediaCloud.WebApp.Services.Statistic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MediaCloud.WebApp.Controllers
{
    [Authorize]
    public class GalleryController : Controller
    {
        private readonly IConfigProvider _configProvider;
        private readonly TagRepository _tagRepository;
        private readonly PreviewRepository _previewRepository;
        private readonly ITaskScheduler _taskScheduler;
        private readonly IActorProvider _actorProvider;
        private readonly IAutotagService _autotagService;

        public GalleryController(IConfigProvider configProvider, TagRepository tagRepository, 
            PreviewRepository previewRepository, ITaskScheduler taskScheduler, IActorProvider actorProvider,
            IAutotagService actorService)
        {
            _configProvider = configProvider;
            _tagRepository = tagRepository;
            _previewRepository = previewRepository;
            _taskScheduler = taskScheduler;
            _actorProvider = actorProvider;
            _autotagService = actorService;
        }

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
            var task = new AutocompleteTagsTask(_actorProvider.GetCurrent(), new() { previewId });

            return _taskScheduler.AddTask(task);
        }
    }
}
