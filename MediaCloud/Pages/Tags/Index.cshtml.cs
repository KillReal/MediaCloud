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
using MediaCloud.WebApp.Services;

namespace MediaCloud.Pages.Tags
{
    [Authorize]
    public class ListModel : PageModel
    {
        private TagRepository TagRepository;

        public ListModel(AppDbContext context, ILogger<ListModel> logger, 
            IActorProvider actorProvider)
        {
            var actor = actorProvider.GetCurrent() ?? new();

            TagRepository = new(context, logger, actor.Id);
        }

        [BindProperty]
        public List<Tag> Tags { get; set; }
        [BindProperty]
        public ListBuilder<Tag> ListBuilder { get; set; }

        public IActionResult OnGet(ListRequest request)
        {
            ListBuilder = new(request);
            Tags = ListBuilder.Build(TagRepository);

            return Page();
        }
    }
}
