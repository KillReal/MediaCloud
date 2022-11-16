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

namespace MediaCloud.Pages.Medias
{
    public class UploadModel : PageModel
    {
        private IUploader Uploader;

        [BindProperty]
        public List<IFormFile> Files { get; set; }
        [BindProperty]
        public string? Tags { get; set; }
        [BindProperty]
        public bool IsCollection { get; set; }
        [BindProperty]
        public string ReturnUrl { get; set; }

        public UploadModel(IUploader uploader)
        {
            Uploader = uploader;
        }

        public IActionResult OnGet(string returnUrl = "/Medias/Index")
        {
            ReturnUrl = returnUrl.Replace("$", "&");

            return Page();
        }

        public IActionResult OnPost()
        {
            var task = new UploadTask(Files, IsCollection, Tags);
            var taskId = Uploader.AddTask(task);

            return Redirect($"/Uploader/GetTaskStatus?id={taskId}");
        }
    }
}
