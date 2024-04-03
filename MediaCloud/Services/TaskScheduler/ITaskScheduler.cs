using Task = MediaCloud.MediaUploader.Tasks.Task;

namespace MediaCloud.MediaUploader
{
    public interface ITaskScheduler
    {
        public Guid AddTask(Task task);

        public TaskSchedulerStatus GetStatus();

        public TaskStatus GetStatus(Guid taskId);
    }
}
