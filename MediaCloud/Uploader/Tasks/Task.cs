using MediaCloud.Data;

namespace MediaCloud.MediaUploader.Tasks
{
    public class Task : ITask
    {
        public Guid Id { get; set; }

        public virtual int GetWorkCount()
        {
            throw new NotImplementedException();
        }

        public virtual void DoTheTask()
        {
            throw new NotImplementedException();
        }
    }
}
