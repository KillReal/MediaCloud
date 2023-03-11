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
using MediaCloud.Builders.Components;
using MediaCloud.Repositories;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using MediaCloud.WebApp.Services.Repository;
using MediaCloud.WebApp.Services.Statistic;

namespace MediaCloud.Pages.Medias
{
    [Authorize]
    public class DetailModel : PageModel
    {
        private IRepository Repository;
        private IStatisticService StatisticService;

        [BindProperty]
        public Guid PreviewId { get; set; }
        [BindProperty]
        public Media Media { get; set; }
        [BindProperty]
        public List<Tag> Tags { get; set; }
        [BindProperty]
        public string? TagsString { get; set; } = "";
        [BindProperty]
        public string ReturnUrl { get; set; }
        [BindProperty]
        public Guid? PrevPreviewId { get; set; } = null;
        [BindProperty]
        public Guid? NextPreviewId { get; set; } = null;

        public DetailModel(IRepository repository, IStatisticService statisticService)
        {
            Repository = repository;
            StatisticService = statisticService;
        }

        public IActionResult OnGet(Guid id, string returnUrl = "/Medias/Index")
        {
            var preview = Repository.Previews.Get(id);

            if (preview == null)
            {
                return Redirect("/Error");
            }

            PreviewId = preview.Id;
            Media = preview.Media;

            if (preview.Collection != null)
            {
                PrevPreviewId = preview.Collection.Previews.FirstOrDefault(x => x.Order == preview.Order - 1)?.Id;
                NextPreviewId = preview.Collection.Previews.FirstOrDefault(x => x.Order == preview.Order + 1)?.Id;
            }

            Tags = preview.Tags.OrderBy(x => x.Type)
                               .ToList();

            ReturnUrl = returnUrl.Replace("$", "&");
            TagsString = string.Join(" ", Tags.Select(x => x.Name.ToLower()));

            StatisticService.ActivityFactorRaised.Invoke();

            return Page();
        }

        public IActionResult OnPost()
        {
            var preview = Repository.Previews.Get(PreviewId);

            if (preview == null)
            {
                return Redirect("/Error");
            }

            var tags = Repository.Tags.GetRangeByString(TagsString);
            Repository.Previews.SetPreviewTags(preview, tags);
            
            var media = preview.Media;
            media.Rate = Media.Rate;
            Repository.Medias.Update(media);

            return Redirect(ReturnUrl.Replace("$", "&"));
        }

        public IActionResult OnPostDelete(Guid id)
        {
            if (Repository.Previews.TryRemove(id) == false)
            {
                return Redirect("/Error");
            }

            return Redirect(ReturnUrl.Replace("$", "&"));
        }
    }
}
