using MediaCloud.Data;
using MediaCloud.Data.Models;
using MediaCloud.WebApp.Pages;
using MediaCloud.WebApp.Services;
using MediaCloud.WebApp.Services.ActorProvider;
using MediaCloud.WebApp.Services.DataService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MediaCloud.Pages
{
    public class IndexModel : PageModel
    {
        public IndexModel()
        { 
        }

        public IActionResult OnGet()
        {
            return Page();
        }
    }
}