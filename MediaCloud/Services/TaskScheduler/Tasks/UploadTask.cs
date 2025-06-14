﻿using MediaCloud.Data;
using MediaCloud.Data.Models;
using MediaCloud.Extensions;
using MediaCloud.Repositories;
using MediaCloud.Services;
using MediaCloud.WebApp;
using MediaCloud.WebApp.Services.UserProvider;
using MediaCloud.WebApp.Services.Statistic;
using System.Text;
using Blob = MediaCloud.Data.Models.Blob;
using MediaCloud.WebApp.Services.ConfigProvider;
using MediaCloud.WebApp.Services.TaskScheduler.Tasks;
using MediaCloud.WebApp.Controllers;
using Microsoft.Extensions.Caching.Memory;

namespace MediaCloud.TaskScheduler.Tasks
{
    internal class FileNameComparer : IComparer<string>
    {

        public int Compare(string? x, string? y)
        {
            if (x == null && y == null) return 0;
            if (x == null) return -1;
            if (y == null) return 1;

            int lx = x.Length, ly = y.Length;

            for (int mx = 0, my = 0; mx < lx && my < ly; mx++, my++)
            {
                if (char.IsDigit(x[mx]) && char.IsDigit(y[my]))
                {
                    long vx = 0, vy = 0;

                    for (; mx < lx && char.IsDigit(x[mx]); mx++)
                        vx = vx * 10 + x[mx] - '0';

                    for (; my < ly && char.IsDigit(y[my]); my++)
                        vy = vy * 10 + y[my] - '0';

                    if (vx != vy)
                        return vx > vy ? 1 : -1;
                }

                if (mx < lx && my < ly && x[mx] != y[my])
                    return x[mx] > y[my] ? 1 : -1;
            }

            return lx - ly;
        }
    }

    public class UploadTask(User user, List<UploadedFile> uploadedFiles, bool isCollection, string? tagString) 
        : Task(user), ITask
    {
        private protected List<Preview> _processedPreviews = [];

        private List<UploadedFile> UploadedFiles { get; set; } = [.. uploadedFiles.OrderByDescending(x => x.Name, new FileNameComparer())];

        private bool IsCollection { get; set; } = isCollection;

        private string TagString { get; set; } = tagString ?? "";

        public override int GetWorkCount() => UploadedFiles.Count(x => x.IsProcessed == false);

        public override void DoTheTask(IServiceProvider serviceProvider, IUserProvider userProvider, StatisticProvider statisticProvider)
        {
            var context = serviceProvider.GetRequiredService<AppDbContext>();
            var pictureService = serviceProvider.GetRequiredService<IPictureService>();
            var configProvider = serviceProvider.GetRequiredService<IConfigProvider>();

            var sizeToUpload = UploadedFiles.Select(x => x.Content.LongLength).Sum();
            var targetSize = statisticProvider.GetTodaySnapshot().MediasSize + sizeToUpload;

            if (User.SpaceLimitBytes != 0 && targetSize > User.SpaceLimitBytes)
            {
                throw new Exception($"Uploading failed due to low space limit (need at least {sizeToUpload.FormatSize()} free space)");
            }

            var tagRepository = new TagRepository(context, statisticProvider, userProvider);
            var blobRepository = new BlobRepository(context, statisticProvider, userProvider, pictureService, configProvider);

            var foundTags = tagRepository.GetRangeByString(TagString);
            
            var files = new List<Blob>();

            if (IsCollection)
            {
                files = blobRepository.CreateCollection(UploadedFiles);
            }
            else
            {
                files = blobRepository.CreateRange(UploadedFiles);
            }

            _processedPreviews = files.Select(x => x.Preview).ToList();
            
            
            if (foundTags.Count > 0)
            {
                foreach (var preview in _processedPreviews)
                {
                    tagRepository.UpdatePreviewLinks(foundTags, preview);
                }
            }
            
            UploadedFiles.Clear();

            CompletionMessage = $"Proceeded {files.Count} files, total size: {files.Select(x => x.Size).Sum().FormatSize()}";
        }
    }
}
