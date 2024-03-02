using MediaCloud.Data;
using MediaCloud.Repositories;
using MediaCloud.WebApp.Services.DataService;
using Microsoft.Extensions.DependencyInjection;
using ITask = MediaCloud.MediaUploader.Tasks.Task;

namespace MediaCloud.MediaUploader
{
    /// <summary>
    /// Content uploading service
    /// </summary>
    public class Uploader : IUploader
    {
        private readonly IServiceScope ServiceScope;
        private readonly Queue Queue;
        private readonly Scheduler Scheduler;

        /// <summary>
        /// Initialize service
        /// </summary>
        /// <param name="scopeFactory"> Scope factory for creation own scope </param>
        public Uploader(IServiceScopeFactory scopeFactory) 
        {
            ServiceScope = scopeFactory.CreateScope();
            var DataService = ServiceScope.ServiceProvider.GetRequiredService<IDataService>();
            Queue = new Queue();
            Scheduler = new Scheduler(DataService, Queue);
        }

        /// <summary>
        /// Add task to <see cref="Queue"/> and run <see cref="Scheduler"/> for start task processing.
        /// </summary>
        /// <param name="task"> Task instance <see cref="ITask"/></param>
        /// <returns> Id of task. </returns>
        public Guid AddTask(ITask task)
        {
            Queue.AddTask(task);
            Scheduler.Run();

            return task.Id;
        }

        /// <summary>
        /// Collect info about uploader status.
        /// </summary>
        /// <returns> <see cref="UploaderStatus"/> with state of internal queue and scheduler. </returns>
        public UploaderStatus GetStatus() => new(Queue, Scheduler);

        /// <summary>
        /// Collect info about certain task.
        /// </summary>
        /// <param name="taskId"> Task id. </param>
        /// <returns> <see cref="TaskStatus"/> with task state. </returns>
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