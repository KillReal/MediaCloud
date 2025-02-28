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
            var userRepository = serviceProvider.GetRequiredService<UserRepository>();
            var userProvider = new DummyUserProvider(_task.GetAuthor(), userRepository);
            var statisticProvider = ActivatorUtilities.CreateInstance<StatisticProvider>(serviceProvider, userProvider);
            
            _task.DoTheTask(serviceProvider, userProvider, statisticProvider);
        }
    }
}
