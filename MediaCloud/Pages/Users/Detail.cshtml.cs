using Microsoft.AspNetCore.Mvc;
using MediaCloud.Data.Models;
using MediaCloud.WebApp;
using MediaCloud.Repositories;
using NLog;
using MediaCloud.WebApp.Pages;
using MediaCloud.WebApp.Services.ConfigProvider;
using MediaCloud.WebApp.Services.UserProvider;

namespace MediaCloud.Pages.Users
{
    public class DetailModel : AuthorizedPageModel
    {
        private readonly UserRepository _userRepository;

        [BindProperty]
        public new Data.Models.User User { get; set; }

        public DetailModel(IUserProvider userProvider, UserRepository userRepository, IConfigProvider configProvider) 
            : base(userProvider, configProvider)
        {
            Logger = LogManager.GetLogger("User.Detail");
            _userRepository = userRepository;

            User =  UserProvider.GetCurrent();
        }

        public IActionResult OnGet(Guid id)
        {
            TempData["ReturnUrl"] = Request.Headers.Referer.ToString();

            if (User.IsAdmin == false)
            {
                return Redirect("/Account/Login");
            }

            User = _userRepository.Get(id) ?? new Data.Models.User();
            User.PasswordHash = string.Empty;

            return Page();
        }   

        public IActionResult OnPost()
        {
            var currentActor = UserProvider.GetCurrent();

            if (currentActor.IsAdmin == false)
            {
                Logger.Error("Fail attempt to access to User/Detail by: {User.Id}", User.Id);
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
            User = UserProvider.GetCurrent();

            if (User.IsAdmin && _userRepository.TryRemove(id))
            {
                return Redirect(TempData["ReturnUrl"]?.ToString() ?? "/Users");
            }
            
            Logger.Error("Fail attempt to access to User/Detail?action=Delete by: {User.Id}", User.Id);
            
            return Redirect("/Account/Login");
        }
    }
}
