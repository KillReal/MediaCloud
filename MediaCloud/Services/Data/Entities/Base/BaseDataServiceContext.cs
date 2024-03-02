using MediaCloud.Data;
using MediaCloud.Data.Models;
using MediaCloud.WebApp.Services.DataService;
using MediaCloud.WebApp.Services.Statistic;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MediaCloud.WebApp.Services.DataService.Entities.Base
{
    public class DataServiceContext
    {
        public IStatisticService StatisticService { get; set; }
        public AppDbContext DbContext { get; set; }
        public virtual ILogger Logger { get; set; }
        public Actor? Actor { get; set; }

        public DataServiceContext(AppDbContext dbContext, IStatisticService statisticService, ILogger<DataService> logger, Actor? actor)
        {
            StatisticService = statisticService;
            DbContext = dbContext;
            Logger = logger;
            Actor = actor;
        }
    }
}
