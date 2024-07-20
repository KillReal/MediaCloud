using MediaCloud.Data;
using MediaCloud.Data.Models;
using MediaCloud.WebApp.Services.ActorProvider;
using MediaCloud.WebApp.Services.Statistic;

namespace MediaCloud.TaskScheduler.Tasks
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
