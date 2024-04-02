using MediaCloud.Data;
using MediaCloud.Data.Models;
using MediaCloud.Services;
using MediaCloud.WebApp.Services.ConfigurationProvider;
using MediaCloud.WebApp.Services.DataService;
using MediaCloud.WebApp.Services.Statistic;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NLog;
using ILogger = NLog.ILogger;

namespace MediaCloud.WebApp.Services.DataService.Entities.Base
{
    public class RepositoryContext
    {
        public StatisticProvider StatisticProvider { get; set; }
        public IPictureService PictureService { get; set; }
        public AppDbContext DbContext { get; set; }
        public virtual ILogger Logger { get; set; }
        public Actor? Actor { get; set; }

        public RepositoryContext(AppDbContext dbContext, StatisticProvider statisticProvider, 
            IPictureService pictureService, ILogger logger, Actor? actor)
        {
            StatisticProvider = statisticProvider;
            PictureService = pictureService;
            DbContext = dbContext;
            Logger = logger;
            Actor = actor;
        }
    }
}
