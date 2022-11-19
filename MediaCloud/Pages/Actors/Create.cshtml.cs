using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using MediaCloud.Data;
using MediaCloud.Services;
using MediaCloud.Data.Models;
using MediaCloud.Repositories;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using MediaCloud.WebApp.Services;
using MediaCloud.WebApp;

namespace MediaCloud.Pages.Actors
{
    [Authorize]
    public class CreateModel : PageModel
    {
        private ActorRepository _repository;
        private IActorProvider _provider;
        private ILogger _logger;

        [BindProperty]
        public Actor Actor { get; set; } = new();

        public CreateModel(AppDbContext context, ILogger<CreateModel> logger, 
            IActorProvider actorProvider)
        {
            _repository = new(context);
            _provider = actorProvider;
            _logger = logger;
        }

        public IActionResult OnGet()
        {
            var currentActor = _provider.GetCurrent() ?? new();

            if (currentActor.IsAdmin == false)
            {
                _logger.LogError($"Fail attempt to access to Actor/Create by: {currentActor.Id}");
                return Redirect("/Login");
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var currentActor = _provider.GetCurrent() ?? new();

            if (currentActor.IsAdmin == false)
            {
                _logger.LogError($"Fail attempt to access to Actor/Create by: {currentActor.Id}");
                return Redirect("/Login");
            }

            if (string.IsNullOrEmpty(Actor.PasswordHash) == false)
            {
                Actor.PasswordHash = SecurePasswordHasher.Hash(Actor.PasswordHash);
            }

            _repository.Create(Actor);

            return RedirectToPage("/Actors/Index");
        }
    }
}
