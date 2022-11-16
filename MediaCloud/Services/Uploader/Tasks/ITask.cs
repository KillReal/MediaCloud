using MediaCloud.Data;

namespace MediaCloud.MediaUploader.Tasks
{
    public interface ITask
    {
        public int GetWorkCount();

        public void DoTheTask();
    }
}
