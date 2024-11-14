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
        public List<Tag> Tags { get; set; } = [];
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

        public IActionResult OnGet(Guid id)
        {
            Collection = _collectionRepository.Get(id) ?? new();

            if (Collection == null)
            {
                return Redirect("/Error");
            }

            var previewsTags = Collection.Previews.OrderBy(x => x.Order).Select(x => x.Tags);
            IEnumerable<Tag>? tagsUnion = null;

           foreach (var tags in previewsTags)
           {
                tagsUnion ??= tags;
                tagsUnion = tagsUnion.Union(tags);
           }

            Tags = [.. tagsUnion?.OrderBy(x => x.Type)];
            
            var collectionSize = _collectionRepository.GetSize(id);
            CollectionSizeInfo = collectionSize.FormatSize();
            TotalCount = _collectionRepository.GetListCount(id).Result;
            IsAutotaggingAvailable = Collection.Previews.Select(x => x.BlobType).Any(x => x.Contains("image"));

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

            return Redirect($"/Gallery/Collection?id={collection.Id}");
        }

        public IActionResult OnPostDelete(Guid id)
        {
            if (_collectionRepository.TryRemove(id) == false)
            {
                return Redirect("/Error");
            }

            return Redirect(Request.Headers.Referer.ToString());
        }
    }
}
