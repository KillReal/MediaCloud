using MediaCloud.Data;
using MediaCloud.Data.Models;
using MediaCloud.WebApp.Services.DataService;

namespace MediaCloud.MediaUploader.Tasks
{
    /// <summary>
    /// Abstract Task with unique id and <see cref="Actor"/>.
    /// </summary>
    public class Task : ITask
    {
        /// <summary>
        /// Unique identificator.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Customer.
        /// </summary>
        public Actor Actor { get; set; }

        /// <summary>
        /// Task init.
        /// </summary>
        /// <param name="actor"> Customer of task. </param>
        public Task(Actor actor)
        {
            Actor = actor;
        }

        /// <summary>
        /// Check work count to process.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public virtual int GetWorkCount()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Execute task.
        /// </summary>
        /// <param name="DataService"> Current <see cref="IDataService"/> to work with. </param>
        /// <exception cref="NotImplementedException"> Exception because it's abstract parent instance. </exception>
        public virtual void DoTheTask(IDataService DataService)
        {
            throw new NotImplementedException();
        }
    }
}
