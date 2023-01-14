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

        public List<object> PreviewsBatch(ListRequest listRequest)
        {
            var previews = new ListBuilder<Preview>(listRequest).Build(Repository.Previews);

            var jsonPreviews = new List<object>();
            foreach (var preview in previews)
            {
                jsonPreviews.Add(new
                {
                    Id = preview.Id,
                    Collection = new
                    {
                        Id = preview.Collection?.Id,
                        Count = preview.Collection?.Count
                    },
                    Content = preview.Content,
                });
            }

            return jsonPreviews;
        }

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
