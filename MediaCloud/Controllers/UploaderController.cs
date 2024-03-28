using MediaCloud.MediaUploader;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MediaCloud.WebApp.Controllers
{
    [Authorize]
    public class UploaderController : Controller
    {
        private IUploader _uploader;

        public IActionResult Index()
        {
            return View();
        }

        public UploaderController(IUploader uploader)
        {
            _uploader = uploader;
        }

        public MediaUploader.TaskStatus GetTaskStatus(Guid id)
        {
            return _uploader.GetStatus(id);
        }

        public UploaderStatus GetStatus()
        {
            return _uploader.GetStatus();
        }
    }
}
