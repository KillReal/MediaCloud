using MediaCloud.TaskScheduler.Tasks;
using Task = MediaCloud.TaskScheduler.Tasks.Task;

namespace MediaCloud.TaskScheduler
{
    /// <summary>
    /// Worker which do task processing.
    /// </summary>
    public class Worker
    {
        private readonly Queue _queue;
        private readonly TaskScheduler _scheduler;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        /// <summary>
        /// Current task. Return's null if no task currently taken.
        /// </summary>
        public Task? Task;

        /// <summary>
        /// State of Worker that it ready to take next <see cref="ITask"/>.
        /// </summary>
        public bool IsReady { get; private set; } = true;

        /// <summary>
        /// Initializes a new instance of the <see cref="Worker"/> class.
        /// </summary>
        /// <param name="queue">The queue from which tasks will be dequeued.</param>
        /// <param name="scheduler">The task scheduler responsible for managing the execution of tasks.</param>
        /// <param name="serviceScopeFactory">The service scope factory used to create scopes for task execution.</param>
        public Worker(Queue queue, TaskScheduler scheduler, IServiceScopeFactory serviceScopeFactory)
        {
            _queue = queue;
            _scheduler = scheduler;
            _serviceScopeFactory = serviceScopeFactory;
        }

        /// <summary>
        /// Worker take first <see cref="ITask"/> from <see cref="Queue"/> and begin it's processing in new <see cref="Thread"/>.
        /// </summary>
        public void Run()
        {
            Task = _queue.GetNextTask();

            if (Task == null)
            {
                return;
            }
            
            ThreadPool.QueueUserWorkItem(WorkRoutine);
        }

        private void WorkRoutine(object? state)
        {
            IsReady = false;

            if (Task == null)
            {
                return;
            }

            _scheduler.OnTaskStarted.Invoke(Task);

            try
            {
                var taskContext = new TaskExecutionContext(Task);

                taskContext.DoTheTask(_serviceScopeFactory.CreateScope().ServiceProvider);
            }
            catch (Exception ex)
            {
                _scheduler.OnTaskErrorOccured.Invoke(Task, ex);
            }

            IsReady = true;

            _scheduler.OnTaskCompleted.Invoke(Task);
        }
    }
}
