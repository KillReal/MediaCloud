using Microsoft.AspNetCore.Mvc;
using MediaCloud.Data.Models;
using MediaCloud.Repositories;
using MediaCloud.WebApp.Pages;
using MediaCloud.WebApp.Services.UserProvider;

namespace MediaCloud.Pages.Tags
{
    public class TagDetailModel(IUserProvider actorProvider, TagRepository tagRepository) : AuthorizedPageModel(actorProvider)
    {
        private readonly TagRepository _tagRepository = tagRepository;

        [BindProperty]
        public Tag Tag { get; set; } = new();

        [BindProperty]
        public string ReturnUrl { get; set; } = "/Tags";

        public IActionResult OnGet(Guid id, string returnUrl = "/Tags/Index")
        {
            ReturnUrl = returnUrl.Replace("$", "&");  
            Tag = _tagRepository.Get(id) ?? new();

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
            tag.Type = Tag.Type;

            _tagRepository.Update(tag);

            return Redirect(ReturnUrl.Replace("$", "&"));
        }

        public IActionResult OnPostDelete(Guid id)
        {
            if (_tagRepository.TryRemove(id) == false)
            {
                return Redirect("/Error");
            }

            return Redirect(ReturnUrl.Replace("$", "&"));
        }
    }
}
