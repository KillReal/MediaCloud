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
using MediaCloud.WebApp.Services;

namespace MediaCloud.Pages.Tags
{
    [Authorize]
    public class DetailModel : PageModel
    {
        private readonly IRepository _repository;

        [BindProperty]
        public Tag Tag { get; set; }

        [BindProperty]
        public string ReturnUrl { get; set; }

        public DetailModel(IRepository repository)
        {
            _repository = repository;
        }

        public IActionResult OnGet(Guid id, string returnUrl = "/Tags/Index")
        {
            ReturnUrl = returnUrl.Replace("$", "&");  
            Tag = _repository.Tags.Get(id) as Tag ?? new();

            return Page();
        }   

        public IActionResult OnPost()
        {
            _repository.Tags.Update(Tag);

            return Redirect(ReturnUrl.Replace("$", "&"));
        }

        public IActionResult OnPostDelete(Guid id)
        {
            if (_repository.Tags.TryRemove(id) == false)
            {
                return Redirect("/Error");
            }

            _repository.SaveChanges();

            return Redirect(ReturnUrl.Replace("$", "&"));
        }
    }
}
