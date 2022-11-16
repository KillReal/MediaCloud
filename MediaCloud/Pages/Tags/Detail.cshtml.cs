using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using MediaCloud.Data;
using MediaCloud.Services;
using MediaCloud.Data.Models;
using MediaCloud.Repositories;

namespace MediaCloud.Pages.Tags
{
    public class DetailModel : PageModel
    {
        private readonly TagRepository TagRepository;

        public DetailModel(AppDbContext context, ILogger<TagRepository> logger)
        {
            TagRepository = new(context, logger);
        }

        public IActionResult OnGet(Guid id, string returnUrl = "/Tags/Index")
        {
            ReturnUrl = returnUrl.Replace("$", "&");  
            Tag = TagRepository.Get(id) ?? new();

            return Page();
        }   

        [BindProperty]
        public Tag Tag { get; set; }

        [BindProperty]
        public string ReturnUrl { get; set; }

        public IActionResult OnPost()
        {
            TagRepository.Update(Tag);

            return Redirect(ReturnUrl.Replace("$", "&"));
        }

        public IActionResult OnPostDelete(Guid id)
        {
            if (TagRepository.TryRemove(id) == false)
            {
                return Redirect("/Error");
            }

            TagRepository.SaveChanges();

            return Redirect(ReturnUrl.Replace("$", "&"));
        }
    }
}
