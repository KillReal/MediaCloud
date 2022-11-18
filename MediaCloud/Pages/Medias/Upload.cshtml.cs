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

namespace MediaCloud.Pages.Medias
{
    [Authorize]
    public class UploadModel : PageModel
    {
        private Actor? Actor;
        private IUploader Uploader;

        [BindProperty]
        public List<IFormFile> Files { get; set; }
        [BindProperty]
        public string? Tags { get; set; }
        [BindProperty]
        public bool IsCollection { get; set; }
        [BindProperty]
        public string ReturnUrl { get; set; }

        public UploadModel(AppDbContext context, IUploader uploader, IActorProvider actorProvider)
        {
            Actor = actorProvider.GetCurrent();
            Uploader = uploader;
        }

        public IActionResult OnGet(string returnUrl = "/Medias/Index")
        {
            ReturnUrl = returnUrl.Replace("$", "&");

            return Page();
        }

        public IActionResult OnPost()
        {
            if (Actor == null)
            {
                return Redirect("/Login");
            }

            var task = new UploadTask(Files, Actor.Id, IsCollection, Tags);
            var taskId = Uploader.AddTask(task);

            return Redirect($"/Uploader/GetTaskStatus?id={taskId}");
        }
    }
}
