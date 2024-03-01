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
using MediaCloud.WebApp.Services.Repository;

namespace MediaCloud.Pages.Medias
{
    [Authorize]
    public class CollectionModel : PageModel
    {
        private IRepository Repository;

        [BindProperty]
        public Collection? Collection { get; set; }
        [BindProperty]
        public string ReturnUrl { get; set; }
        [BindProperty]
        public List<Tag> Tags { get; set; }
        [BindProperty]
        public string TagsString { get; set; }
        [BindProperty]
        public bool IsOrderChanged { get; set; } = false;
        [BindProperty]
        public ListRequest ListRequest { get; set; } = new() { Count = 42};
        [BindProperty]
        public int TotalCount { get; set; }
        [BindProperty]
        public List<int> Orders { get; set; }
        [BindProperty]
        public string CollectionSizeInfo { get; set; }

        public CollectionModel(IRepository repository)
        {
            Repository = repository;
        }

        public IActionResult OnGet(Guid id, string returnUrl = "/Medias/Index")
        {
            Collection = Repository.Collections.Get(id);

            if (Collection == null)
            {
                return Redirect("/Error");
            }

            var preview = Collection.Previews.OrderBy(x => x.Order).First();
            var collectionSize = Repository.Collections.GetSize(id);
            CollectionSizeInfo = PictureService.FormatSize(collectionSize);
            TotalCount = Repository.Collections.GetListCount(id).Result;

            Tags = preview.Tags.OrderBy(x => x.Type).ToList();
            TagsString = string.Join(" ", Tags.Select(x => x.Name.ToLower()));
            ReturnUrl = returnUrl.Replace("$", "&");

            return Page();
        }

        public IActionResult OnPost()
        {
            if (Collection == null)
            {
                return Redirect("/Error");
            }

            if (IsOrderChanged)
            {
                if (Repository.Collections.TryUpdateOrder(Collection.Id, Orders) == false)
                {
                    return Redirect("/Error");
                }
            }

            var collection = Repository.Collections.Get(Collection.Id);
            if (collection == null)
            {
                return Redirect("/Error");
            }

            var preview = collection.Previews.OrderBy(x => x.Order).First();
            var tags = Repository.Tags.GetRangeByString(TagsString);
            Repository.Previews.SetPreviewTags(preview, tags);

            if (IsOrderChanged)
            {
                return Redirect($"/Medias/Collection?id={collection.Id}&returnUrl={ReturnUrl.Replace("&", "$")}");
            }

            return Redirect(ReturnUrl.Replace("$", "&"));
        }

        public IActionResult OnPostDelete(Guid id)
        {
            if (Repository.Collections.TryRemove(id) == false)
            {
                return Redirect("/Error");
            }

            Repository.SaveChanges();

            return Redirect(ReturnUrl.Replace("$", "&"));
        }
    }
}
