﻿using MediaCloud.Data;
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
using Microsoft.EntityFrameworkCore;

namespace MediaCloud.TaskScheduler.Tasks
{
    public class UpgradeUserImagesTask(User actor, int batchSize) : Task(actor), ITask
    {
        private readonly Logger _logger = LogManager.GetLogger("Scheduler");
        private readonly int _batchSize = batchSize;
        private int _workCount;

        public override int GetWorkCount() => _workCount;

        public override async void DoTheTask(IServiceProvider serviceProvider, IUserProvider userProvider)
        {
            var context = serviceProvider.GetRequiredService<AppDbContext>();
            var pictureService = serviceProvider.GetRequiredService<IPictureService>();

            var statisticProvider = new StatisticProvider(context, userProvider);

            var previewsCount = await context.Previews.Where(x => x.CreatorId == Actor.Id).CountAsync();
            var blobModelBuilder = new BlobModelBuilder(pictureService);

            _workCount = previewsCount;

            var offset = 0;
            var totalShrinkSize = (long)0;

            _logger.Info("Start preview processing for {0} with {1} previews", actor.Name, _workCount);

            while (_workCount > 0)
            {
                var previews = await context.Previews.Where(x => x.CreatorId == Actor.Id)
                    .OrderByDescending(x => x.CreatedAt)
                    .Skip(offset)
                    .Take(_batchSize)
                    .ToListAsync();

                var shrinkedSize = (long)0;

                _logger.Info("Start batch processing up to date {0}", previews.Last().CreatedAt);


                var proceededCount = 0;
                foreach (var preview in previews)
                {
                    if (preview.BlobType == "image/webp")
                    {
                        continue;
                    }

                    if (preview.BlobType.Contains("image") == false || preview.BlobType == "image/gif")
                    {
                        continue;
                    }

                    if (preview == null)
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

                    var convertedFile = blobModelBuilder.Build(fileToConvert);

                    var sizeBefore = preview.Blob.Size;
                    shrinkedSize += sizeBefore - convertedFile.File.Content.Length;

                    preview.Blob.Content = convertedFile.File.Content;
                    preview.Blob.Size = convertedFile.File.Content.Length;
                    preview.BlobName = convertedFile.Preview.BlobName;
                    preview.BlobType = "image/webp";
                    preview.Content = convertedFile.Preview.Content;

                    proceededCount += 1;
                }
                
                statisticProvider.MediasCountChanged(0, -shrinkedSize);
                _workCount -= previews.Count;
                offset += _batchSize;
                totalShrinkSize += shrinkedSize;

                context.Previews.UpdateRange(previews);
                await context.SaveChangesAsync();

                _logger.Info("Previews batch processed {0} converted and {1} left {2} size compressed", proceededCount, _workCount, shrinkedSize.FormatSize());
            }

            _logger.Info("Previews processing completed {0} compressed", totalShrinkSize.FormatSize());
        }
    }
}
