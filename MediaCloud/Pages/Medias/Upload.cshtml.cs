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

namespace MediaCloud.Pages.Medias
{
    public class UploadModel : PageModel
    {
        private AppDbContext _context;  

        public UploadModel(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult OnGet(string returnUrl = "/Medias/Index")
        {
            ReturnUrl = returnUrl.Replace("$", "&");

            return Page();
        }  

        [BindProperty]
        public List<IFormFile> Files { get; set; }
        [BindProperty]
        public string? Tags { get; set; }
        [BindProperty]
        public bool IsCollection { get; set; }
        [BindProperty]
        public string ReturnUrl { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            var taskId = Uploader.AddTask(new(Files, IsCollection, Tags));

            return Redirect($"/Uploader/GetTaskStatus?id={taskId}");
        }
    }
}
