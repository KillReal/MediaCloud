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

namespace MediaCloud.Pages.Actors
{
    [Authorize]
    public class ListModel : PageModel
    {
        private readonly Actor _actor;
        private readonly IDataService _dataService;

        public ListModel(IDataService dataService)
        {
            _dataService = dataService;
            _actor = _dataService.GetCurrentActor();
        }

        [BindProperty]
        public List<Actor>? Actors { get; set; }
        [BindProperty]
        public ListBuilder<Actor>? ListBuilder { get; set; }

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
