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
using MediaCloud.WebApp.Services.DataService;
using MediaCloud.WebApp.Services.Statistic;
using MediaCloud.WebApp.Pages;

namespace MediaCloud.Pages.Medias
{
    public class MediaDetailModel : AuthorizedPageModel
    {
        [BindProperty]
        public Guid PreviewId { get; set; }
        [BindProperty]
        public Media Media { get; set; }
        [BindProperty]
        public List<Tag> Tags { get; set; } = new();
        [BindProperty]
        public string? TagsString { get; set; } = "";
        [BindProperty]
        public string ReturnUrl { get; set; } = "/";
        [BindProperty]
        public Guid? PrevPreviewId { get; set; } = null;
        [BindProperty]
        public Guid? NextPreviewId { get; set; } = null;

        public MediaDetailModel(IDataService dataService) : base(dataService)
        {
            Media = new();
        }

        public IActionResult OnGet(Guid id, string returnUrl = "/Medias")
        {
            var preview = _dataService.Previews.Get(id);

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
            TagsString = string.Join(" ", Tags.Select(x => x.Name.ToLower()));

            return Page();
        }

        public IActionResult OnPost()
        {
            var preview = _dataService.Previews.Get(PreviewId);

            if (preview == null || Media == null)
            {
                return Redirect("/Error");
            }

            var tags = _dataService.Tags.GetRangeByString(TagsString);
            _dataService.Tags.UpdatePreviewLinks(tags, preview);
            
            var media = preview.Media;
            media.Rate = Media.Rate;
            _dataService.Medias.Update(media);

            return Redirect(ReturnUrl.Replace("$", "&"));
        }

        public IActionResult OnPostDelete(Guid id)
        {
            if (_dataService.Previews.TryRemove(id) == false)
            {
                return Redirect("/Error");
            }

            return Redirect(ReturnUrl.Replace("$", "&"));
        }
    }
}
