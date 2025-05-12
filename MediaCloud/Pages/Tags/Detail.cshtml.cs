using Microsoft.AspNetCore.Mvc;
using MediaCloud.Data.Models;
using MediaCloud.Repositories;
using MediaCloud.WebApp.Pages;
using MediaCloud.WebApp.Services.ConfigProvider;
using MediaCloud.WebApp.Services.UserProvider;

namespace MediaCloud.Pages.Tags
{
    public class DetailModel(IUserProvider userProvider, TagRepository tagRepository, IConfigProvider configProvider) 
        : AuthorizedPageModel(userProvider, configProvider)
    {
        private readonly TagRepository _tagRepository = tagRepository;

        [BindProperty]
        public Tag Tag { get; set; } = new();

        public IActionResult OnGet(Guid id)
        {
            Tag = _tagRepository.Get(id) ?? new();
            TempData["ReturnUrl"] = Request.Headers.Referer.ToString();

            return Page();
        }   

        public IActionResult OnPost()
        {
            var tag = _tagRepository.Get(Tag.Id);

            if (tag == null)
            {
                return Redirect("/Error");
            }

            if (Tag.Alias == null)
            {
                Tag.Alias = string.Empty;
            }

            if (Tag.Description == null)
            {
                Tag.Description = string.Empty;
            }

            tag.Name = Tag.Name;
            tag.Description = Tag.Description;
            tag.Alias = Tag.Alias;
            tag.Color = Tag.Color;

            _tagRepository.Update(tag);

            return Redirect(TempData["ReturnUrl"]?.ToString() ?? "/Tags");
        }

        public IActionResult OnPostDelete(Guid id)
        {
            if (_tagRepository.TryRemove(id) == false)
            {
                return Redirect("/Error");
            }

            return Redirect(TempData["ReturnUrl"]?.ToString() ?? "/Tags");
        }
    }
}
