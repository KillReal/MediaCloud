using MediaCloud.Builders.List;
using MediaCloud.Data.Models;
using MediaCloud.WebApp.Services.Repository;
using Microsoft.AspNetCore.Mvc;

namespace MediaCloud.WebApp.Controllers
{
    public class GalleryController : Controller
    {
        private IRepository Repository { get; set; }

        public GalleryController(IRepository repository)
        {
            Repository = repository;
        }

        public List<Preview> Index(ListRequest listRequest)
            => new ListBuilder<Preview>(listRequest).Build(Repository.Previews);

        public Preview? Index(Guid id)
            => Repository.Previews.Get(id);

        public FileContentResult Preview(Guid id)
        {
            var preview = Index(id);

            if (preview != null)
                return new FileContentResult(preview.Content, "image/jpeg");

            return new FileContentResult(System.IO.File.ReadAllBytes("wwwroot/noimg.jpg"), "image/jpeg");
        }
    }
}
