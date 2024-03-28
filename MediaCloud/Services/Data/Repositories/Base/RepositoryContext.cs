using MediaCloud.Data;
using MediaCloud.Data.Models;
using MediaCloud.WebApp.Services.DataService;
using MediaCloud.WebApp.Services.Statistic;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NLog;
using ILogger = NLog.ILogger;

namespace MediaCloud.WebApp.Services.DataService.Entities.Base
{
    public class RepositoryContext
    {
        public IStatisticService StatisticService { get; set; }
        public AppDbContext DbContext { get; set; }
        public virtual ILogger Logger { get; set; }
        public Actor? Actor { get; set; }

        public RepositoryContext(AppDbContext dbContext, IStatisticService statisticService, ILogger logger, Actor? actor)
        {
            StatisticService = statisticService;
            DbContext = dbContext;
            Logger = logger;
            Actor = actor;
        }
    }
}
