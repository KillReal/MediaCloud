using MediaCloud.Data.Models;
using Microsoft.AspNetCore.Mvc;
using MediaCloud.TaskScheduler;
using MediaCloud.TaskScheduler.Tasks;
using MediaCloud.WebApp.Pages;
using MediaCloud.WebApp.Services.UserProvider;
using MediaCloud.WebApp;
using MediaCloud.Extensions;
using MediaCloud.WebApp.Services.Statistic;
using MediaCloud.WebApp.Services.ConfigProvider;

namespace MediaCloud.Pages.Gallery
{
    public class UploadModel(IUserProvider userProvider, ITaskScheduler taskScheduler, IConfigProvider configProvider) 
        : AuthorizedPageModel(userProvider)
    {
        private readonly ITaskScheduler _taskScheduler = taskScheduler;
        private readonly IConfigProvider _configProvider = configProvider;

        [BindProperty]
        public List<IFormFile> Files { get; set; } = [];
        [BindProperty]
        public string? Tags { get; set; }
        [BindProperty]
        public bool IsCollection { get; set; }
        [BindProperty]
        public bool IsNeedAutotagging { get; set; }
        [BindProperty]
        public bool IsAutotaggingEnabled { get; set; }
        [BindProperty]
        public bool IsKeepOriginalFormat { get; set; }

        public IActionResult OnGet()
        {
            IsAutotaggingEnabled = _configProvider.EnvironmentSettings.AutotaggingEnabled 
                && CurrentUser != null 
                && CurrentUser.IsAutotaggingAllowed;

            return Page();
        }

        // Move method to controller
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
                    Content = await file.GetBytesAsync(),
                    KeepOriginalFormat = IsKeepOriginalFormat
                });
            }

            var task = IsNeedAutotagging 
                ? new UploadAndAutotagTask(CurrentUser, uploadedFiles, IsCollection, Tags)
                : new UploadTask(CurrentUser, uploadedFiles, IsCollection, Tags);
            var taskId = _taskScheduler.AddTask(task);

            return Redirect($"/TaskScheduler/GetTaskStatus?id={taskId}");
        }
    }
}
