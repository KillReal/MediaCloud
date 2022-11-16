﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using MediaCloud.Data;
using MediaCloud.Builders.List;
using MediaCloud.Data.Models;
using MediaCloud.Repositories;

namespace MediaCloud.Pages.Medias
{
    public class ListModel : PageModel
    {
        private PreviewRepository PreviewRepository;

        [BindProperty]
        public List<Preview> Previews { get; set; }

        [BindProperty]
        public ListBuilder<Preview> ListBuilder { get; set; }

        public ListModel(AppDbContext context, ILogger<ListModel> logger)
        {
            PreviewRepository = new(context, logger);
        }

        public IActionResult OnGet(ListRequest request)
        {
            ListBuilder = new ListBuilder<Preview>(request);
            Previews = ListBuilder.Build(PreviewRepository);

            return Page();
        }  
    }
}
