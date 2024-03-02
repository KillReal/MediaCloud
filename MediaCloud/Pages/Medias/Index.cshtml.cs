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
using MediaCloud.WebApp.Services.DataService;

namespace MediaCloud.Pages.Medias
{
    [Authorize]
    public class ListModel : PageModel
    {
        private readonly IDataService _dataService;

        [BindProperty]
        public List<Preview> Previews { get; set; } = new();

        [BindProperty]
        public ListBuilder<Preview>? ListBuilder { get; set; }

        [BindProperty]
        public string ExampleFilter { get; set; }

        [BindProperty]
        public bool IsAutoloadEnabled { get; set; } = true;

        public ListModel(IDataService dataService)
        {
            _dataService = dataService;

            var topTagNames = _dataService.Tags.GetTopUsed(2).Select(x => x.Name.ToLower());
            if (topTagNames.Count() > 1)
            {
                ExampleFilter = $"{topTagNames.First()} !{topTagNames.Last()}";
            }
            else
            {
                ExampleFilter = "Create more tags to filtering";
            }
        }

        public async Task<IActionResult> OnGetAsync(ListRequest request)
        {
            ListBuilder = new ListBuilder<Preview>(request);
            Previews = await ListBuilder.BuildAsync(_dataService.Previews);
            IsAutoloadEnabled = request.IsUseAutoload;

            return Page();
        }  
    }
}
