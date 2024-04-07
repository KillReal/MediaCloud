using MediaCloud.Data;
using MediaCloud.MediaUploader.Tasks;
using MediaCloud.Repositories;
using MediaCloud.WebApp.Services;
using MediaCloud.WebApp.Services.ConfigurationProvider;
using NLog;
using ILogger = NLog.ILogger;
using Task = MediaCloud.MediaUploader.Tasks.Task;

namespace MediaCloud.MediaUploader
{
    /// <summary>
    /// Internal task scheduler.
    /// </summary>
    public partial class TaskScheduler : ITaskScheduler
    {
        /// <summary>
        /// Logging task start event.
        /// </summary>
        public Action<Task> OnTaskStarted { get; set; }
        /// <summary>
        /// Logging task completion event. 
        /// Notify <see cref="Queue"/> about task completion and if <see cref="Queue"/> is not empty, 
        /// tries to run free <see cref="Worker"/>.
        /// </summary>
        public Action<Task> OnTaskCompleted { get; set; }
        /// <summary>
        /// Task error occured event.
        /// </summary>
        public Action<Task, Exception> OnTaskErrorOccured { get; set; }

        private void WorkerStartTask(Task task)
        {
            _logger.Info("Worker ({0}/{1}) processing the task: {2} author: {3}",
                BusyWorkersCount, MaxWorkersCount, task.Id, task.Actor.Name);
        }

        private void WorkerCompleteTask(Task task)
        {
            _logger.Info("Worker ({0}/{1}) completed the task: {2} author: {3}",
                BusyWorkersCount - 1, MaxWorkersCount, task.Id, task.Actor.Name);

            _queue.OnTaskComplete.Invoke(task);
            if (_queue.IsEmpty == false)
            {
                Run();
            }
        }

        private void WorkerFacedErrorWithTask(Task task, Exception ex)
        {
            _logger.Error("Worker faced error during processing of task: {0} author: {1} exception: {3}", 
                task.Id, task.Actor.Name, ex);
        }
    }
}
