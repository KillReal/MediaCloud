using MediaCloud.Data;
using MediaCloud.Data.Models;
using MediaCloud.Repositories;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace MediaCloud.WebApp.Pages
{
    public class JoininModel : PageModel
    {
        private ActorRepository _repository;
        private ILogger _logger;

        [BindProperty]
        public string FailStatus { get; set; }
        [BindProperty]
        public string InviteCode { get; set; }
        [BindProperty]
        public AuthData AuthData { get; set; }

        [BindProperty]
        public string ReturnUrl { get; set; }

        public JoininModel(AppDbContext context, ILogger<LoginModel> logger)
        {
            _repository = new(context);
            _logger = logger;
        }

        public IActionResult OnGet(string returnUrl = "/")
        {
            AuthData = new();
            ReturnUrl = returnUrl;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var actor = _repository.GetByInviteCode(InviteCode);

            if (actor == null)
            {
                FailStatus = "invite code";
                _logger.LogError($"Join attempt fail with next invite code: {InviteCode}");
                return Page();
            }

            if (_repository.IsNameFree(AuthData.Name) == false)
            {
                FailStatus = "name";
                _logger.LogError($"Join attempt fail with next name: {AuthData.Name}");
                return Page();
            }

            if (AuthData.Password.Length < 6)
            {
                FailStatus = "password";
                _logger.LogError($"Join attempt fail with next name: {AuthData.Name}");
                return Page();
            }

            actor.Name = AuthData.Name;
            actor.PasswordHash = SecureHash.Hash(AuthData.Password);
            actor.IsActivated = true;
            _repository.Update(actor);

            _logger.LogInformation($"Joined in actor with id: {actor.Id} and invite code: {InviteCode}");
            return Redirect("/Account/Login");
        }
    }
}
    