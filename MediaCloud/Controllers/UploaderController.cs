using MediaCloud.MediaUploader;
using Microsoft.AspNetCore.Mvc;

namespace MediaCloud.WebApp.Controllers
{
    public class UploaderController : Controller
    {
        private IUploader Uploader;

        public IActionResult Index()
        {
            return View();
        }

        public UploaderController(IUploader uploader)
        {
            Uploader = uploader;
        }

        public UploaderTaskStatus GetTaskStatus(Guid id)
        {
            return Uploader.GetStatus(id);
        }

        public UploaderStatus GetStatus()
        {
            return Uploader.GetStatus();
        }
    }
}
