﻿using Microsoft.AspNetCore.Mvc;
using MediaCloud.Builders.List;
using MediaCloud.Data.Models;
using MediaCloud.Repositories;
using MediaCloud.WebApp.Pages;
using MediaCloud.Extensions;
using MediaCloud.WebApp.Services.UserProvider;

namespace MediaCloud.Pages.Gallery
{
    public class CollectionModel(IUserProvider userProvider, CollectionRepository collectionRepository, TagRepository tagRepository) : AuthorizedPageModel(userProvider)
    {
        private readonly CollectionRepository _collectionRepository = collectionRepository;
        private readonly TagRepository _tagRepository = tagRepository;

        [BindProperty]
        public Collection Collection { get; set; } = new();
        [BindProperty]
        public string ReturnUrl { get; set; } = "/Gallery";
        [BindProperty]
        public List<Tag> Tags { get; set; } = [];
        [BindProperty]
        public string? TagsString { get; set; }
        [BindProperty]
        public bool IsOrderChanged { get; set; } = false;
        [BindProperty]
        public bool IsAutotaggingAvailable { get; set; } = false;
        [BindProperty]
        public ListRequest ListRequest { get; set; } = new() { Count = 42};
        [BindProperty]
        public int TotalCount { get; set; }
        [BindProperty]
        public List<int> Orders { get; set; } = [];
        [BindProperty]
        public string? CollectionSizeInfo { get; set; }

        public IActionResult OnGet(Guid id, string returnUrl = "/Gallery")
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
            IsAutotaggingAvailable = Collection.Previews.Select(x => x.BlobType).Any(x => x.Contains("image"));

            Tags = [.. preview.Tags.OrderBy(x => x.Type)];
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
                return Redirect($"/Gallery/Collection?id={collection.Id}&returnUrl={ReturnUrl.Replace("&", "$")}");
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
