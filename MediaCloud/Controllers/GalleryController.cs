using MediaCloud.Builders.List;
using MediaCloud.Data.Models;
using MediaCloud.Repositories;
using MediaCloud.WebApp.Services.UserProvider;
using MediaCloud.WebApp.Services.ConfigProvider;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IO.Compression;
using MediaCloud.Extensions;
using MediaCloud.WebApp.Services.Statistic;
using MediaCloud.WebApp.Repositories;

namespace MediaCloud.WebApp.Controllers
{
    [Authorize]
    public class GalleryController : Controller
    {
        private readonly IConfigProvider _configProvider;
        private readonly TagRepository _tagRepository;
        private readonly PreviewRepository _previewRepository;
        private readonly CollectionRepository _collectionRepository;
        private readonly IUserProvider _userProvider;
        private readonly StatisticProvider _statisticProvider;

        private readonly List<string> _customAliases = [];
        
        public GalleryController(IConfigProvider configProvider, TagRepository tagRepository,
            PreviewRepository previewRepository, CollectionRepository collectionRepository,
            IUserProvider userProvider,
            StatisticProvider statisticProvider)
        {
            _configProvider = configProvider;
            _tagRepository = tagRepository;
            _previewRepository = previewRepository;
            _collectionRepository = collectionRepository;
            _userProvider = userProvider;
            _statisticProvider = statisticProvider;
            
            _customAliases.AddRange(TagFiltration<Preview>.GetAliasSuggestions().Select(x => new string(x)));

            if (userProvider.GetCurrent().IsAutotaggingAllowed)
            {
                _customAliases.AddRange(RatingFiltration<Preview>.GetAliasSuggestions());
            }
        }
        
        public List<string> GetTagSuggestions(string searchString, int limit = 10)
        {
            var tags = _tagRepository.GetSuggestionsByString(searchString, limit);
            
            return tags.Union(_customAliases).ToList();
        }

        public async Task<List<object>> PreviewsBatchAsync(ListRequest listRequest)
        {
            var listBuilder = new ListBuilder<Preview>(listRequest, _configProvider.UserSettings);
            var previews = await listBuilder.BuildAsync(_previewRepository);

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

        public async Task<ActionResult> GetPreviewsBatchAsync(ListRequest listRequest)
        {
            var listBuilder = new ListBuilder<Preview>(listRequest, _configProvider.UserSettings);
            var previews = await listBuilder.BuildAsync(_previewRepository);

            return PartialView("/Pages/Gallery/_Gallery.cshtml", new _GalleryPageModel(previews, _configProvider.UserSettings.AllowedNSFWContent));
        }

        public ActionResult GetCollectionPreviewsBatch(Guid id, ListRequest listRequest)
        {
            var previews = _collectionRepository.GetList(id, listRequest);

            return PartialView("/Pages/Gallery/_Collection.cshtml", new _CollectionPageModel(previews, _configProvider.UserSettings.AllowedNSFWContent, listRequest.Offset));
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

            if (preview == null)
            {
                return new FileContentResult(System.IO.File.ReadAllBytes("wwwroot/img/types/noimg.jpg"), "image/jpeg");
            }
            
            if (preview.BlobName == "")
            {
                preview.BlobName = preview.Id.ToString() + '.' + preview.BlobType.Split('/').Last();
            }

            return File(preview.Blob.Content, "application/octet-stream", preview.BlobName);
        }

        public IActionResult DownloadCollection(Guid id)
        {
            var collection = _collectionRepository.Get(id);

            if (collection == null)
            {
                return new FileContentResult(System.IO.File.ReadAllBytes("wwwroot/img/types/noimg.jpg"), "image/jpeg");
            }
            
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

        public object IsCanUploadFiles(List<long> fileSizes)
        {
            var fileSizeLimit = _configProvider.EnvironmentSettings.MaxFileSize;
            var totalFileSize = fileSizes.Sum();

            if (fileSizes.Any(x => x > fileSizeLimit))
            {
                return new
                {
                    Success = false,
                    Message = $"File size above {fileSizeLimit.FormatSize()}"
                };
            }
            
            var userLimit = _userProvider.GetCurrent().SpaceLimitBytes;
            var userFilesSize = _statisticProvider.GetTodaySnapshot().MediasSize;

            if (userLimit != 0 && userFilesSize + totalFileSize > userLimit)
            {
                return new
                {
                    Success = false,
                    Message = $"Space limit exceeded, at least {(userFilesSize + totalFileSize).FormatSize()} free space needed"
                };
            }
            
            return new
            {
                Success = true
            };
        }
    }
}
