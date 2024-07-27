using Microsoft.AspNetCore.Mvc;
using MediaCloud.Data.Models;
using MediaCloud.WebApp;
using MediaCloud.Repositories;
using NLog;
using MediaCloud.WebApp.Pages;
using MediaCloud.WebApp.Services.UserProvider;

namespace MediaCloud.Pages.Actors
{
    public class ActorCreateModel : AuthorizedPageModel
    {
        private readonly UserRepository _actorRepository;

        [BindProperty]
        public User Actor { get; set; } = new();

        public ActorCreateModel(IUserProvider actorProvider, UserRepository actorRepository) : base(actorProvider)
        {
            _logger = LogManager.GetLogger("Actors.Create");
            _actorRepository = actorRepository;
        }

        public IActionResult OnGet()
        {
            var currentActor = _actorProvider.GetCurrent();

            if (currentActor.IsAdmin == false)
            {
                _logger.Error("Fail attempt to access to Actor/Create by: {currentActor.Id}", currentActor.Id);
                return Redirect("/Account/Login");
            }

            return Page();
        }

        public IActionResult OnPost()
        {
            var currentActor = _actorProvider.GetCurrent();

            if (currentActor.IsAdmin == false)
            {
                _logger.Error("Fail attempt to access to Actor/Create by: {currentActor.Id}", currentActor.Id);
                return Redirect("/Account/Login");
            }

            if (string.IsNullOrEmpty(Actor.PasswordHash) == false)
            {
                Actor.PasswordHash = SecureHash.Hash(Actor.PasswordHash);
            }

            if (string.IsNullOrEmpty(Actor.InviteCode) == false)
            {
                Actor.InviteCode = SecureHash.HashMD5(Actor.InviteCode);
            }

            _actorRepository.Create(Actor);

            return RedirectToPage("/Actors/Index");
        }
    }
}
