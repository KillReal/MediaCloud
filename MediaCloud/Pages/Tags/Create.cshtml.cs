using Microsoft.AspNetCore.Mvc;
using MediaCloud.Data.Models;
using MediaCloud.Repositories;
using MediaCloud.WebApp.Pages;
using MediaCloud.WebApp.Services.ActorProvider;

namespace MediaCloud.Pages.Tags
{
    public class TagCreateModel : AuthorizedPageModel
    {
        private readonly TagRepository _tagRepository;

        [BindProperty]
        public Tag Tag { get; set; } = new();

        public TagCreateModel(IActorProvider actorProvider, TagRepository tagRepository) 
            : base(actorProvider)
        {
            _tagRepository = tagRepository;
        }

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
