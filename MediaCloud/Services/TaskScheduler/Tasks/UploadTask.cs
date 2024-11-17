using MediaCloud.Data;
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

    public class UploadTask(User actor, List<UploadedFile> uploadedFiles, bool isCollection, string? tagString) 
        : Task(actor), ITask
    {
        public List<UploadedFile> UploadedFiles { get; set; } = [.. uploadedFiles.OrderByDescending(x => x.Name, new FileNameComparer())];

        public bool IsCollection { get; set; } = isCollection;

        public string TagString { get; set; } = tagString ?? "";

        public override int GetWorkCount() => UploadedFiles.Count(x => x.IsProcessed == false);

        public override void DoTheTask(IServiceProvider serviceProvider, IUserProvider userProvider)
        {
            var context = serviceProvider.GetRequiredService<AppDbContext>();
            var statisticProvider = new StatisticProvider(context, userProvider);
            var pictureService = serviceProvider.GetRequiredService<IPictureService>();
            var configProvider = serviceProvider.GetRequiredService<IConfigProvider>();

            var sizeToUpload = UploadedFiles.Select(x => x.Content.Length).Sum();
            var targetSize = statisticProvider.GetTodaySnapshot().MediasSize + sizeToUpload;

            if (targetSize > User.SpaceLimitBytes)
            {
                var delta = (targetSize - User.SpaceLimitBytes).FormatSize();
                throw new Exception($"Uploading failed due to low space limit (need at least {delta} free space)");
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

            var preview = files.Select(x => x.Preview).Where(x => x.Order == 0).First();
            
            
            if (foundTags.Count > 0)
            {
                tagRepository.UpdatePreviewLinks(foundTags, preview);
            }

            CompletionMessage = $"Proceeded {files.Count} files";
        }
    }
}
