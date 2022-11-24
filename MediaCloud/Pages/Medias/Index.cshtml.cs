using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using MediaCloud.Data;
using MediaCloud.Builders.List;
using MediaCloud.Data.Models;
using MediaCloud.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Security.Claims;
using MediaCloud.WebApp.Services;

namespace MediaCloud.Pages.Medias
{
    [Authorize]
    public class ListModel : PageModel
    {
        private IRepository _repository;

        [BindProperty]
        public List<Preview> Previews { get; set; }

        [BindProperty]
        public ListBuilder<Preview> ListBuilder { get; set; }

        [BindProperty]
        public string ExampleFilter { get; set; }

        public ListModel(IRepository repository)
        {
            _repository = repository;
        }

        public IActionResult OnGet(ListRequest request)
        {
            ListBuilder = new ListBuilder<Preview>(request);
            Previews = ListBuilder.Build(_repository.Previews);

            var topTagNames = _repository.Tags.GetTopUsed(2).Select(x => x.Name.ToLower());
            ExampleFilter = $"{topTagNames.First()} !{topTagNames.Last()}";

            return Page();
        }  
    }
}
