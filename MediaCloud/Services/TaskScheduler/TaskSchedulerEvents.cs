using Task = MediaCloud.TaskScheduler.Tasks.Task;

namespace MediaCloud.TaskScheduler
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
            task.IsWaiting = false;

            _logger.Info("Worker ({BusyWorkersCount}/{MaxWorkersCount}) processing the {task.GetType().Name}: {task.Id} author: {task.Actor.Name}",
                BusyWorkersCount, MaxWorkersCount, task.GetType().Name, task.Id, task.Actor.Name);
        }

        private void WorkerCompleteTask(Task task)
        {
            _logger.Info("Worker ({BusyWorkersCount - 1}/{MaxWorkersCount}) completed the {task.GetType().Name}: {task.Id} author: {task.Actor.Name}",
                BusyWorkersCount, MaxWorkersCount, task.GetType().Name, task.Id, task.Actor.Name);

            _queue.OnTaskComplete.Invoke(task);
            Run();
        }

        private void WorkerFacedErrorWithTask(Task task, Exception ex)
        {
            _logger.Error("Worker faced error during processing of {task.GetType().Name}: {task.Id} author: {task.Actor.Name} exception: {ex}", 
                task.GetType().Name, task.Id, task.Actor.Name, ex);
            
            _queue.OnTaskComplete.Invoke(task);
            Run();
        }
    }
}
