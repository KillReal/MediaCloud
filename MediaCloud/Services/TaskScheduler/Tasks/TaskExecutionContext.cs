using MediaCloud.Repositories;
using MediaCloud.WebApp.Services.UserProvider;

namespace MediaCloud.TaskScheduler.Tasks
{
    public class TaskExecutionContext(ITask task)
    {
        private ITask _task = task;

        public virtual void DoTheTask(IServiceProvider serviceProvider)
        {
            var actorRepository = serviceProvider.GetRequiredService<UserRepository>();

            _task.DoTheTask(serviceProvider, new DummyUserProvider(_task.GetAuthor(), actorRepository));
        }
    }
}
