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
using MediaCloud.WebApp.Pages;
using Microsoft.AspNetCore.Mvc.Abstractions;
using MediaCloud.WebApp.Services.ActorProvider;

namespace MediaCloud.Pages.Tags
{
    public class TagDetailModel : AuthorizedPageModel
    {
        private readonly TagRepository _tagRepository;

        [BindProperty]
        public Tag Tag { get; set; } = new();

        [BindProperty]
        public string ReturnUrl { get; set; } = "/Tags";

        public TagDetailModel(IActorProvider actorProvider, TagRepository tagRepository) : base(actorProvider)
        {
            _tagRepository = tagRepository;
        }

        public IActionResult OnGet(Guid id, string returnUrl = "/Tags/Index")
        {
            ReturnUrl = returnUrl.Replace("$", "&");  
            Tag = _tagRepository.Get(id) ?? new();

            return Page();
        }   

        public IActionResult OnPost()
        {
            if (Tag == null)
            {
                return Redirect("/Error");
            }

            _tagRepository.Update(Tag);

            return Redirect(ReturnUrl.Replace("$", "&"));
        }

        public IActionResult OnPostDelete(Guid id)
        {
            if (_tagRepository.TryRemove(id) == false)
            {
                return Redirect("/Error");
            }

            return Redirect(ReturnUrl.Replace("$", "&"));
        }
    }
}
