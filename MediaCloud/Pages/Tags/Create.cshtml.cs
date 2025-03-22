using Microsoft.AspNetCore.Mvc;
using MediaCloud.Data.Models;
using MediaCloud.Repositories;
using MediaCloud.WebApp.Pages;
using MediaCloud.WebApp.Services.ConfigProvider;
using MediaCloud.WebApp.Services.UserProvider;

namespace MediaCloud.Pages.Tags
{
    public class CreateModel(IUserProvider userProvider, TagRepository tagRepository, IConfigProvider configProvider) 
        : AuthorizedPageModel(userProvider, configProvider)
    {
        private readonly TagRepository _tagRepository = tagRepository;

        [BindProperty]
        public Tag Tag { get; set; } = new();

        public IActionResult OnGet()
        {
            TempData["ReturnUrl"] = Request.Headers.Referer.ToString();

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

            return Redirect(TempData["ReturnUrl"]?.ToString() ?? "/Tags");
        }
    }
}
