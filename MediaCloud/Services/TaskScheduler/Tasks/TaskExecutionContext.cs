using MediaCloud.Repositories;
using MediaCloud.WebApp.Services.ActorProvider;

namespace MediaCloud.TaskScheduler.Tasks
{
    public class TaskExecutionContext
    {
        private ITask _task;

        public TaskExecutionContext(ITask task)
        {
            _task = task;
        }
        
        public virtual void DoTheTask(IServiceProvider serviceProvider)
        {
            var actorRepository = serviceProvider.GetRequiredService<ActorRepository>();

            _task.DoTheTask(serviceProvider, new DummyActorProvider(_task.GetAuthor(), actorRepository));
        }
    }
}
