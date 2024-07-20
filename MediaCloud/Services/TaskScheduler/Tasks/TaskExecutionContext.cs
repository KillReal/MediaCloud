using MediaCloud.Repositories;
using MediaCloud.WebApp.Services.ActorProvider;

namespace MediaCloud.TaskScheduler.Tasks
{
    public class TaskExecutionContext(ITask task)
    {
        private ITask _task = task;

        public virtual void DoTheTask(IServiceProvider serviceProvider)
        {
            var actorRepository = serviceProvider.GetRequiredService<ActorRepository>();

            _task.DoTheTask(serviceProvider, new DummyActorProvider(_task.GetAuthor(), actorRepository));
        }
    }
}
