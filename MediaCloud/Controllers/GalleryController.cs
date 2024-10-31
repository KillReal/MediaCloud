using MediaCloud.Builders.List;
using MediaCloud.Data.Models;
using MediaCloud.Repositories;
using MediaCloud.TaskScheduler;
using MediaCloud.WebApp.Services.UserProvider;
using MediaCloud.WebApp.Services.ConfigProvider;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IO.Compression;
using MediaCloud.WebApp.Services.TaskScheduler.Tasks;

namespace MediaCloud.WebApp.Controllers
{
    [Authorize]
    public class GalleryController(IConfigProvider configProvider, TagRepository tagRepository,
        PreviewRepository previewRepository, CollectionRepository collectionRepository,
        ITaskScheduler taskScheduler, IUserProvider userProvider, IAutotagService actorService) : Controller
    {
        private readonly IConfigProvider _configProvider = configProvider;
        private readonly TagRepository _tagRepository = tagRepository;
        private readonly PreviewRepository _previewRepository = previewRepository;
        private readonly ITaskScheduler _taskScheduler = taskScheduler;
        private readonly IUserProvider _userProvider = userProvider;
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
            var ListBuilder = new ListBuilder<Preview>(listRequest, _configProvider.UserSettings);
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
            var ListBuilder = new ListBuilder<Preview>(listRequest, _configProvider.UserSettings);
            var previews = await ListBuilder.BuildAsync(_previewRepository);

            return PartialView("/Pages/Gallery/_Gallery.cshtml", new _GalleryPageModel(previews));
        }

        public FileContentResult Preview(Guid id)
        {
            var preview = _previewRepository.Get(id);

            if (preview != null)
            {
                return new FileContentResult(preview.Content, "image/jpeg");
            }

            return new FileContentResult(System.IO.File.ReadAllBytes("wwwroot/img/types/noimg.jpg"), "image/jpeg");
        }

        public FileContentResult Source(Guid id)
        {
            var preview = _previewRepository.Get(id);

            if (preview != null)
            {
                return new FileContentResult(preview.Blob.Content, preview.BlobType);
            }

            return new FileContentResult(System.IO.File.ReadAllBytes("wwwroot/img/types/noimg.jpg"), "image/jpeg");
        }

        public IActionResult Download(Guid id)
        {
            var preview = _previewRepository.Get(id);

            if (preview != null)
            {
                if (preview.BlobName == "")
                {
                    preview.BlobName = preview.Id.ToString() + '.' + preview.BlobType.Split('/').Last();
                }

                return File(preview.Blob.Content, "application/octet-stream", preview.BlobName);
            }

            return new FileContentResult(System.IO.File.ReadAllBytes("wwwroot/img/types/noimg.jpg"), "image/jpeg");
        }

        public IActionResult DownloadCollection(Guid id)
        {
            var collection = _collectionRepository.Get(id);

            if (collection != null)
            {
                var fileName = collection.Id.ToString() + ".zip";
                using var stream = new MemoryStream();
                using (var zip = new ZipArchive(stream, ZipArchiveMode.Create, true))
                {
                    foreach (var preview in collection.Previews)
                    {
                        if (preview.BlobName == "")
                        {
                            preview.BlobName = preview.Id.ToString() + '.' + preview.BlobType.Split('/').Last();
                        }

                        var entry = zip.CreateEntry(preview.BlobName, CompressionLevel.Optimal);
                        using var file = entry.Open();
                        file.Write(preview.Blob.Content, 0, preview.Blob.Content.Length);
                    }
                }
                stream.Position = 0;
                return File(stream.ToArray(), "application/zip", fileName);
            }

            return new FileContentResult(System.IO.File.ReadAllBytes("wwwroot/img/types/noimg.jpg"), "image/jpeg");
        }

        public Guid AutocompleteTagForPreview(Guid previewId)
        {
            var task = new AutotagPreviewTask(_userProvider.GetCurrent(), [previewId]);

            return _taskScheduler.AddTask(task);
        }

        public List<Guid> AutocompleteTagForCollection(Guid collectionId)
        {
            var previewIds = _collectionRepository.Get(collectionId)?.Previews.Select(x => x.Id);
            
            if (previewIds == null || !previewIds.Any())
            {
                return [];
            }

            if (_configProvider.EnvironmentSettings.UseParallelProcessingForAutotagging)
            {
                var parts = _configProvider.EnvironmentSettings.AutotaggingMaxParallelDegree;
                var chunks = previewIds.Chunk(previewIds.Count() / parts + 1);
                var result = new List<Guid>();

                foreach (var chunk in chunks)
                {
                    var task = new AutotagPreviewTask(_userProvider.GetCurrent(), [.. chunk]);
                    result.Add(_taskScheduler.AddTask(task));
                }

                return result;
            }
            
            var singleTask = new AutotagPreviewTask(_userProvider.GetCurrent(), previewIds.ToList());

            return [_taskScheduler.AddTask(singleTask)];
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

            if (previewIds == null || previewIds.Count == 0)
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
