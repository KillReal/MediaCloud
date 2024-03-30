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
    public class TagDetailModel : AuthorizedPageModel
    {
        [BindProperty]
        public Tag Tag { get; set; }

        [BindProperty]
        public string ReturnUrl { get; set; } = "/Tags";

        public TagDetailModel(IDataService dataService) : base(dataService)
        {
            Tag = new();
        }

        public IActionResult OnGet(Guid id, string returnUrl = "/Tags/Index")
        {
            ReturnUrl = returnUrl.Replace("$", "&");  
            Tag = _dataService.Tags.Get(id) ?? new();

            return Page();
        }   

        public IActionResult OnPost()
        {
            if (Tag == null)
            {
                return Redirect("/Error");
            }

            _dataService.Tags.Update(Tag);

            return Redirect(ReturnUrl.Replace("$", "&"));
        }

        public IActionResult OnPostDelete(Guid id)
        {
            if (_dataService.Tags.TryRemove(id) == false)
            {
                return Redirect("/Error");
            }

            return Redirect(ReturnUrl.Replace("$", "&"));
        }
    }
}
