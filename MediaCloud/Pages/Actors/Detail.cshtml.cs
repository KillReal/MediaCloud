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
using Microsoft.Extensions.Logging;

namespace MediaCloud.Pages.Actors
{
    [Authorize]
    public class DetailModel : PageModel
    {
        private readonly IRepository Repository;
        private readonly ILogger Logger;

        [BindProperty]
        public Actor Actor { get; set; }

        [BindProperty]
        public string ReturnUrl { get; set; }

        public DetailModel(IRepository repository, ILogger<DetailModel> logger)
        {
            Repository = repository;
            Logger = logger;
        }

        public IActionResult OnGet(Guid id, string returnUrl = "/Tags/Index")
        {
            Actor = Repository.GetCurrentActor();

            if (Actor.IsAdmin == false)
            {
                return Redirect("/Account/Login");
            }

            ReturnUrl = returnUrl.Replace("$", "&");  
            Actor = Repository.Actors.Get(id) ?? new();
            Actor.PasswordHash = string.Empty;

            return Page();
        }   

        public IActionResult OnPost()
        {
            Actor = Repository.GetCurrentActor();

            if (Actor.IsAdmin == false)
            {
                Logger.LogError($"Fail attempt to access to Actor/Detail by: {Actor.Id}");
                return Redirect("/Account/Login");
            }

            var referenceActor = Repository.Actors.Get(Actor.Id);

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

            Repository.Actors.Update(referenceActor);

            return Redirect(ReturnUrl.Replace("$", "&"));
        }

        public IActionResult OnPostDelete(Guid id)
        {
            Actor = Repository.GetCurrentActor();

            if (Actor.IsAdmin == false || Repository.Actors.TryRemove(id) == false)
            {
                Logger.LogError($"Fail attempt to access to Actor/Detail?action=Delete by: {Actor.Id}");
                return Redirect("/Account/Login");
            }

            return Redirect(ReturnUrl.Replace("$", "&"));
        }
    }
}
