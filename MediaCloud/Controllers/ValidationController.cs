using Microsoft.AspNetCore.Mvc;

namespace MediaCloud.WebApp.Controllers;

public class ValidationController : Controller
{
    public IActionResult VerifyTagName([FromQuery(Name = "Tag.Name")]string name)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Contains(' '))
        {
            return Json($"Empty spaces are not allowed in tag name.");
        }

        return Json(true);
    }
}