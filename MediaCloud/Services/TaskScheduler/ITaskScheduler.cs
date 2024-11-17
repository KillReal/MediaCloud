using Task = MediaCloud.TaskScheduler.Tasks.Task;

namespace MediaCloud.TaskScheduler
{
    public interface ITaskScheduler
    {
        public Guid AddTask(Task task);

        public TaskSchedulerStatus GetStatus();

        public TaskStatus GetStatus(Guid taskId);

        public void CleanupQueue(bool cleanupOnlyCompleted = true);
    }
}
