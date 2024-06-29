using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MediaCloud.Data;
using MediaCloud.Data.Models;
using MediaCloud.Builders.List;
using MediaCloud.Services;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using MediaCloud.WebApp.Services;
using MediaCloud.Repositories;
using MediaCloud.WebApp.Pages;
using MediaCloud.WebApp.Services.ConfigProvider;
using MediaCloud.WebApp.Services.ActorProvider;

namespace MediaCloud.Pages.Actors
{
    public class ActorListModel : AuthorizedPageModel
    {
        private IConfigProvider _configProvider;
        private readonly ActorRepository _actorRepository;

        public ActorListModel(IActorProvider actorProvider, IConfigProvider configProvider, ActorRepository actorRepository) 
            : base(actorProvider)
        {
            _configProvider = configProvider;
            _actorRepository = actorRepository;

            CurrentActor = _actorProvider.GetCurrent();
            ListBuilder = new(new(), _configProvider.ActorSettings);
        }

        [BindProperty]
        public List<Actor> Actors { get; set; } = new();
        [BindProperty]
        public ListBuilder<Actor> ListBuilder { get; set; }

        public async Task<IActionResult> OnGetAsync(ListRequest request)
        {
            if (CurrentActor == null || CurrentActor.IsAdmin == false)
            {
                return Redirect("/Account/Login");
            }

            ListBuilder = new(request, _configProvider.ActorSettings);
            Actors = await ListBuilder.BuildAsync(_actorRepository);

            return Page();
        }
    }
}
