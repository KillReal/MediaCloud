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
using MediaCloud.WebApp.Services.DataService;

namespace MediaCloud.Pages.Tags
{
    [Authorize]
    public class CreateModel : PageModel
    {
        private readonly IDataService _dataService;

        [BindProperty]
        public Tag Tag { get; set; } = new();

        public CreateModel(IDataService dataService)
        {
            _dataService = dataService;
        }

        public IActionResult OnGet()
        {
            return Page();
        }

        public IActionResult OnPost()
        {
            _dataService.Tags.Create(Tag);

            return RedirectToPage("/Tags");
        }
    }
}
