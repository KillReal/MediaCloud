using Microsoft.AspNetCore.Mvc;
using MediaCloud.Data.Models;
using MediaCloud.Services;
using MediaCloud.Repositories;
using MediaCloud.WebApp.Pages;
using MediaCloud.WebApp.Services.UserProvider;

namespace MediaCloud.Pages.Gallery
{
    public class DetailModel(IUserProvider actorProvider, IPictureService pictureService, TagRepository tagRepository,
        PreviewRepository previewRepository, BlobRepository blobRepository) : AuthorizedPageModel(actorProvider)
    {
        private readonly IPictureService _pictureService = pictureService;
        private readonly TagRepository _tagRepository = tagRepository;
        private readonly PreviewRepository _previewRepository = previewRepository;
        private readonly BlobRepository _blobRepository = blobRepository;

        [BindProperty]
        public Guid PreviewId { get; set; }
        [BindProperty]
        public string BlobName {get; set; } = "unknown";
        [BindProperty]
        public string BlobType {get; set; } = "unknown";    
        [BindProperty]
        public Blob Blob { get; set; } = new();
        [BindProperty]
        public List<Tag> Tags { get; set; } = [];
        [BindProperty]
        public string? TagsString { get; set; } = "";
        [BindProperty]
        public string ReturnUrl { get; set; } = "/";
         [BindProperty]
        public string RootReturnUrl { get; set; } = "/";
        [BindProperty]
        public Guid? PrevPreviewId { get; set; } = null;
        [BindProperty]
        public Guid? NextPreviewId { get; set; } = null;
        [BindProperty]
        public int RotationDegree {get; set;} = 0;

        public IActionResult OnGet(Guid id, string returnUrl = "/Blobs", string rootReturnUrl = "/")
        {
            var preview = _previewRepository.Get(id);

            if (preview == null)
            {
                return Redirect("/Error");
            }

            PreviewId = preview.Id;
            BlobName = preview.BlobName;
            BlobType = preview.BlobType;
            Blob = preview.Blob;

            if (preview.Collection != null)
            {
                var collectionPreviews = preview.Collection.Previews.OrderBy(x => x.Order);
                PrevPreviewId = collectionPreviews.LastOrDefault(x => x.Order < preview.Order)?.Id;
                NextPreviewId = collectionPreviews.FirstOrDefault(x => x.Order > preview.Order)?.Id;
            }

            Tags = [.. preview.Tags.OrderBy(x => x.Type)];

            ReturnUrl = returnUrl.Replace("$", "&");
            RootReturnUrl = rootReturnUrl.Replace("$", "&");
            TagsString = string.Join(" ", Tags.Select(x => x.Name.ToLower()));

            return Page();
        }

        public IActionResult OnPost()
        {
            var preview = _previewRepository.Get(PreviewId);

            if (preview == null || Blob == null)
            {
                return Redirect("/Error");
            }

            var tags = _tagRepository.GetRangeByString(TagsString);
            _tagRepository.UpdatePreviewLinks(tags, preview);
            
            var blob = preview.Blob;
            blob.Rate = Blob.Rate;
            blob.UpdatedAt = DateTime.UtcNow;

            if (RotationDegree != 0)
            {
                blob.Preview.Content = _pictureService.Rotate(preview.Content, RotationDegree);
                blob.Content = _pictureService.Rotate(blob.Content, RotationDegree);
            }
            
            _blobRepository.Update(blob); 

            return Redirect(ReturnUrl.Replace("$", "&"));
        }

        public IActionResult OnPostDelete(Guid id)
        {
            if (_previewRepository.TryRemove(id) == false)
            {
                return Redirect("/Error");
            }

            return Redirect(ReturnUrl.Replace("$", "&"));
        }
    }
}
