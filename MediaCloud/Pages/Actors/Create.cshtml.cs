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
using MediaCloud.WebApp.Services.DataService;
using NLog;
using ILogger = NLog.ILogger;
using MediaCloud.WebApp.Pages;

namespace MediaCloud.Pages.Actors
{
    [Authorize]
    public class ActorCreateModel : BasePageModel
    {
        [BindProperty]
        public Actor Actor { get; set; } = new();

        public ActorCreateModel(IDataService dataService) : base(dataService)
        {
            _logger = LogManager.GetLogger("Actors.Create");
        }

        public IActionResult OnGet()
        {
            var currentActor = _dataService.GetCurrentActor();

            if (currentActor.IsAdmin == false)
            {
                _logger.Error("Fail attempt to access to Actor/Create by: {currentActor.Id}", currentActor.Id);
                return Redirect("/Account/Login");
            }

            return Page();
        }

        public IActionResult OnPost()
        {
            var currentActor = _dataService.GetCurrentActor();

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

            _dataService.Actors.Create(Actor);

            return RedirectToPage("/Actors/Index");
        }
    }
}
