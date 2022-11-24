﻿using System;
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
using MediaCloud.WebApp.Services;

namespace MediaCloud.Pages.Medias
{
    [Authorize]
    public class DetailModel : PageModel
    {
        private IRepository _repository;

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

        public DetailModel(IRepository repository)
        {
            _repository = repository;
        }

        public IActionResult OnGet(Guid id, string returnUrl = "/Medias/Index")
        {
            var preview = _repository.Previews.Get(id);

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
            var preview = _repository.Previews.Get(PreviewId);

            if (preview == null)
            {
                return Redirect("/Error");
            }

            var tags = _repository.Tags.GetRangeByString(TagsString);
            _repository.Previews.SetPreviewTags(preview, tags);
            
            var media = preview.Media;
            media.Rate = Media.Rate;
            _repository.Medias.Update(media);

            return Redirect(ReturnUrl.Replace("$", "&"));
        }

        public IActionResult OnPostDelete(Guid id)
        {
            var collection = (_repository.Previews.Get(id))?.Collection;

            if (_repository.Previews.TryRemove(id) == false)
            {
                return Redirect("/Error");
            }

            if (collection != null)
            {
                return Redirect($"/Medias/Collection?id={collection.Id}");
            }

            return Redirect(ReturnUrl.Replace("$", "&"));
        }
    }
}
