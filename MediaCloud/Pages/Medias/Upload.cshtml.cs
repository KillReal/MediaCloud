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
using MediaCloud.WebApp.Services.Repository;
using MediaCloud.WebApp.Services.Statistic;

namespace MediaCloud.Pages.Medias
{
    [Authorize]
    public class UploadModel : PageModel
    {
        private Actor? Actor;
        private IUploader Uploader;
        private IStatisticService StatisticService;

        [BindProperty]
        public List<IFormFile> Files { get; set; }
        [BindProperty]
        public string? Tags { get; set; }
        [BindProperty]
        public bool IsCollection { get; set; }
        [BindProperty]
        public string ReturnUrl { get; set; }

        public UploadModel(IRepository repository, IUploader uploader, IStatisticService statisticService)
        {
            Actor = repository.GetCurrentActor();
            Uploader = uploader;
            StatisticService = statisticService;
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

            var task = new UploadTask(Actor, Files, IsCollection, Tags);
            var taskId = Uploader.AddTask(task);

            StatisticService.ActivityFactorRaised.Invoke();

            return Redirect($"/Uploader/GetTaskStatus?id={taskId}");
        }
    }
}
