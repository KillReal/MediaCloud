using Microsoft.AspNetCore.Mvc;
using MediaCloud.Data.Models;
using MediaCloud.Builders.List;
using MediaCloud.Repositories;
using MediaCloud.WebApp.Pages;
using MediaCloud.WebApp.Services.ConfigProvider;
using MediaCloud.WebApp.Services.ActorProvider;

namespace MediaCloud.Pages.Tags
{
    public class TagListModel : AuthorizedPageModel
    {
        private readonly TagRepository _tagRepository;
        private readonly IConfigProvider _configProvider;

        [BindProperty]
        public List<Tag> Tags { get; set; } = [];
        [BindProperty]
        public ListBuilder<Tag> ListBuilder { get; set; }
        [BindProperty]
        public bool IsAutoloadEnabled { get; set; } = false;

        public TagListModel(IActorProvider actorProvider, IConfigProvider configProvider, TagRepository tagRepository) 
            : base(actorProvider)
        {
            _configProvider = configProvider;
            _tagRepository = tagRepository;

            ListBuilder = new(new(), _configProvider.ActorSettings);
        }

        public async Task<IActionResult> OnGetAsync(ListRequest request)
        {
            ListBuilder = new(request, _configProvider.ActorSettings);
            Tags = await ListBuilder.BuildAsync(_tagRepository);

            return Page();
        }
    }
}
