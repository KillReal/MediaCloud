using Microsoft.AspNetCore.Mvc;
using MediaCloud.Data.Models;
using MediaCloud.Builders.List;
using MediaCloud.Repositories;
using MediaCloud.WebApp.Pages;
using MediaCloud.WebApp.Services.ConfigProvider;
using MediaCloud.WebApp.Services.UserProvider;

namespace MediaCloud.Pages.Tags
{
    public class ListModel : AuthorizedPageModel
    {
        private readonly TagRepository _tagRepository;
        private readonly IConfigProvider _configProvider;

        [BindProperty]
        public List<Tag> Tags { get; set; } = [];
        [BindProperty]
        public ListBuilder<Tag> ListBuilder { get; set; }
        [BindProperty]
        public bool IsAutoloadEnabled { get; set; } = false;

        public ListModel(IUserProvider userProvider, IConfigProvider configProvider, TagRepository tagRepository) 
            : base(userProvider)
        {
            _configProvider = configProvider;
            _tagRepository = tagRepository;

            ListBuilder = new(new(), _configProvider.UserSettings);
        }

        public async Task<IActionResult> OnGetAsync(ListRequest request)
        {
            ListBuilder = new(request, _configProvider.UserSettings);
            Tags = await ListBuilder.BuildAsync(_tagRepository);

            return Page();
        }
    }
}
