﻿using System;
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
using MediaCloud.WebApp.Services.DataService;
using MediaCloud.WebApp.Services.Statistic;
using MediaCloud.WebApp.Pages;

namespace MediaCloud.Pages.Medias
{
    [Authorize]
    public class MediaUploadModel : BasePageModel
    {
        private readonly Actor? _actor;
        private readonly IUploader _uploader;
        private readonly IStatisticService _statisticService;

        [BindProperty]
        public List<IFormFile> Files { get; set; } = new();
        [BindProperty]
        public string? Tags { get; set; }
        [BindProperty]
        public bool IsCollection { get; set; }
        [BindProperty]
        public string ReturnUrl { get; set; } = "/Medias";

        public MediaUploadModel(IDataService dataService, IUploader uploader, IStatisticService statisticService) : base(dataService)
        {
            _actor = dataService.GetCurrentActor();
            _uploader = uploader;
            _statisticService = statisticService;
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
            var taskId = _uploader.AddTask(task);

            _statisticService.ActivityFactorRaised.Invoke();

            return Redirect($"/Uploader/GetTaskStatus?id={taskId}");
        }
    }
}
