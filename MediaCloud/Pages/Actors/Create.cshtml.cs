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
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using MediaCloud.WebApp.Services;
using MediaCloud.WebApp;
using MediaCloud.Repositories;
using MediaCloud.WebApp.Services.Repository;

namespace MediaCloud.Pages.Actors
{
    [Authorize]
    public class CreateModel : PageModel
    {
        private IRepository Repository;
        private ILogger Logger;

        [BindProperty]
        public Actor Actor { get; set; } = new();

        public CreateModel(IRepository repository, ILogger<CreateModel> logger)
        {
            Repository = repository;
            Logger = logger;
        }

        public IActionResult OnGet()
        {
            var currentActor = Repository.GetCurrentActor();

            if (currentActor.IsAdmin == false)
            {
                Logger.LogError($"Fail attempt to access to Actor/Create by: {currentActor.Id}");
                return Redirect("/Account/Login");
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var currentActor = Repository.GetCurrentActor();

            if (currentActor.IsAdmin == false)
            {
                Logger.LogError($"Fail attempt to access to Actor/Create by: {currentActor.Id}");
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

            Repository.Actors.Create(Actor);

            return RedirectToPage("/Actors/Index");
        }
    }
}
