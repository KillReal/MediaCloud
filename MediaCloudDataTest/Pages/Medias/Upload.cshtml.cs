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
            List<Media> medias;

            if (IsCollection)
            {
                medias = new MediaRepository(_context).CreateCollection(Files.OrderBy(x => x.FileName)
                                                                                .ToList());
            }
            else
            {
                medias = new MediaRepository(_context).CreateRange(Files);
            }

            if (!medias.Any())
            {
                Redirect("/Error");
            }

            var foundTags = new TagRepository(_context).GetRangeByString(Tags);
            
            foreach (var media in medias)
            {
                media.Preview.Tags = foundTags;
            }
                
            _context.Medias.UpdateRange(medias);
            _context.SaveChanges();

            return Redirect(ReturnUrl.Replace("$", "&"));
        }
    }
}
