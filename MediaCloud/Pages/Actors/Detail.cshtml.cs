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
    public class DetailModel : PageModel
    {
        private readonly ActorRepository ActorRepository;

        [BindProperty]
        public Actor Actor { get; set; }

        [BindProperty]
        public string ReturnUrl { get; set; }

        public DetailModel(AppDbContext context, ILogger<TagRepository> logger,
            IActorProvider actorProvider)
        {
            Actor = actorProvider.GetCurrent() ?? new();

            if (Actor.IsAdmin == false)
            {
                Actor = new();
            }

            ActorRepository = new(context);
        }

        public IActionResult OnGet(Guid id, string returnUrl = "/Tags/Index")
        {
            if (Actor.IsAdmin == false)
            {
                return Redirect("/Account/Login");
            }

            ReturnUrl = returnUrl.Replace("$", "&");  
            Actor = ActorRepository.Get(id) ?? new();
            Actor.PasswordHash = string.Empty;

            return Page();
        }   

        public IActionResult OnPost()
        {
            var referenceActor = ActorRepository.Get(Actor.Id);

            if (string.IsNullOrEmpty(Actor.PasswordHash) == false)
            {
                referenceActor.PasswordHash = SecureHash.Hash(Actor.PasswordHash);
            }

            if (string.IsNullOrEmpty(Actor.InviteCode) == false)
            {
                referenceActor.InviteCode = SecureHash.HashMD5(Actor.InviteCode);
            }

            referenceActor.Name = Actor.Name;
            referenceActor.IsPublic = Actor.IsPublic;
            referenceActor.IsAdmin = Actor.IsAdmin;
            referenceActor.IsActivated = Actor.IsActivated;
            referenceActor.InviteCode = Actor.InviteCode;

            ActorRepository.Update(referenceActor);

            return Redirect(ReturnUrl.Replace("$", "&"));
        }

        public IActionResult OnPostDelete(Guid id)
        {
            if (ActorRepository.TryRemove(id) == false)
            {
                return Redirect("/Error");
            }

            return Redirect(ReturnUrl.Replace("$", "&"));
        }
    }
}
