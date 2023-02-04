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
using MediaCloud.WebApp.Services.Repository;

namespace MediaCloud.Pages.Tags
{
    [Authorize]
    public class ListModel : PageModel
    {
        private IRepository Repository;

        public ListModel(IRepository repository)
        {
            Repository = repository;
        }

        [BindProperty]
        public List<Tag> Tags { get; set; }
        [BindProperty]
        public ListBuilder<Tag> ListBuilder { get; set; }

        public async Task<IActionResult> OnGetAsync(ListRequest request)
        {
            ListBuilder = new(request);
            Tags = await ListBuilder.BuildAsync(Repository.Tags);

            return Page();
        }
    }
}
