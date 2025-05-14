using Microsoft.AspNetCore.Mvc;
using MediaCloud.Data.Models;
using MediaCloud.Services;
using MediaCloud.Repositories;
using MediaCloud.WebApp.Data.Types;
using MediaCloud.WebApp.Pages;
using MediaCloud.WebApp.Services.UserProvider;
using MediaCloud.WebApp.Repositories;
using MediaCloud.WebApp.Services.ConfigProvider;

namespace MediaCloud.Pages.Gallery
{
    public class DetailModel(IUserProvider userProvider, IPictureService pictureService, TagRepository tagRepository,
        PreviewRepository previewRepository, BlobRepository blobRepository, IConfigProvider configProvider) 
        : AuthorizedPageModel(userProvider, configProvider)
    {
        [BindProperty] public Guid PreviewId { get; set; }
        [BindProperty] public PreviewRatingType PreviewRating { get; set; }
        [BindProperty] public string BlobName {get; set; } = "unknown";
        [BindProperty] public string BlobType {get; set; } = "unknown";    
        [BindProperty] public Blob Blob { get; set; } = new Blob();
        [BindProperty] public List<Tag> Tags { get; set; } = [];
        [BindProperty] public string? TagsString { get; set; } = "";
        [BindProperty] public Guid? PrevPreviewId { get; set; }
        [BindProperty] public Guid? NextPreviewId { get; set; }
        [BindProperty] public int RotationDegree { get; set; } = 0;
        [BindProperty] public bool IsAutotaggingEnabled { get; set; } = configProvider.EnvironmentSettings.AutotaggingEnabled;

        [BindProperty] public bool IsUserAnAdmin { get; set; } = userProvider.GetCurrent().IsAdmin;

        public IActionResult OnGet(Guid id)
        {
            TempData["ReturnUrl"] = Request.Headers.Referer.ToString();

            var preview = previewRepository.Get(id);

            if (preview == null)
            {
                return Redirect("/Error");
            }

            PreviewId = preview.Id;
            PreviewRating = preview.Rating;
            BlobName = preview.BlobName;
            BlobType = preview.BlobType;
            Blob = preview.Blob;

            if (IsAutotaggingEnabled)
            {
                IsAutotaggingEnabled = CurrentUser != null && CurrentUser.IsAutotaggingAllowed;
            }

            if (preview.Collection != null)
            {
                var collectionPreviews = preview.Collection.Previews.OrderBy(x => x.Order);
                
                PrevPreviewId = collectionPreviews.LastOrDefault(x => x.Order < preview.Order)?.Id;
                NextPreviewId = collectionPreviews.FirstOrDefault(x => x.Order > preview.Order)?.Id;
            }

            Tags = [.. preview.Tags.OrderBy(x => x.Color)];
            TagsString = string.Join(" ", Tags.Select(x => x.Name.ToLower()));

            return Page();
        }

        public IActionResult OnPost()
        {
            var preview = previewRepository.Get(PreviewId);

            if (preview == null)
            {
                return Redirect("/Error");
            }

            var tags = tagRepository.GetRangeByString(TagsString);
            tagRepository.UpdatePreviewLinks(tags, preview);
            
            var blob = preview.Blob;
            blob.Rate = Blob.Rate;
            blob.UpdatedAt = DateTime.UtcNow;

            if (RotationDegree != 0)
            {
                blob.Preview.Content = pictureService.Rotate(preview.Content, RotationDegree);
                blob.Content = pictureService.Rotate(blob.Content, RotationDegree);
            }
            
            blobRepository.Update(blob);

            if (preview.Rating != PreviewRating)
            {
                preview.Rating = PreviewRating;
                previewRepository.Update(preview);
            }

            return Redirect(TempData["ReturnUrl"]?.ToString() ?? "/Gallery");
        }

        public IActionResult OnPostDelete(Guid id)
        {
            if (previewRepository.TryRemove(id) == false)
            {
                return Redirect("/Error");
            }

            return Redirect(TempData["ReturnUrl"]?.ToString() ?? "/Gallery");
        }
    }
}
