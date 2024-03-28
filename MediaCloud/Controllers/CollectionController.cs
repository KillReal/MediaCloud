using MediaCloud.Builders.List;
using MediaCloud.Data.Models;
using MediaCloud.WebApp.Services.DataService;
using MediaCloud.WebApp.Services.Statistic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MediaCloud.WebApp.Controllers
{
    [Authorize]
    public class CollectionController : Controller
    {
        private readonly IDataService _dataService;

        public CollectionController(IDataService dataService)
        {
            _dataService = dataService;
        }

        public List<object> PreviewsBatch(Guid id, ListRequest listRequest)
        {
            var previews = _dataService.Collections.GetList(id, listRequest);

            var jsonPreviews = new List<object>();
            foreach (var preview in previews)
            {
                jsonPreviews.Add(new
                {
                    preview.Id,
                    preview.Content,
                    preview.Order,
                });
            }

            return jsonPreviews;
        }
    }
}
