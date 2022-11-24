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
using MediaCloud.Repositories;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using MediaCloud.WebApp.Services;

namespace MediaCloud.Pages.Actors
{
    [Authorize]
    public class ListModel : PageModel
    {
        private Actor Actor;
        private ActorRepository ActorRepository;

        public ListModel(AppDbContext context, ILogger<ListModel> logger, 
            IActorProvider actorProvider)
        {
            Actor = actorProvider.GetCurrent() ?? new();

            ActorRepository = new(context);
        }

        [BindProperty]
        public List<Actor> Actors { get; set; }
        [BindProperty]
        public ListBuilder<Actor> ListBuilder { get; set; }

        public IActionResult OnGet(ListRequest request)
        {
            if (Actor.IsAdmin == false)
            {
                return Redirect("/Account/Login");
            }

            ListBuilder = new(request);
            Actors = ListBuilder.Build(ActorRepository);

            return Page();
        }
    }
}
