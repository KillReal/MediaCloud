using MediaCloud.Data.Models;
using Microsoft.AspNetCore.Mvc;
using MediaCloud.TaskScheduler;
using MediaCloud.TaskScheduler.Tasks;
using MediaCloud.WebApp.Pages;
using MediaCloud.WebApp.Services.UserProvider;
using MediaCloud.WebApp;
using MediaCloud.Extensions;

namespace MediaCloud.Pages.Gallery
{
    public class MediaUploadModel(IUserProvider actorProvider, ITaskScheduler taskScheduler) : AuthorizedPageModel(actorProvider)
    {
        private readonly User? _actor = actorProvider.GetCurrent();
        private readonly ITaskScheduler _taskScheduler = taskScheduler;

        [BindProperty]
        public List<IFormFile> Files { get; set; } = [];
        [BindProperty]
        public string? Tags { get; set; }
        [BindProperty]
        public bool IsCollection { get; set; }
        [BindProperty]
        public string ReturnUrl { get; set; } = "/Medias";

        public IActionResult OnGet(string returnUrl = "/Medias")
        {
            ReturnUrl = returnUrl.Replace("$", "&");

            return Page();
        }

        public IActionResult OnPost()
        {
            if (_actor == null)
            {
                return Redirect("/Login");
            }

            var uploadedFiles = new List<UploadedFile>();
            foreach (var file in Files)
            {
                uploadedFiles.Add(new UploadedFile()
                {
                    Name = file.FileName,
                    Type = file.ContentType,
                    Content = file.GetBytes()
                });
            }

            var task = new UploadTask(_actor, uploadedFiles, IsCollection, Tags);
            var taskId = _taskScheduler.AddTask(task);

            return Redirect($"/TaskScheduler/GetTaskStatus?id={taskId}");
        }
    }
}
