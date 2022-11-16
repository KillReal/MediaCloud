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

namespace MediaCloud.Pages.Tags
{
    public class ListModel : PageModel
    {
        private TagRepository TagRepository;

        public ListModel(AppDbContext context, ILogger<ListModel> logger)
        {
            TagRepository = new(context, logger);
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
