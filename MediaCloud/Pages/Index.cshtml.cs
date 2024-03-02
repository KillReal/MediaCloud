using MediaCloud.Data;
using MediaCloud.Data.Models;
using MediaCloud.WebApp.Services;
using MediaCloud.WebApp.Services.DataService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MediaCloud.Pages
{
    public class IndexModel : PageModel
    {
        private readonly Actor _currentActor;

        public IndexModel(IDataService dataService)
        {
            _currentActor = dataService.GetCurrentActor() ?? new();
        }

        public IActionResult OnGet()
        {
            if (_currentActor.IsAdmin)
            {
                return Redirect("/Medias");
            }

            return Page();
        }
    }
}