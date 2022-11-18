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
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using MediaCloud.WebApp.Services;

namespace MediaCloud.Pages.Tags
{
    [Authorize]
    public class CreateModel : PageModel
    {
        private TagRepository TagRepository;

        [BindProperty]
        public Tag Tag { get; set; } = new();

        public CreateModel(AppDbContext context, ILogger<CreateModel> logger, 
            IActorProvider actorProvider)
        {
            var actor = actorProvider.GetCurrent() ?? new();

            TagRepository = new(context, logger, actor.Id);
        }

        public IActionResult OnGet()
        {
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {       
            TagRepository.Create(Tag);

            return RedirectToPage("/Tags/Index");
        }
    }
}
