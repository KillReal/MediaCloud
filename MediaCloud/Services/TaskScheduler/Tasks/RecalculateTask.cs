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

        public override void DoTheTask(IServiceProvider serviceProvider, IUserProvider actorProvider)
        {
            var context = serviceProvider.GetRequiredService<AppDbContext>();
            var memoryCache = serviceProvider.GetRequiredService<IMemoryCache>();
            var statisticProvider = new StatisticProvider(context, actorProvider, memoryCache);
            statisticProvider.Recalculate(_startDate, ref _workCount);
        }
    }
}
