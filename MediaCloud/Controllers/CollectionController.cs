using MediaCloud.Builders.List;
using MediaCloud.Data.Models;
using MediaCloud.WebApp.Services.Repository;
using MediaCloud.WebApp.Services.Statistic;
using Microsoft.AspNetCore.Mvc;

namespace MediaCloud.WebApp.Controllers
{
    public class CollectionController : Controller
    {
        private IRepository Repository { get; set; }
        private IStatisticService StatisticService { get; set; }

        public CollectionController(IRepository repository, IStatisticService statisticService)
        {
            Repository = repository;
            StatisticService = statisticService;
        }

        public List<object> PreviewsBatch(Guid id, ListRequest listRequest)
        {
            var previews = Repository.Collections.GetList(id, listRequest);

            var jsonPreviews = new List<object>();
            foreach (var preview in previews)
            {
                jsonPreviews.Add(new
                {
                    Id = preview.Id,
                    Content = preview.Content,
                    Order = preview.Order,
                });
            }

            StatisticService.NotifyActivityFactorRaised();

            return jsonPreviews;
        }
    }
}
