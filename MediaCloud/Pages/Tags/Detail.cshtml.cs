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
using MediaCloud.WebApp.Services.Repository;

namespace MediaCloud.Pages.Tags
{
    [Authorize]
    public class DetailModel : PageModel
    {
        private readonly IRepository Repository;

        [BindProperty]
        public Tag Tag { get; set; }

        [BindProperty]
        public string ReturnUrl { get; set; }

        public DetailModel(IRepository repository)
        {
            Repository = repository;
        }

        public IActionResult OnGet(Guid id, string returnUrl = "/Tags/Index")
        {
            ReturnUrl = returnUrl.Replace("$", "&");  
            Tag = Repository.Tags.Get(id) ?? new();

            return Page();
        }   

        public IActionResult OnPost()
        {
            Repository.Tags.Update(Tag);

            return Redirect(ReturnUrl.Replace("$", "&"));
        }

        public IActionResult OnPostDelete(Guid id)
        {
            if (Repository.Tags.TryRemove(id) == false)
            {
                return Redirect("/Error");
            }

            Repository.SaveChanges();

            return Redirect(ReturnUrl.Replace("$", "&"));
        }
    }
}
