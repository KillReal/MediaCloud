using MediaCloud.Repositories;
using MediaCloud.WebApp.Services.Statistic;
using MediaCloud.WebApp.Services.UserProvider;

namespace MediaCloud.TaskScheduler.Tasks
{
    public class TaskExecutionContext(ITask task)
    {
        private readonly ITask _task = task;

        public virtual void DoTheTask(IServiceProvider serviceProvider)
        {
            var actorRepository = serviceProvider.GetRequiredService<UserRepository>();
            var userProvider = new DummyUserProvider(_task.GetAuthor(), actorRepository);
            var statisticProvider = ActivatorUtilities.CreateInstance<StatisticProvider>(serviceProvider, userProvider);
            
            _task.DoTheTask(serviceProvider, userProvider, statisticProvider);
        }
    }
}
