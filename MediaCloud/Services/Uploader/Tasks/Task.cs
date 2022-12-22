using MediaCloud.Data;
using MediaCloud.Data.Models;
using MediaCloud.WebApp.Services.Repository;

namespace MediaCloud.MediaUploader.Tasks
{
    public class Task : ITask
    {
        public Guid Id { get; set; }

        public Actor Actor { get; set; }

        public Task(Actor actor)
        {
            Actor = actor;
        }

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
