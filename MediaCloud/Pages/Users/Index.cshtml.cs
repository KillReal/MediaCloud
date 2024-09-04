using Microsoft.AspNetCore.Mvc;
using MediaCloud.Data.Models;
using MediaCloud.Builders.List;
using MediaCloud.Repositories;
using MediaCloud.WebApp.Pages;
using MediaCloud.WebApp.Services.ConfigProvider;
using MediaCloud.WebApp.Services.UserProvider;

namespace MediaCloud.Pages.Users
{
    public class ListModel : AuthorizedPageModel
    {
        private IConfigProvider _configProvider;
        private readonly UserRepository _userRepository;

        public ListModel(IUserProvider userProvider, IConfigProvider configProvider, UserRepository userRepository) 
            : base(userProvider)
        {
            _configProvider = configProvider;
            _userRepository = userRepository;

            CurrentUser = _userProvider.GetCurrent();
            ListBuilder = new(new(), _configProvider.UserSettings);
        }

        [BindProperty]
        public List<User> Users { get; set; } = [];
        [BindProperty]
        public ListBuilder<User> ListBuilder { get; set; }

        public async Task<IActionResult> OnGetAsync(ListRequest request)
        {
            if (CurrentUser == null || CurrentUser.IsAdmin == false)
            {
                return Redirect("/Account/Login");
            }

            ListBuilder = new(request, _configProvider.UserSettings);
            Users = await ListBuilder.BuildAsync(_userRepository);

            return Page();
        }
    }
}
