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
using MediaCloud.WebApp.Services.DataService;
using MediaCloud.WebApp.Pages;

namespace MediaCloud.Pages.Actors
{
    [Authorize]
    public class ActorListModel : BasePageModel
    {
        private readonly Actor _actor;

        public ActorListModel(IDataService dataService) : base(dataService)
        {
            _actor = _dataService.GetCurrentActor();
            ListBuilder = new(new());
        }

        [BindProperty]
        public List<Actor> Actors { get; set; } = new();
        [BindProperty]
        public ListBuilder<Actor> ListBuilder { get; set; }

        public async Task<IActionResult> OnGetAsync(ListRequest request)
        {
            if (_actor == null || _actor.IsAdmin == false)
            {
                return Redirect("/Account/Login");
            }

            ListBuilder = new(request);
            Actors = await ListBuilder.BuildAsync(_dataService.Actors);

            return Page();
        }
    }
}
