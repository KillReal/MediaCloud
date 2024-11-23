using MediaCloud.WebApp.Pages;
using MediaCloud.WebApp.Services.UserProvider;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SixLabors.ImageSharp.PixelFormats;
using System.Diagnostics;

namespace MediaCloud.Pages
{
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    [IgnoreAntiforgeryToken]
    public class ErrorModel(IUserProvider userProvider) : AuthorizedPageModel(userProvider)
    {
        public string? RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

        public string ErrorMessage { get; set; }

        public void OnGet(string message = "Unknown error")
        {
            ErrorMessage = message;
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        }
    }
}