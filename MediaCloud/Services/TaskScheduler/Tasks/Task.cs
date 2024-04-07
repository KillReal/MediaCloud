using MediaCloud.Data;
using MediaCloud.Data.Models;
using MediaCloud.WebApp.Services.ActorProvider;

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
            Id = Guid.NewGuid();
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
        /// Do the task.
        /// </summary>
        /// <param name="serviceProvider"> Used for creation of needed repositories. </param>
        /// <param name="actorProvider"> Used to set the current actor for repositories. </param>
        /// <exception cref="NotImplementedException"></exception>
        public virtual void DoTheTask(IServiceProvider serviceProvider, IActorProvider actorProvider)
        {
            throw new NotImplementedException();
        }

        public Actor GetAuthor()
        {
            return Actor;
        }
    }
}
