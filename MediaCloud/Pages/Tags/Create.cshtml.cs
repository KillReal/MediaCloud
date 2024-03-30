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
using MediaCloud.WebApp.Pages;

namespace MediaCloud.Pages.Tags
{
    public class TagCreateModel : AuthorizedPageModel
    {
        [BindProperty]
        public Tag Tag { get; set; } = new();

        public TagCreateModel(IDataService dataService) : base(dataService)
        {
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
