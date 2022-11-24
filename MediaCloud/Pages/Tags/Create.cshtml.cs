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
        private IRepository _repository;

        [BindProperty]
        public Tag Tag { get; set; } = new();

        public CreateModel(IRepository repository)
        {
            _repository = repository;
        }

        public IActionResult OnGet()
        {
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            _repository.Tags.Create(Tag);

            return RedirectToPage("/Tags/Index");
        }
    }
}
