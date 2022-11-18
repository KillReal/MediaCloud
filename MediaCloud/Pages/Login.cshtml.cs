using MediaCloud.Data;
using MediaCloud.Data.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace MediaCloud.WebApp.Pages
{
    public class LoginModel : PageModel
    {
        private AppDbContext _context;

        [BindProperty]
        public Actor Actor { get; set; }

        [BindProperty]
        public string ReturnUrl { get; set; }

        public LoginModel(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult OnGet(string returnUrl = "/")
        {
            Actor = new();
            ReturnUrl = returnUrl;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var actor = _context.Actors.FirstOrDefault(x => x.Name == Actor.Name && x.PasswordHash == Actor.PasswordHash);

            if (actor == null)
            {
                return Unauthorized();
            }

            var claims = new List<Claim> { new Claim(ClaimTypes.Name, Actor.Name) };
            ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, "Cookies");
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

            return Redirect(ReturnUrl);
        }
    }
}
