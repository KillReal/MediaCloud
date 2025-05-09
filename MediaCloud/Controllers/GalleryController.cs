using MediaCloud.Builders.List;
using MediaCloud.Data.Models;
using MediaCloud.Repositories;
using MediaCloud.TaskScheduler;
using MediaCloud.WebApp.Services.UserProvider;
using MediaCloud.WebApp.Services.ConfigProvider;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IO.Compression;
using MediaCloud.Extensions;
using MediaCloud.WebApp.Services.Statistic;
using MediaCloud.WebApp.Services.TaskScheduler.Tasks;
using MediaCloud.WebApp.Repositories;

namespace MediaCloud.WebApp.Controllers
{
    [Authorize]
    public class GalleryController(IConfigProvider configProvider, TagRepository tagRepository,
        PreviewRepository previewRepository, CollectionRepository collectionRepository,
        IUserProvider userProvider,
        StatisticProvider statisticProvider) : Controller
    {
        public List<string> GetTagSuggestions(string searchString, int limit = 10)
        {
            return tagRepository.GetSuggestionsByString(searchString, limit);
        }

        public async Task<List<object>> PreviewsBatchAsync(ListRequest listRequest)
        {
            var ListBuilder = new ListBuilder<Preview>(listRequest, configProvider.UserSettings);
            var previews = await ListBuilder.BuildAsync(previewRepository);

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
            var ListBuilder = new ListBuilder<Preview>(listRequest, configProvider.UserSettings);
            var previews = await ListBuilder.BuildAsync(previewRepository);

            return PartialView("/Pages/Gallery/_Gallery.cshtml", new _GalleryPageModel(previews, configProvider.UserSettings.AllowedNSFWContent));
        }

        public ActionResult GetCollectionPreviewsBatch(Guid id, ListRequest listRequest)
        {
            var previews = collectionRepository.GetList(id, listRequest);

            return PartialView("/Pages/Gallery/_Collection.cshtml", new _CollectionPageModel(previews, configProvider.UserSettings.AllowedNSFWContent, listRequest.Offset));
        }

        public FileContentResult Preview(Guid id)
        {
            var preview = previewRepository.Get(id);

            if (preview != null)
            {
                return new FileContentResult(preview.Content, "image/jpeg");
            }

            return new FileContentResult(System.IO.File.ReadAllBytes("wwwroot/img/types/noimg.jpg"), "image/jpeg");
        }

        public FileContentResult Source(Guid id)
        {
            var preview = previewRepository.Get(id);

            if (preview != null)
            {
                return new FileContentResult(preview.Blob.Content, preview.BlobType);
            }

            return new FileContentResult(System.IO.File.ReadAllBytes("wwwroot/img/types/noimg.jpg"), "image/jpeg");
        }

        public IActionResult Download(Guid id)
        {
            var preview = previewRepository.Get(id);

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
            var collection = collectionRepository.Get(id);

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

        public object IsCanUploadFiles(List<long> fileSizes)
        {
            var fileSizeLimit = configProvider.EnvironmentSettings.MaxFileSize;
            var totalFileSize = fileSizes.Sum();

            if (fileSizes.Any(x => x > fileSizeLimit))
            {
                return new
                {
                    Success = false,
                    Message = $"File size above {fileSizeLimit.FormatSize()}"
                };
            }
            
            var userLimit = userProvider.GetCurrent().SpaceLimitBytes;
            var userFilesSize = statisticProvider.GetTodaySnapshot().MediasSize;

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
