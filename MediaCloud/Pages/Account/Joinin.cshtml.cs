using MediaCloud.Data;
using MediaCloud.Data.Models;
using MediaCloud.Repositories;
using MediaCloud.WebApp.Services.DataService;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NLog;
using System.Security.Claims;

namespace MediaCloud.WebApp.Pages
{
    public class JoininModel : BasePageModel
    {
        [BindProperty]
        public string FailStatus { get; set; } = "";
        [BindProperty]
        public string InviteCode { get; set; } = "";
        [BindProperty]
        public AuthData AuthData { get; set; } = new();

        [BindProperty]
        public string ReturnUrl { get; set; } = "";

        public JoininModel(IDataService dataService) : base(dataService)
        {
            _logger = LogManager.GetLogger("Actor.Joinin");
        }

        public IActionResult OnGet(string returnUrl = "/")
        {
            ReturnUrl = returnUrl;

            return Page();
        }

        public IActionResult OnPost()
        {
            var actor = _dataService.Actors.GetByInviteCode(InviteCode);

            if (actor == null)
            {
                FailStatus = "invite code";
                _logger.Error("Join attempt fail with next invite code: {InviteCode}", InviteCode);
                return Page();
            }

            if (_dataService.Actors.IsNameFree(AuthData.Name) == false)
            {
                FailStatus = "name";
                _logger.Error("Join attempt fail with next name: {AuthData.Name}", AuthData.Name);
                return Page();
            }

            if (AuthData.Password.Length < 6)
            {
                FailStatus = "password";
                _logger.Error("Join attempt fail with next name: {AuthData.Name}", AuthData.Name);
                return Page();
            }

            actor.Name = AuthData.Name;
            actor.PasswordHash = SecureHash.Hash(AuthData.Password);
            actor.IsActivated = true;

            _dataService.Actors.Update(actor);

            _logger.Info("Joined in actor with id: {actor.Id} and invite code: {InviteCode}", actor.Id, InviteCode);
            return Redirect("/Account/Login");
        }
    }
}
    