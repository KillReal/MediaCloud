using Microsoft.AspNetCore.Mvc;
using MediaCloud.Builders.List;
using MediaCloud.Data.Models;
using MediaCloud.Repositories;
using MediaCloud.WebApp.Pages;
using MediaCloud.WebApp.Services.ConfigProvider;
using MediaCloud.WebApp.Services.UserProvider;
using MediaCloud.WebApp.Repositories;
using MediaCloud.WebApp.Services.Statistic;
using MediaCloud.Extensions;

namespace MediaCloud.Pages.Gallery
{
    public class ListModel : AuthorizedPageModel
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
        [BindProperty]
        public string SpaceUsage {get; set;}
        [BindProperty]
        public int SpaceUsagePercent {get; set;}

        public ListModel(IUserProvider userProvider, IConfigProvider configProvider, TagRepository tagRepository, PreviewRepository previewRepository, StatisticProvider statisticProvider) : base(userProvider)
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

            ListBuilder = new ListBuilder<Preview>(new(), _configProvider.UserSettings);
            IsAutoloadEnabled = _configProvider.UserSettings.ListAutoloadingEnabled;

            var currentUsedSpace = statisticProvider.GetTodaySnapshot().MediasSize;
            SpaceUsage = $"{currentUsedSpace.FormatSize(false, 1)} / ";

            if (CurrentUser != null && CurrentUser.SpaceLimit > 0)
            {
                SpaceUsage += CurrentUser.SpaceLimit + ",0 GB";
                SpaceUsagePercent = Convert.ToInt32((double)currentUsedSpace / CurrentUser.SpaceLimitBytes) * 100;
            }
            else
            {
                SpaceUsage += "unlimited";
                SpaceUsagePercent = 0;
            }
        }

        public async Task<IActionResult> OnGetAsync(ListRequest request)
        {
            ListBuilder = new ListBuilder<Preview>(request, _configProvider.UserSettings);
            Previews = await ListBuilder.BuildAsync(_previewRepository);
            IsAutoloadEnabled = request.IsUseAutoload ?? IsAutoloadEnabled;

            return Page();
        }  
    }
}
