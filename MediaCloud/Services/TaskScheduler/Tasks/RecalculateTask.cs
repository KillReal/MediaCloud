using MediaCloud.Data;
using MediaCloud.Data.Models;
using MediaCloud.Extensions;
using MediaCloud.Repositories;
using MediaCloud.WebApp.Services.ActorProvider;
using MediaCloud.WebApp.Services.Statistic;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using ILogger = NLog.ILogger;

namespace MediaCloud.MediaUploader.Tasks
{
    public class RecalculateTask : Task, ITask
    {
        private readonly DateTime _startDate;
        private int _workCount;

        public override int GetWorkCount() => _workCount;

        public RecalculateTask(Actor actor, DateTime startDate) 
            : base(actor)
        {
            _startDate = startDate;
        }

        public override void DoTheTask(IServiceProvider serviceProvider, IActorProvider actorProvider)
        {
            var context = serviceProvider.GetRequiredService<AppDbContext>();

            var statisticProvider = new StatisticProvider(context, actorProvider);
            statisticProvider.Recalculate(_startDate, ref _workCount);
        }
    }
}
