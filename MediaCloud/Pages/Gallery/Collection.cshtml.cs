using Microsoft.AspNetCore.Mvc;
using MediaCloud.Builders.List;
using MediaCloud.Data.Models;
using MediaCloud.Repositories;
using MediaCloud.WebApp.Pages;
using MediaCloud.Extensions;
using MediaCloud.WebApp.Data.Types;
using MediaCloud.WebApp.Services.ConfigProvider;
using MediaCloud.WebApp.Services.UserProvider;
using Microsoft.Extensions.Primitives;

namespace MediaCloud.Pages.Gallery
{
    public class CollectionModel(IUserProvider userProvider, CollectionRepository collectionRepository, 
        TagRepository tagRepository, IConfigProvider configProvider) 
        : AuthorizedPageModel(userProvider, configProvider)
    {
        private readonly CollectionRepository _collectionRepository = collectionRepository;
        private readonly TagRepository _tagRepository = tagRepository;
        private readonly IConfigProvider _configProvider = configProvider;

        [BindProperty]
        public Collection Collection { get; set; } = new Collection();
        [BindProperty]
        public List<Preview> Previews { get; set; } = [];
        [BindProperty]
        public List<Tag> Tags { get; set; } = [];
        [BindProperty]
        public string TagsString { get; set; } = string.Empty;
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

        private static List<Tag> GetCollectionTags(List<Preview> previews)
        {
            var previewsTags = previews.OrderBy(x => x.Order).Select(x => x.Tags);
            IEnumerable<Tag>? tagsUnion = null;

            foreach (var tags in previewsTags)
            {
                tagsUnion ??= tags;
                tagsUnion = tagsUnion.Union(tags);
            }

            return [.. tagsUnion?.OrderBy(x => x.Color) ?? (IEnumerable<Tag>)Array.Empty<object>()];
        }
        
        public IActionResult OnGet(Guid id)
        {
            TempData["ReturnUrl"] = Request.Headers.Referer.ToString();

            Collection = _collectionRepository.Get(id) ?? new Collection();
            Previews = Collection.Previews.OrderBy(x => x.Order).Take(ListRequest.Count).ToList();
            
            Tags = GetCollectionTags(Previews);
            TagsString = string.Join(" ", Tags.Select(x => x.Name.ToLower()));
            
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
            
            var previews = collection.Previews.ToList();
            var currentTags = GetCollectionTags(previews);
            var currentTagsString = string.Join(" ", currentTags.Select(x => x.Name.ToLower()));

            var tagStringsToRemove = new List<string>();

            foreach (var currentTagString in currentTagsString.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            {
                if (TagsString.Contains(currentTagString) == false)
                {
                    tagStringsToRemove.Add(currentTagString);
                }
            }

            var tagStringsToAdd = new List<string>();
            foreach (var tagString in TagsString.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            {
                if (currentTagsString.Contains(tagString) == false)
                {
                    tagStringsToAdd.Add(tagString);
                }
            }

            foreach (var preview in previews)
            {
                var previewTagStrings = preview.Tags.Select(x => x.Name.ToLower()).ToList();
                previewTagStrings.AddRange(tagStringsToAdd);
                previewTagStrings.RemoveAll(x => tagStringsToRemove.Contains(x));
                var newTags = _tagRepository.GetRangeByString(string.Join(' ', previewTagStrings));
                
                _tagRepository.UpdatePreviewLinks(newTags, preview);
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
