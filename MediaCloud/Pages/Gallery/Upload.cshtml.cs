﻿using MediaCloud.Data.Models;
using Microsoft.AspNetCore.Mvc;
using MediaCloud.TaskScheduler;
using MediaCloud.TaskScheduler.Tasks;
using MediaCloud.WebApp.Pages;
using MediaCloud.WebApp.Services.UserProvider;
using MediaCloud.WebApp;
using MediaCloud.Extensions;

namespace MediaCloud.Pages.Gallery
{
    public class UploadModel(IUserProvider userProvider, ITaskScheduler taskScheduler) : AuthorizedPageModel(userProvider)
    {
        private readonly User? _user = userProvider.GetCurrent();
        private readonly ITaskScheduler _taskScheduler = taskScheduler;

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
            if (_user == null)
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

            var task = new UploadTask(_user, uploadedFiles, IsCollection, Tags);
            var taskId = _taskScheduler.AddTask(task);

            return Redirect($"/TaskScheduler/GetTaskStatus?id={taskId}");
        }
    }
}
