using MediaCloud.Data.Models;
using MediaCloud.WebApp.Services.UserProvider;

namespace MediaCloud.TaskScheduler.Tasks
{
    /// <summary>
    /// Abstract Task with unique id and <see cref="User"/>.
    /// </summary>
    /// <remarks>
    /// Task init.
    /// </remarks>
    /// <param name="user"> Customer of task. </param>
    public class Task(User user) : ITask
    {
        /// <summary>
        /// Unique identificator.
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Customer.
        /// </summary>
        public User User { get; set; } = user;

        /// <summary>
        /// Wether the task executed.
        /// </summary>
        public bool IsExecuted { get; set; }

        /// <summary>
        /// Wether the task was completed.
        /// </summary>
        public bool IsCompleted {get; set;}

        /// <summary>
        /// Completion message.
        /// </summary>
        public string CompletionMessage {get; set;}

        /// <summary>
        /// When task was executed.
        /// </summary>
        public DateTime ExecutedAt {get; set;}

        /// <summary>
        /// When task was executed.
        /// </summary>
        public DateTime CompletedAt {get; set;}

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
        public virtual void DoTheTask(IServiceProvider serviceProvider, IUserProvider actorProvider)
        {
            throw new NotImplementedException();
        }

        public User GetAuthor()
        {
            return User;
        }
    }
}
