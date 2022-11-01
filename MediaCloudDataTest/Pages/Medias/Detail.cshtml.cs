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

        public DetailModel(AppDbContext context)
        {
            PreviewRepository = new(context);
            TagRepository = new(context);
            MediaRepository = new(context);
        }

        public IActionResult OnGet(Guid id, int offset = 0, string returnUrl = "/Medias/Index")
        {
            var preview = PreviewRepository.Get(id);
            var media = MediaRepository.Get(id);

            if (preview == null)
            {
                if (media == null)
                {
                    return Redirect("/Error");
                }
                else
                {
                    PreviewId = media.Preview.Id;
                    Media = media;
                    Tags = media.Preview.Tags.OrderBy(x => x.Type).ToList();
                    ReturnUrl = returnUrl.Replace("$", "&");
                    TagsString = string.Join(" ", Tags.Select(x => x.Name.ToLower()));

                    return Page();
                }
            }


            PreviewId = preview.Id;
            Media = MediaRepository.GetOffsetByPreview(PreviewId, offset);
            Tags = preview.Tags.OrderBy(x => x.Type).ToList();
            ReturnUrl = returnUrl.Replace("$", "&");
            TagsString = string.Join(" ", Tags.Select(x => x.Name.ToLower()));

            return Page();
        }

        public IActionResult OnPost()
        {
            var tags = TagRepository.GetRangeByString(TagsString);
            var preview = PreviewRepository.Get(PreviewId);
            preview.Tags.Clear();
            preview.Tags.AddRange(tags);
            PreviewRepository.Update(preview);

            var media = preview.Medias.First(x => x.Id == Media.Id);
            media.Rank = Media.Rank;
            MediaRepository.Update(media);

            return Redirect(ReturnUrl.Replace("$", "&"));
        }

        public IActionResult OnPostDelete(Guid id)
        {
            var preview = PreviewRepository.Get(id);

            MediaRepository.Remove(preview.Medias);
            PreviewRepository.Remove(preview);

            return Redirect(ReturnUrl.Replace("$", "&"));
        }
    }
}
