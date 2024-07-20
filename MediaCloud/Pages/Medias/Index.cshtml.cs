using Microsoft.AspNetCore.Mvc;
using MediaCloud.Builders.List;
using MediaCloud.Data.Models;
using MediaCloud.Repositories;
using MediaCloud.WebApp.Pages;
using MediaCloud.WebApp.Services.ConfigProvider;
using MediaCloud.WebApp.Services.ActorProvider;

namespace MediaCloud.Pages.Medias
{
    public class MediaListModel : AuthorizedPageModel
    {
        private readonly IConfigProvider _configProvider;
        private readonly TagRepository _tagRepository;
        private readonly PreviewRepository _previewRepository;

        [BindProperty]
        public List<Preview> Previews { get; set; } = [];

        [BindProperty]
        public ListBuilder<Preview> ListBuilder { get; set; }

        [BindProperty]
        public string ExampleFilter { get; set; }

        [BindProperty]
        public bool IsAutoloadEnabled { get; set; } = true;

        public MediaListModel(IActorProvider actorProvider, IConfigProvider configProvider, TagRepository tagRepository, 
            PreviewRepository previewRepository) : base(actorProvider)
        {
            _configProvider = configProvider;
            _tagRepository = tagRepository;
            _previewRepository = previewRepository;

            var topTagNames = _tagRepository.GetTopUsed(2).Select(x => x.Name.ToLower());
            if (topTagNames.Count() > 1)
            {
                ExampleFilter = $"{topTagNames.First()} !{topTagNames.Last()}";
            }
            else
            {
                ExampleFilter = "Create more tags to filtering";
            }

            ListBuilder = new ListBuilder<Preview>(new(), _configProvider.ActorSettings);
            IsAutoloadEnabled = _configProvider.ActorSettings.ListAutoloadingEnabled;
        }

        public async Task<IActionResult> OnGetAsync(ListRequest request)
        {
            ListBuilder = new ListBuilder<Preview>(request, _configProvider.ActorSettings);
            Previews = await ListBuilder.BuildAsync(_previewRepository);
            IsAutoloadEnabled = request.IsUseAutoload ?? IsAutoloadEnabled;

            return Page();
        }  
    }
}
