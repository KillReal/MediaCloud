using MediaCloud.Builders.List;
using MediaCloud.Data.Models;
using MediaCloud.WebApp.Services.Repository;
using MediaCloud.WebApp.Services.Statistic;
using Microsoft.AspNetCore.Mvc;

namespace MediaCloud.WebApp.Controllers
{
    public class GalleryController : Controller
    {
        private IRepository Repository { get; set; }
        private IStatisticService StatisticService {get; set;}

        public GalleryController(IRepository repository, IStatisticService statisticService)
        {
            Repository = repository;
            StatisticService = statisticService;
        }

        public List<string> GetSuggestions(string searchString, int limit = 10)
        {
            return Repository.Tags.GetSuggestionsByString(searchString, limit);
        }

        public async Task<List<object>> PreviewsBatchAsync(ListRequest listRequest)
        {
            var previews = await new ListBuilder<Preview>(listRequest).BuildAsync(Repository.Previews);

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
                
            StatisticService.ActivityFactorRaised.Invoke();

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

        public FileContentResult Source(Guid id)
        {
            var preview = Index(id);

            if (preview != null)
                return new FileContentResult(preview.Media.Content, "image/jpeg");

            return new FileContentResult(System.IO.File.ReadAllBytes("wwwroot/noimg.jpg"), "image/jpeg");
        }
    }
}
