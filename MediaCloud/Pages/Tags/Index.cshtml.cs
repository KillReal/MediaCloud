using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MediaCloud.Data;
using MediaCloud.Data.Models;
using MediaCloud.Builders.List;
using MediaCloud.Services;
using MediaCloud.Repositories;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using MediaCloud.WebApp.Services.DataService;

namespace MediaCloud.Pages.Tags
{
    [Authorize]
    public class ListModel : PageModel
    {
        private readonly IDataService _dataService;

        public ListModel(IDataService dataService)
        {
            _dataService = dataService;
            ListBuilder = new(new());
        }

        [BindProperty]
        public List<Tag> Tags { get; set; } = new();
        [BindProperty]
        public ListBuilder<Tag> ListBuilder { get; set; }
        [BindProperty]
        public bool IsAutoloadEnabled { get; set; } = false;

        public async Task<IActionResult> OnGetAsync(ListRequest request)
        {
            ListBuilder = new(request);
            Tags = await ListBuilder.BuildAsync(_dataService.Tags);

            return Page();
        }
    }
}
