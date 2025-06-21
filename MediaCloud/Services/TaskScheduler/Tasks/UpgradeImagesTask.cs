using MediaCloud.Data;
using MediaCloud.Data.Models;
using MediaCloud.WebApp.Services.UserProvider;
using MediaCloud.WebApp.Services.Statistic;
using MediaCloud.Repositories;
using MediaCloud.Builders.List;
using System.Reflection.Metadata;
using MediaCloud.WebApp.Builders.BlobModel;
using MediaCloud.Services;
using MediaCloud.WebApp;
using System.Net.Mime;
using NLog;
using Microsoft.IdentityModel.Tokens;
using MediaCloud.Extensions;
using MediaCloud.WebApp.Services.ConfigProvider;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace MediaCloud.TaskScheduler.Tasks
{
    public class UpgradeUserImagesTask(User user, int batchSize) : Task(user), ITask
    {
        private readonly Logger _logger = LogManager.GetLogger("Scheduler");
        private int _workCount;

        public override int GetWorkCount() => _workCount;

        public override async void DoTheTask(IServiceProvider serviceProvider, IUserProvider userProvider, StatisticProvider statisticProvider)
        {
            var context = serviceProvider.GetRequiredService<AppDbContext>();
            var pictureService = serviceProvider.GetRequiredService<IPictureService>();
            var configProvider = serviceProvider.GetRequiredService<IConfigProvider>();

            var previewsCount = await context.Previews.Where(x => x.CreatorId == User.Id).CountAsync();
            var blobModelBuilder = new FileModelBuilder(pictureService, configProvider);

            _workCount = previewsCount;

            var offset = 0;
            var totalShrinkSize = (long)0;

            _logger.Info("Start preview processing for {0} with {1} previews", User.Name, _workCount);

            while (_workCount > 0)
            {
                var previews = await context.Previews.Where(x => x.CreatorId == User.Id)
                    .OrderByDescending(x => x.CreatedAt)
                    .Skip(offset)
                    .Take(batchSize)
                    .ToListAsync();

                var shrinkedSize = (long)0;

                _logger.Info("Start batch processing up to date {0}", previews.Last().CreatedAt);


                var proceededCount = 0;
                foreach (var preview in previews)
                {
                    if (preview.BlobType != "image/jpeg")
                    {
                        continue;
                    }

                    var fileToConvert = new UploadedFile()
                    {
                        Name = preview.BlobName,
                        Type = preview.BlobType,
                        Content = preview.Blob.Content
                    };

                    if (fileToConvert.Name.IsNullOrEmpty())
                    {
                        fileToConvert.Name = preview.Id.ToString() + ".jpeg";
                    }

                    var blob = blobModelBuilder.Build(fileToConvert);

                    var sizeBefore = preview.Blob.Size;
                    shrinkedSize += sizeBefore - blob.File.Content.Length;

                    preview.Blob.Content = blob.File.Content;
                    preview.Blob.Size = blob.File.Content.Length;
                    preview.BlobName = blob.Preview.BlobName;
                    preview.BlobType = "image/webp";
                    preview.Content = blob.Preview.Content;

                    proceededCount += 1;
                }
                
                statisticProvider.MediasCountChanged(0, -shrinkedSize);
                _workCount -= previews.Count;
                offset += batchSize;
                totalShrinkSize += shrinkedSize;

                context.Previews.UpdateRange(previews);
                await context.SaveChangesAsync();

                _logger.Info("Previews batch processed {0} converted and {1} left {2} size compressed", proceededCount, _workCount, shrinkedSize.FormatSize());
            }

            _logger.Info("Previews processing completed {0} compressed", totalShrinkSize.FormatSize());
        }
    }
}
