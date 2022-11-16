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

namespace MediaCloud.Pages.Medias
{
    public class DetailModel : PageModel
    {
        private PreviewRepository PreviewRepository;
        private TagRepository TagRepository;
        private MediaRepository MediaRepository;

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

        public DetailModel(AppDbContext context, ILogger<DetailModel> logger)
        {
            PreviewRepository = new(context, logger);
            TagRepository = new(context, logger);
            MediaRepository = new(context, logger);
        }

        public IActionResult OnGet(Guid id, string returnUrl = "/Medias/Index")
        {
            var preview = PreviewRepository.Get(id);

            if (preview == null)
            {
                return Redirect("/Error");
            }

            PreviewId = preview.Id;
            Media = preview.Media;
            if (preview.Collection != null)
            {
                Tags = preview.Collection.Previews.OrderBy(x => x.Order)
                                                  .First()
                                                  .Tags.OrderBy(x => x.Type)
                                                       .ToList();
            }
            else
            {
                Tags = preview.Tags.OrderBy(x => x.Type)
                                   .ToList();
            }

            ReturnUrl = returnUrl.Replace("$", "&");
            TagsString = string.Join(" ", Tags.Select(x => x.Name.ToLower()));

            return Page();
        }

        public IActionResult OnPost()
        {
            var preview = PreviewRepository.Get(PreviewId);

            if (preview == null)
            {
                return Redirect("/Error");
            }

            var tags = TagRepository.GetRangeByString(TagsString);
            PreviewRepository.SetPreviewTags(preview, tags);
            
            var media = preview.Media;
            media.Rate = Media.Rate;
            MediaRepository.Update(media);

            return Redirect(ReturnUrl.Replace("$", "&"));
        }

        public IActionResult OnPostDelete(Guid id)
        {
            var collection = PreviewRepository.Get(id)?.Collection;

            if (!PreviewRepository.TryRemove(id))
            {
                return Redirect("/Error");
            }

            if (collection != null)
            {
                return Redirect($"/Medias/Collection?id={collection.Previews[0].Id}");
            }

            return Redirect("/Medias/Index");
        }
    }
}
