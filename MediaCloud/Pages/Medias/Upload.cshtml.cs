using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediaCloud.Data;
using MediaCloud.Data.Models;
using MediaCloud.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using MediaCloud.MediaUploader;
using MediaCloud.MediaUploader.Tasks;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using MediaCloud.Repositories;
using MediaCloud.WebApp.Services;
using MediaCloud.WebApp.Services.Statistic;
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
