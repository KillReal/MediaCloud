using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using MediaCloud.Data.Models;
using MediaCloud.Data;
using MediaCloud.Services;
using MediaCloud.Repositories;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using MediaCloud.WebApp.Services.Statistic;
using MediaCloud.WebApp.Pages;
using MediaCloud.WebApp.Services.ActorProvider;

namespace MediaCloud.Pages.Medias
{
    public class MediaDetailModel : AuthorizedPageModel
    {
        private readonly IPictureService _pictureService;
        private readonly TagRepository _tagRepository;
        private readonly PreviewRepository _previewRepository;
        private readonly MediaRepository _mediaRepository;

        [BindProperty]
        public Guid PreviewId { get; set; }
        [BindProperty]
        public Media Media { get; set; } = new();
        [BindProperty]
        public List<Tag> Tags { get; set; } = new();
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

        public MediaDetailModel(IActorProvider actorProvider, IPictureService pictureService, TagRepository tagRepository,
            PreviewRepository previewRepository, MediaRepository mediaRepository) : base(actorProvider)
        {
            _pictureService = pictureService;
            _tagRepository = tagRepository;
            _previewRepository = previewRepository;
            _mediaRepository = mediaRepository;
        }

        public IActionResult OnGet(Guid id, string returnUrl = "/Medias", string rootReturnUrl = "/")
        {
            var preview = _previewRepository.Get(id);

            if (preview == null)
            {
                return Redirect("/Error");
            }

            PreviewId = preview.Id;
            Media = preview.Media;

            if (preview.Collection != null)
            {
                var collectionPreviews = preview.Collection.Previews.OrderBy(x => x.Order);
                PrevPreviewId = collectionPreviews.LastOrDefault(x => x.Order < preview.Order)?.Id;
                NextPreviewId = collectionPreviews.FirstOrDefault(x => x.Order > preview.Order)?.Id;
            }

            Tags = preview.Tags.OrderBy(x => x.Type).ToList();

            ReturnUrl = returnUrl.Replace("$", "&");
            RootReturnUrl = rootReturnUrl.Replace("$", "&");
            TagsString = string.Join(" ", Tags.Select(x => x.Name.ToLower()));

            return Page();
        }

        public IActionResult OnPost()
        {
            var preview = _previewRepository.Get(PreviewId);

            if (preview == null || Media == null)
            {
                return Redirect("/Error");
            }

            var tags = _tagRepository.GetRangeByString(TagsString);
            _tagRepository.UpdatePreviewLinks(tags, preview);
            
            var media = preview.Media;
            media.Rate = Media.Rate;
            media.UpdatedAt = DateTime.UtcNow;

            if (RotationDegree != 0)
            {
                media.Preview.Content = _pictureService.Rotate(preview.Content, RotationDegree);
                media.Content = _pictureService.Rotate(media.Content, RotationDegree);
                
            }
            
            _mediaRepository.Update(media); 

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
