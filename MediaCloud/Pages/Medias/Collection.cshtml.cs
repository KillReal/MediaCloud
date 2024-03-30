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
using MediaCloud.WebApp.Services.DataService;
using MediaCloud.WebApp.Pages;

namespace MediaCloud.Pages.Medias
{
    public class CollectionModel : AuthorizedPageModel
    {
        [BindProperty]
        public Collection Collection { get; set; }
        [BindProperty]
        public string ReturnUrl { get; set; } = "/Medias";
        [BindProperty]
        public List<Tag> Tags { get; set; } = new();
        [BindProperty]
        public string? TagsString { get; set; }
        [BindProperty]
        public bool IsOrderChanged { get; set; } = false;
        [BindProperty]
        public ListRequest ListRequest { get; set; } = new() { Count = 42};
        [BindProperty]
        public int TotalCount { get; set; }
        [BindProperty]
        public List<int> Orders { get; set; } = new();
        [BindProperty]
        public string? CollectionSizeInfo { get; set; }

        public CollectionModel(IDataService dataService) : base(dataService)
        {
            Collection = new();
        }

        public IActionResult OnGet(Guid id, string returnUrl = "/Medias")
        {
            Collection = _dataService.Collections.Get(id) ?? new();

            if (Collection == null)
            {
                return Redirect("/Error");
            }

            var preview = Collection.Previews.OrderBy(x => x.Order).First();
            var collectionSize = _dataService.Collections.GetSize(id);
            CollectionSizeInfo = PictureService.FormatSize(collectionSize);
            TotalCount = _dataService.Collections.GetListCount(id).Result;

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
                if (_dataService.Collections.TryUpdateOrder(Collection.Id, Orders) == false)
                {
                    return Redirect("/Error");
                }
            }

            var collection = _dataService.Collections.Get(Collection.Id);
            if (collection == null)
            {
                return Redirect("/Error");
            }

            var preview = collection.Previews.OrderBy(x => x.Order).First();
            var tags = _dataService.Tags.GetRangeByString(TagsString);
            _dataService.Tags.UpdatePreviewLinks(tags, preview);

            if (IsOrderChanged)
            {
                return Redirect($"/Medias/Collection?id={collection.Id}&returnUrl={ReturnUrl.Replace("&", "$")}");
            }

            return Redirect(ReturnUrl.Replace("$", "&"));
        }

        public IActionResult OnPostDelete(Guid id)
        {
            if (_dataService.Collections.TryRemove(id) == false)
            {
                return Redirect("/Error");
            }

            return Redirect(ReturnUrl.Replace("$", "&"));
        }
    }
}
