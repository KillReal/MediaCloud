using MediaCloud.Data.Models;
using MediaCloud.WebApp.Services.ActorProvider;

namespace MediaCloud.TaskScheduler.Tasks
{
    /// <summary>
    /// Abstract Task with unique id and <see cref="Actor"/>.
    /// </summary>
    /// <remarks>
    /// Task init.
    /// </remarks>
    /// <param name="actor"> Customer of task. </param>
    public class Task(Actor actor) : ITask
    {
        /// <summary>
        /// Unique identificator.
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Customer.
        /// </summary>
        public Actor Actor { get; set; } = actor;

        /// <summary>
        /// Wether the task can be executed.
        /// </summary>
        public bool IsWaiting { get; set; } = true;

        public DateTime ExecutedAt {get; set;}

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
