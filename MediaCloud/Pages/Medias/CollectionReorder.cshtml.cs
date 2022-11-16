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

namespace MediaCloud.Pages.Medias
{
    public class CollectionReorderModel : PageModel
    {
        private CollectionRepository CollectionRepository;

        [BindProperty]
        public Collection? Collection { get; set; }
        [BindProperty]
        public string ReturnUrl { get; set; }

        public CollectionReorderModel(AppDbContext context, ILogger<CollectionReorderModel> logger)
        {
            CollectionRepository = new(context, logger);
            ReturnUrl = "/Medias/Index";
        }

        public IActionResult OnGet(Guid id, string returnUrl = "/Medias/Index")
        {
            Collection = CollectionRepository.Get(id);

            if (Collection == null)
            {
                Redirect("/Error");
            }

            ReturnUrl = returnUrl.Replace("$", "&");

            return Page();
        }

        public IActionResult OnPost()
        {
            if (Collection == null)
            {
                return Redirect("/Error");
            }

            var orders = Collection.Previews.Select(x => x.Order)
                                            .ToList();

            if (CollectionRepository.TryUpdateOrder(Collection.Id, orders) == false)
            {
                return Redirect("/Error");
            }

            return Redirect(ReturnUrl.Replace("$", "&"));
        }
    }
}
