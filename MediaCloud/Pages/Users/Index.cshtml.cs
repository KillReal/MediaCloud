using Microsoft.AspNetCore.Mvc;
using MediaCloud.Data.Models;
using MediaCloud.Builders.List;
using MediaCloud.Repositories;
using MediaCloud.WebApp.Pages;
using MediaCloud.WebApp.Services.ConfigProvider;
using MediaCloud.WebApp.Services.UserProvider;

namespace MediaCloud.Pages.Actors
{
    public class ActorListModel : AuthorizedPageModel
    {
        private IConfigProvider _configProvider;
        private readonly UserRepository _actorRepository;

        public ActorListModel(IUserProvider actorProvider, IConfigProvider configProvider, UserRepository actorRepository) 
            : base(actorProvider)
        {
            _configProvider = configProvider;
            _actorRepository = actorRepository;

            CurrentActor = _actorProvider.GetCurrent();
            ListBuilder = new(new(), _configProvider.ActorSettings);
        }

        [BindProperty]
        public List<User> Actors { get; set; } = [];
        [BindProperty]
        public ListBuilder<User> ListBuilder { get; set; }

        public async Task<IActionResult> OnGetAsync(ListRequest request)
        {
            if (CurrentActor == null || CurrentActor.IsAdmin == false)
            {
                return Redirect("/Account/Login");
            }

            ListBuilder = new(request, _configProvider.ActorSettings);
            Actors = await ListBuilder.BuildAsync(_actorRepository);

            return Page();
        }
    }
}
