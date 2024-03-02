using MediaCloud.Data;
using MediaCloud.Repositories;
using MediaCloud.WebApp.Services.DataService;
using Microsoft.Extensions.DependencyInjection;
using Task = MediaCloud.MediaUploader.Tasks.Task;

namespace MediaCloud.MediaUploader
{
    public class Uploader : IUploader
    {
        private IServiceScope ServiceScope;
        private Queue Queue;
        private Scheduler Scheduler;

        public Uploader(IServiceScopeFactory scopeFactory, ILogger<Uploader> logger) 
        {
            ServiceScope = scopeFactory.CreateScope();
            var DataService = ServiceScope.ServiceProvider.GetRequiredService<IDataService>();
            Queue = new Queue();
            Scheduler = new Scheduler(DataService, logger, Queue);
        }

        public Guid AddTask(Task task)
        {
            Queue.AddTask(task);
            Scheduler.Run();

            return task.Id;
        }

        public UploaderStatus GetStatus() => new(Queue, Scheduler);

        public TaskStatus GetStatus(Guid taskId) 
        {
            var tastStatus = new TaskStatus
            {
                Id = taskId,
                IsInProgress = Scheduler.IsTaskInProgress(taskId),
                QueuePosition = Queue.GetTaskPosition(taskId)
            };
            var task = Queue.GetTask(taskId);
            tastStatus.IsExist = task != null;
            tastStatus.WorkCount = task == null 
                ? 0 
                : task.GetWorkCount();

            return tastStatus;
        }
    }
}