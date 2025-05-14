using Microsoft.AspNetCore.Mvc;
using MediaCloud.Builders.List;
using MediaCloud.Data.Models;
using MediaCloud.Repositories;
using MediaCloud.WebApp.Pages;
using MediaCloud.Extensions;
using MediaCloud.WebApp.Data.Types;
using MediaCloud.WebApp.Services.ConfigProvider;
using MediaCloud.WebApp.Services.UserProvider;

namespace MediaCloud.Pages.Gallery
{
    public class CollectionModel(IUserProvider userProvider, CollectionRepository collectionRepository, IConfigProvider configProvider) 
        : AuthorizedPageModel(userProvider, configProvider)
    {
        private readonly CollectionRepository _collectionRepository = collectionRepository;
        private readonly IConfigProvider _configProvider = configProvider;

        [BindProperty]
        public Collection Collection { get; set; } = new Collection();
        [BindProperty]
        public List<Preview> Previews { get; set; } = [];
        [BindProperty]
        public List<Tag> Tags { get; set; } = [];
        [BindProperty]
        public bool IsOrderChanged { get; set; }
        [BindProperty]
        public bool IsAutotaggingEnabled { get; set; }
        [BindProperty]
        public ListRequest ListRequest { get; set; } = new ListRequest { Count = 42};
        [BindProperty]
        public int TotalCount { get; set; }
        [BindProperty]
        public List<int> Orders { get; set; } = [];
        [BindProperty]
        public string? CollectionSizeInfo { get; set; }
        [BindProperty]
        public int MaxColumnCount { get; set; }
        [BindProperty]
        public PreviewRatingType AllowedNsfwContent { get; set; }

        public IActionResult OnGet(Guid id)
        {
            TempData["ReturnUrl"] = Request.Headers.Referer.ToString();

            Collection = _collectionRepository.Get(id) ?? new Collection();

            Previews = Collection.Previews.OrderBy(x => x.Order).Take(ListRequest.Count).ToList();

            var previewsTags = Collection.Previews.OrderBy(x => x.Order).Select(x => x.Tags);
            IEnumerable<Tag>? tagsUnion = null;

           foreach (var tags in previewsTags)
           {
                tagsUnion ??= tags;
                tagsUnion = tagsUnion.Union(tags);
           }

            Tags = [.. tagsUnion?.OrderBy(x => x.Color) ?? (IEnumerable<Tag>)Array.Empty<object>()];
            
            var collectionSize = _collectionRepository.GetSize(id);
            CollectionSizeInfo = collectionSize.FormatSize();
            TotalCount = _collectionRepository.GetListCount(id).Result;
            IsAutotaggingEnabled = Collection.Previews.Select(x => x.BlobType).Any(x => x.Contains("image")) 
                && CurrentUser != null && CurrentUser.IsAutotaggingAllowed;
            MaxColumnCount = _configProvider.UserSettings.MaxColumnsCount;
            AllowedNsfwContent = _configProvider.UserSettings.AllowedNSFWContent;

            return Page();
        }

        public IActionResult OnPost()
        {
            if (_collectionRepository.TryUpdateOrder(Collection.Id, Orders) == false)
            {
                return Redirect("/Error");
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

            return Redirect(TempData["ReturnUrl"]?.ToString() ?? "/Gallery");
        }
    }
}
