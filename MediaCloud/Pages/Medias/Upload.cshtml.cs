using MediaCloud.Data.Models;
using Microsoft.AspNetCore.Mvc;
using MediaCloud.TaskScheduler;
using MediaCloud.TaskScheduler.Tasks;
using MediaCloud.WebApp.Pages;
using MediaCloud.WebApp.Services.ActorProvider;

namespace MediaCloud.Pages.Medias
{
    public class MediaUploadModel(IActorProvider actorProvider, ITaskScheduler taskScheduler) : AuthorizedPageModel(actorProvider)
    {
        private readonly Actor? _actor = actorProvider.GetCurrent();
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

            var task = new UploadTask(_actor, Files, IsCollection, Tags);
            var taskId = _taskScheduler.AddTask(task);

            return Redirect($"/TaskScheduler/GetTaskStatus?id={taskId}");
        }
    }
}
