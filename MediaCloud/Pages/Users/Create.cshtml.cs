using Microsoft.AspNetCore.Mvc;
using MediaCloud.Data.Models;
using MediaCloud.WebApp;
using MediaCloud.Repositories;
using NLog;
using MediaCloud.WebApp.Pages;
using MediaCloud.WebApp.Services.UserProvider;

namespace MediaCloud.Pages.Users
{
    public class CreateModel : AuthorizedPageModel
    {
        private readonly UserRepository _actorRepository;

        [BindProperty]
        public new User User { get; set; } = new();

        public CreateModel(IUserProvider userProvider, UserRepository userRepository) : base(userProvider)
        {
            _logger = LogManager.GetLogger("Users.Create");
            _actorRepository = userRepository;
        }

        public IActionResult OnGet()
        {
            var currentActor = _userProvider.GetCurrent();

            if (currentActor.IsAdmin == false)
            {
                _logger.Error("Fail attempt to access to Actor/Create by: {currentUser.Id}", currentActor.Id);
                return Redirect("/Account/Login");
            }

            return Page();
        }

        public IActionResult OnPost()
        {
            var currentUser = _userProvider.GetCurrent();

            if (currentUser.IsAdmin == false)
            {
                _logger.Error("Fail attempt to access to Actor/Create by: {currentUser.Id}", currentUser.Id);
                return Redirect("/Account/Login");
            }

            if (string.IsNullOrEmpty(User.PasswordHash) == false)
            {
                User.PasswordHash = SecureHash.Hash(User.PasswordHash);
            }

            if (string.IsNullOrEmpty(User.InviteCode) == false)
            {
                User.InviteCode = SecureHash.HashMD5(User.InviteCode);
            }

            _actorRepository.Create(User);

            return RedirectToPage("/Users/Index");
        }
    }
}
