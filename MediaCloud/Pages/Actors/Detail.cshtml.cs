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
using Microsoft.Extensions.Logging;
using NLog;
using ILogger = NLog.ILogger;
using MediaCloud.WebApp.Pages;
using MediaCloud.WebApp.Services.ConfigProvider;
using MediaCloud.WebApp.Services.ActorProvider;

namespace MediaCloud.Pages.Actors
{
    public class ActorDetailModel : AuthorizedPageModel
    {
        private readonly ActorRepository _actorRepository;

        [BindProperty]
        public Actor Actor { get; set; }

        [BindProperty]
        public string ReturnUrl { get; set; } = "/Actors";

        public ActorDetailModel(IActorProvider actorProvider, ActorRepository actorRepository) : base(actorProvider)
        {
            _logger = LogManager.GetLogger("Actor.Detail");
            _actorRepository = actorRepository;

            Actor =  _actorProvider.GetCurrent();
        }

        public IActionResult OnGet(Guid id, string returnUrl = "/Actors")
        {
            if (Actor.IsAdmin == false)
            {
                return Redirect("/Account/Login");
            }

            ReturnUrl = returnUrl.Replace("$", "&");  
            Actor = _actorRepository.Get(id) ?? new();
            Actor.PasswordHash = string.Empty;

            return Page();
        }   

        public IActionResult OnPost()
        {
            var currentActor = _actorProvider.GetCurrent();

            if (currentActor.IsAdmin == false)
            {
                _logger.Error("Fail attempt to access to Actor/Detail by: {Actor.Id}", Actor.Id);
                return Redirect("/Account/Login");
            }

            var referenceActor = _actorRepository.Get(Actor.Id);

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

            _actorRepository.Update(referenceActor);

            return Redirect(ReturnUrl.Replace("$", "&"));
        }

        public IActionResult OnPostDelete(Guid id)
        {
            Actor = _actorProvider.GetCurrent();

            if (Actor.IsAdmin == false || _actorRepository.TryRemove(id) == false)
            {
                _logger.Error("Fail attempt to access to Actor/Detail?action=Delete by: {Actor.Id}", Actor.Id);
                return Redirect("/Account/Login");
            }

            return Redirect(ReturnUrl.Replace("$", "&"));
        }
    }
}
