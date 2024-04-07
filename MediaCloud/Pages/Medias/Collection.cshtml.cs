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
using MediaCloud.WebApp.Pages;
using MediaCloud.WebApp.Services.ConfigurationProvider;
using MediaCloud.Extensions;
using MediaCloud.WebApp.Services.ActorProvider;

namespace MediaCloud.Pages.Medias
{
    public class CollectionModel : AuthorizedPageModel
    {
        private readonly CollectionRepository _collectionRepository;
        private readonly TagRepository _tagRepository;

        [BindProperty]
        public Collection Collection { get; set; } = new();
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

        public CollectionModel(IActorProvider actorProvider, CollectionRepository collectionRepository, TagRepository tagRepository) : base(actorProvider)
        {
            _collectionRepository = collectionRepository;
            _tagRepository = tagRepository;
        }

        public IActionResult OnGet(Guid id, string returnUrl = "/Medias")
        {
            Collection = _collectionRepository.Get(id) ?? new();

            if (Collection == null)
            {
                return Redirect("/Error");
            }

            var preview = Collection.Previews.OrderBy(x => x.Order).First();
            var collectionSize = _collectionRepository.GetSize(id);
            CollectionSizeInfo = collectionSize.FormatSize();
            TotalCount = _collectionRepository.GetListCount(id).Result;

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
                if (_collectionRepository.TryUpdateOrder(Collection.Id, Orders) == false)
                {
                    return Redirect("/Error");
                }
            }

            var collection = _collectionRepository.Get(Collection.Id);
            if (collection == null)
            {
                return Redirect("/Error");
            }

            var preview = collection.Previews.OrderBy(x => x.Order).First();
            var tags = _tagRepository.GetRangeByString(TagsString);
            _tagRepository.UpdatePreviewLinks(tags, preview);

            if (IsOrderChanged)
            {
                return Redirect($"/Medias/Collection?id={collection.Id}&returnUrl={ReturnUrl.Replace("&", "$")}");
            }

            return Redirect(ReturnUrl.Replace("$", "&"));
        }

        public IActionResult OnPostDelete(Guid id)
        {
            if (_collectionRepository.TryRemove(id) == false)
            {
                return Redirect("/Error");
            }

            return Redirect(ReturnUrl.Replace("$", "&"));
        }
    }
}
