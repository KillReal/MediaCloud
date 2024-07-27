using Microsoft.AspNetCore.Mvc;
using MediaCloud.Data.Models;
using MediaCloud.Repositories;
using MediaCloud.WebApp.Pages;
using MediaCloud.WebApp.Services.UserProvider;

namespace MediaCloud.Pages.Tags
{
    public class TagCreateModel(IUserProvider actorProvider, TagRepository tagRepository) : AuthorizedPageModel(actorProvider)
    {
        private readonly TagRepository _tagRepository = tagRepository;

        [BindProperty]
        public Tag Tag { get; set; } = new();

        public IActionResult OnGet(string ReturnUrl = "/Tags")
        {
            return Page();
        }

        public IActionResult OnPost()
        {
            if (Tag.Alias == null)
            {
                Tag.Alias = string.Empty;
            }

            if (Tag.Description == null)
            {
                Tag.Description = string.Empty;
            }

            _tagRepository.Create(Tag);

            return RedirectToPage("/Tags/Index");
        }
    }
}
