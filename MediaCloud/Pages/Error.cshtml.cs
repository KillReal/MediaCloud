using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Diagnostics;

namespace MediaCloud.Pages
{
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    [IgnoreAntiforgeryToken]
    public class ErrorModel : PageModel
    {
        public string? RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

        private readonly ILogger<ErrorModel> _logger;

        public ErrorModel(ILogger<ErrorModel> logger)
        {
            _logger = logger;
        }

        public void OnGet(string message = "Unknown error")
        {
            _logger.LogInformation("Error occured during request: {HttpContext.Request.Path} by {HttpContext.User.Identity.Name} message: {message}", 
                HttpContext.Request.Path, HttpContext.User.Identity?.Name, message);
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        }
    }
}