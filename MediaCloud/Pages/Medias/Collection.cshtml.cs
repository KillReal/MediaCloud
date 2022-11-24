using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using MediaCloud.Data;
using MediaCloud.Builders.List;
using MediaCloud.Data.Models;
using MediaCloud.Services;
using System.Drawing;
using System.Drawing.Imaging;
using MediaCloud.Repositories;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using MediaCloud.WebApp.Services;

namespace MediaCloud.Pages.Medias
{
    [Authorize]
    public class CollectionModel : PageModel
    {
        private IRepository _repository;

        [BindProperty]
        public Collection? Collection { get; set; }
        [BindProperty]
        public string ReturnUrl { get; set; }

        public CollectionModel(IRepository repository)
        {
            _repository = repository;
        }

        public IActionResult OnGet(Guid id, string returnUrl = "/Medias/Index")
        {
            Collection = _repository.Collections.Get(id);

            if (Collection == null)
            {
                return Redirect("/Error");
            }

            ReturnUrl = returnUrl.Replace("$", "&");

            return Page();
        }

        public IActionResult OnPostDelete(Guid id)
        {
            if (_repository.Collections.TryRemove(id) == false)
            {
                return Redirect("/Error");
            }

            _repository.SaveChanges();

            return Redirect(ReturnUrl.Replace("$", "&"));
        }
    }
}
