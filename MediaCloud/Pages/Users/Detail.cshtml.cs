using Microsoft.AspNetCore.Mvc;
using MediaCloud.Data.Models;
using MediaCloud.WebApp;
using MediaCloud.Repositories;
using NLog;
using MediaCloud.WebApp.Pages;
using MediaCloud.WebApp.Services.UserProvider;

namespace MediaCloud.Pages.Users
{
    public class DetailModel : AuthorizedPageModel
    {
        private readonly UserRepository _userRepository;

        [BindProperty]
        public new User User { get; set; }

        public DetailModel(IUserProvider userProvider, UserRepository userRepository) : base(userProvider)
        {
            _logger = LogManager.GetLogger("User.Detail");
            _userRepository = userRepository;

            User =  _userProvider.GetCurrent();
        }

        public IActionResult OnGet(Guid id)
        {
            TempData["ReturnUrl"] = Request.Headers.Referer.ToString();

            if (User.IsAdmin == false)
            {
                return Redirect("/Account/Login");
            }

            User = _userRepository.Get(id) ?? new();
            User.PasswordHash = string.Empty;

            return Page();
        }   

        public IActionResult OnPost()
        {
            var currentActor = _userProvider.GetCurrent();

            if (currentActor.IsAdmin == false)
            {
                _logger.Error("Fail attempt to access to User/Detail by: {User.Id}", User.Id);
                return Redirect("/Account/Login");
            }

            var referenceUser = _userRepository.Get(User.Id);

            if (string.IsNullOrEmpty(User.PasswordHash) == false)
            {
                referenceUser.PasswordHash = SecureHash.Hash(User.PasswordHash);
            }

            if (string.IsNullOrEmpty(User.InviteCode) == false)
            {
                referenceUser.InviteCode = SecureHash.HashMD5(User.InviteCode);
            }

            referenceUser.Name = User.Name;
            referenceUser.IsPublic = User.IsPublic;
            referenceUser.IsAdmin = User.IsAdmin;
            referenceUser.IsActivated = User.IsActivated;
            referenceUser.InviteCode = User.InviteCode;
            referenceUser.SpaceLimit = User.SpaceLimit;
            referenceUser.IsAutotaggingAllowed = User.IsAutotaggingAllowed;

            _userRepository.Update(referenceUser);

            return Redirect(TempData["ReturnUrl"]?.ToString() ?? "/Users");
        }

        public IActionResult OnPostDelete(Guid id)
        {
            User = _userProvider.GetCurrent();

            if (User.IsAdmin == false || _userRepository.TryRemove(id) == false)
            {
                _logger.Error("Fail attempt to access to User/Detail?action=Delete by: {User.Id}", User.Id);
                return Redirect("/Account/Login");
            }

            return Redirect(TempData["ReturnUrl"]?.ToString() ?? "/Users");
        }
    }
}
