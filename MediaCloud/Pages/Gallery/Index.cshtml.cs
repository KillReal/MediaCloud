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
using MediaCloud.WebApp.Data.Types;

namespace MediaCloud.Pages.Gallery
{
    public class ListModel : AuthorizedPageModel
    {
        private readonly IConfigProvider _configProvider;
        private readonly TagRepository _tagRepository;
        private readonly PreviewRepository _previewRepository;
        private const int SpaceSizePrecision = 1;

        [BindProperty]
        public List<Preview> Previews { get; set; } = [];

        [BindProperty]
        public ListBuilder<Preview> ListBuilder { get; set; }

        [BindProperty]
        public string ExampleFilter { get; set; }

        [BindProperty]
        public bool IsAutoloadEnabled { get; set; }
        [BindProperty]
        public string SpaceUsage { get; set; }
        [BindProperty]
        public int SpaceUsagePercent { get; set; }
        [BindProperty]
        public int MaxColumnCount { get; set; }
        [BindProperty]
        public PreviewRatingType AllowedNSFWContent { get; set; }

        public ListModel(IUserProvider userProvider, IConfigProvider configProvider, TagRepository tagRepository, 
            PreviewRepository previewRepository, StatisticProvider statisticProvider) 
            : base(userProvider, configProvider)
        {
            _configProvider = configProvider;
            _tagRepository = tagRepository;
            _previewRepository = previewRepository;

            var topTagNames = _tagRepository.GetTopUsed(2).Select(x => x.Name.ToLower());
            ExampleFilter = topTagNames.Count() > 1 
                ? $"{topTagNames.First()} !{topTagNames.Last()}" 
                : "Create more tags to filtering";

            ListBuilder = new ListBuilder<Preview>(new(), _configProvider.UserSettings);
            IsAutoloadEnabled = _configProvider.UserSettings.ListAutoloadingEnabled;

            var currentUsedSpace = statisticProvider.GetTodaySnapshot().MediasSize;
            SpaceUsage = $"{currentUsedSpace.FormatSize(true, SpaceSizePrecision)} / ";

            MaxColumnCount = configProvider.UserSettings.MaxColumnsCount;
            AllowedNSFWContent = configProvider.UserSettings.AllowedNSFWContent;

            if (CurrentUser is { SpaceLimit: > 0 })
            {
                SpaceUsage += CurrentUser.SpaceLimitBytes.FormatSize(true, SpaceSizePrecision);
                SpaceUsagePercent = Convert.ToInt32((double)currentUsedSpace / CurrentUser.SpaceLimitBytes * 100);
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
