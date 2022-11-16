using Task = MediaCloud.MediaUploader.Tasks.Task;

namespace MediaCloud.MediaUploader
{
    public interface IUploader
    {
        public Guid AddTask(Task task);

        public UploaderStatus GetStatus();

        public UploaderTaskStatus GetStatus(Guid taskId);
    }
}
