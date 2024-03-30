using MediaCloud.Data;
using MediaCloud.Data.Models;
using MediaCloud.Extensions;
using MediaCloud.Repositories;
using MediaCloud.WebApp.Services.DataService;
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

        public override void DoTheTask(IDataService dataService)
        {
            dataService.StatisticProvider.Recalculate(_startDate, ref _workCount);
        }
    }
}
