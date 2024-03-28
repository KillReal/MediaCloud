using MediaCloud.Builders.List;
using MediaCloud.Data.Models;
using MediaCloud.WebApp.Services.DataService;
using MediaCloud.WebApp.Services.Statistic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MediaCloud.WebApp.Controllers
{
    [Authorize]
    public class GalleryController : Controller
    {
        private readonly IDataService _dataService;
        private readonly IStatisticService _statisticService;

        public GalleryController(IDataService dataService, IStatisticService statisticService)
        {
            _dataService = dataService;
            _statisticService = statisticService;
        }

        public List<string> GetSuggestions(string searchString, int limit = 10)
        {
            return _dataService.Tags.GetSuggestionsByString(searchString, limit);
        }

        public async Task<List<object>> PreviewsBatchAsync(ListRequest listRequest)
        {
            var previews = await new ListBuilder<Preview>(listRequest).BuildAsync(_dataService.Previews);

            var jsonPreviews = new List<object>();
            foreach (var preview in previews)
            {
                jsonPreviews.Add(new
                {
                    preview.Id,
                    Collection = new
                    {
                        preview.Collection?.Id,
                        preview.Collection?.Count
                    },
                    preview.Content,
                });
            }
                
            _statisticService.ActivityFactorRaised.Invoke();

            return jsonPreviews;
        }

        public FileContentResult Preview(Guid id)
        {
            var preview = _dataService.Previews.Get(id);

            if (preview != null)
            {
                return new FileContentResult(preview.Content, "image/jpeg");
            }

            return new FileContentResult(System.IO.File.ReadAllBytes("wwwroot/noimg.jpg"), "image/jpeg");
        }

        public FileContentResult Source(Guid id)
        {
            var preview = _dataService.Previews.Get(id);

            if (preview != null)
            {
                return new FileContentResult(preview.Media.Content, "image/jpeg");
            }

            return new FileContentResult(System.IO.File.ReadAllBytes("wwwroot/noimg.jpg"), "image/jpeg");
        }
    }
}
