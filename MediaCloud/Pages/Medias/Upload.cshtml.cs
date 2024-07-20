using MediaCloud.Data.Models;
using Microsoft.AspNetCore.Mvc;
using MediaCloud.TaskScheduler;
using MediaCloud.TaskScheduler.Tasks;
using MediaCloud.WebApp.Pages;
using MediaCloud.WebApp.Services.ActorProvider;

namespace MediaCloud.Pages.Medias
{
    public class MediaUploadModel : AuthorizedPageModel
    {
        private readonly Actor? _actor;
        private readonly ITaskScheduler _taskScheduler;

        [BindProperty]
        public List<IFormFile> Files { get; set; } = new();
        [BindProperty]
        public string? Tags { get; set; }
        [BindProperty]
        public bool IsCollection { get; set; }
        [BindProperty]
        public string ReturnUrl { get; set; } = "/Medias";

        public MediaUploadModel(IActorProvider actorProvider, ITaskScheduler taskScheduler) : base(actorProvider)
        {
            _actor = actorProvider.GetCurrent();
            _taskScheduler = taskScheduler;
        }

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
