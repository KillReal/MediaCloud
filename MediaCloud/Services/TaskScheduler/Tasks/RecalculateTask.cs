using MediaCloud.Data;
using MediaCloud.Data.Models;
using MediaCloud.WebApp.Services.UserProvider;
using MediaCloud.WebApp.Services.Statistic;
using Microsoft.Extensions.Caching.Memory;

namespace MediaCloud.TaskScheduler.Tasks
{
    public class RecalculateTask(User actor, DateTime startDate) : Task(actor), ITask
    {
        private readonly DateTime _startDate = startDate;
        private int _workCount;

        public override int GetWorkCount() => _workCount;

        public override void DoTheTask(IServiceProvider serviceProvider, IUserProvider userProvider, StatisticProvider statisticProvider)
        {
            statisticProvider.Recalculate(_startDate, ref _workCount);
        }
    }
}
