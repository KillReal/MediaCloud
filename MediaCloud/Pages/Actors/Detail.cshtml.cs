﻿using System;
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
using Microsoft.Extensions.Logging;
using NLog;
using ILogger = NLog.ILogger;

namespace MediaCloud.Pages.Actors
{
    [Authorize]
    public class DetailModel : PageModel
    {
        private readonly IDataService _dataService;
        private readonly ILogger _logger;

        [BindProperty]
        public Actor Actor { get; set; }

        [BindProperty]
        public string ReturnUrl { get; set; } = "/Actors";

        public DetailModel(IDataService dataService)
        {
            _dataService = dataService;
            _logger = LogManager.GetLogger("Actor.Detail");

            Actor = _dataService.GetCurrentActor();
        }

        public IActionResult OnGet(Guid id, string returnUrl = "/Actors")
        {
            if (Actor.IsAdmin == false)
            {
                return Redirect("/Account/Login");
            }

            ReturnUrl = returnUrl.Replace("$", "&");  
            Actor = _dataService.Actors.Get(id) ?? new();
            Actor.PasswordHash = string.Empty;

            return Page();
        }   

        public IActionResult OnPost()
        {
            var currentActor = _dataService.GetCurrentActor();

            if (currentActor.IsAdmin == false)
            {
                _logger.Error("Fail attempt to access to Actor/Detail by: {Actor.Id}", Actor.Id);
                return Redirect("/Account/Login");
            }

            var referenceActor = _dataService.Actors.Get(Actor.Id);

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

            _dataService.Actors.Update(referenceActor);

            return Redirect(ReturnUrl.Replace("$", "&"));
        }

        public IActionResult OnPostDelete(Guid id)
        {
            Actor = _dataService.GetCurrentActor();

            if (Actor.IsAdmin == false || _dataService.Actors.TryRemove(id) == false)
            {
                _logger.Error("Fail attempt to access to Actor/Detail?action=Delete by: {Actor.Id}", Actor.Id);
                return Redirect("/Account/Login");
            }

            return Redirect(ReturnUrl.Replace("$", "&"));
        }
    }
}
