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
    public class CreateModel : PageModel
    {
        private TagRepository TagRepository;

        public CreateModel(AppDbContext context)
        {
            TagRepository = new(context);
        }

        public IActionResult OnGet()
        {
            return Page();
        }

        [BindProperty]
        public Tag Tag { get; set; } = new();

        public async Task<IActionResult> OnPostAsync()
        {       
            TagRepository.Create(Tag);
            TagRepository.SaveChanges();

            return RedirectToPage("/Tags/Index");
        }
    }
}
