﻿using System;
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
    public class DetailModel : PageModel
    {
        private readonly TagRepository TagRepository;

        public DetailModel(AppDbContext context, ILogger<TagRepository> logger,
            IActorProvider actorProvider)
        {
            var actor = actorProvider.GetCurrent() ?? new();

            TagRepository = new(context, logger, actor.Id);
        }

        public IActionResult OnGet(Guid id, string returnUrl = "/Tags/Index")
        {
            ReturnUrl = returnUrl.Replace("$", "&");  
            Tag = TagRepository.Get(id) as Tag ?? new();

            return Page();
        }   

        [BindProperty]
        public Tag Tag { get; set; }

        [BindProperty]
        public string ReturnUrl { get; set; }

        public IActionResult OnPost()
        {
            TagRepository.Update(Tag);

            return Redirect(ReturnUrl.Replace("$", "&"));
        }

        public IActionResult OnPostDelete(Guid id)
        {
            if (TagRepository.TryRemove(id) == false)
            {
                return Redirect("/Error");
            }

            TagRepository.SaveChanges();

            return Redirect(ReturnUrl.Replace("$", "&"));
        }
    }
}
