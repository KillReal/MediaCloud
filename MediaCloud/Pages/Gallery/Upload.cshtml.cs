using MediaCloud.Data.Models;
using Microsoft.AspNetCore.Mvc;
using MediaCloud.TaskScheduler;
using MediaCloud.TaskScheduler.Tasks;
using MediaCloud.WebApp.Pages;
using MediaCloud.WebApp.Services.UserProvider;
using MediaCloud.WebApp;
using MediaCloud.Extensions;
using MediaCloud.WebApp.Services.Statistic;

namespace MediaCloud.Pages.Gallery
{
    public class UploadModel(IUserProvider userProvider, ITaskScheduler taskScheduler, StatisticProvider statisticProvider) : AuthorizedPageModel(userProvider)
    {
        private readonly ITaskScheduler _taskScheduler = taskScheduler;
        private readonly StatisticProvider _statisticProvider = statisticProvider;

        [BindProperty]
        public List<IFormFile> Files { get; set; } = [];
        [BindProperty]
        public string? Tags { get; set; }
        [BindProperty]
        public bool IsCollection { get; set; }
        [BindProperty]
        public string ReturnUrl { get; set; } = "/Gallery";

        public IActionResult OnGet(string returnUrl = "/Gallery")
        {
            ReturnUrl = returnUrl.Replace("$", "&");

            return Page();
        }

        public async Task<IActionResult> OnPost()
        {
            if (CurrentUser == null)
            {
                return Redirect("/Login");
            }

            var uploadedFiles = new List<UploadedFile>();
            foreach (var file in Files)
            {
                uploadedFiles.Add(new UploadedFile()
                {
                    Name = Path.GetFileName(file.FileName),
                    Type = file.ContentType,
                    Content = await file.GetBytesAsync()
                });
            }

            var sizeToUpload = uploadedFiles.Select(x => x.Content.Length).Sum();
            var targetSize = _statisticProvider.GetTodaySnapshot().MediasSize + sizeToUpload;

            if (targetSize > CurrentUser.SpaceLimitBytes)
            {
                // TODO: implement UploadPostResult
                return Redirect("/Error");
            }

            var task = new UploadTask(CurrentUser, uploadedFiles, IsCollection, Tags);
            var taskId = _taskScheduler.AddTask(task);

            // TODO: implement UploadPostResult
            return Redirect($"/TaskScheduler/GetTaskStatus?id={taskId}");
        }
    }
}
