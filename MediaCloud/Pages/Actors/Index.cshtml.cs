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
using MediaCloud.WebApp.Services.Repository;

namespace MediaCloud.Pages.Actors
{
    [Authorize]
    public class ListModel : PageModel
    {
        private Actor Actor;
        private IRepository Repository;

        public ListModel(IRepository repository)
        {
            Repository = repository;
        }

        [BindProperty]
        public List<Actor> Actors { get; set; }
        [BindProperty]
        public ListBuilder<Actor> ListBuilder { get; set; }

        public async Task<IActionResult> OnGetAsync(ListRequest request)
        {
            Actor = Repository.GetCurrentActor();

            if (Actor == null || Actor.IsAdmin == false)
            {
                return Redirect("/Account/Login");
            }

            ListBuilder = new(request);
            Actors = await ListBuilder.BuildAsync(Repository.Actors);

            return Page();
        }
    }
}
