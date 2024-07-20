using MediaCloud.TaskScheduler.Tasks;
using Task = MediaCloud.TaskScheduler.Tasks.Task;

namespace MediaCloud.TaskScheduler
{
    /// <summary>
    /// Worker which do task processing.
    /// </summary>
    public class Worker
    {
        private readonly List<Type> _taskTypes;
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
        /// <param name="taskTypes">The types of tasks that this worker is able to execute.</param>
        public Worker(Queue queue, TaskScheduler scheduler, IServiceScopeFactory serviceScopeFactory, List<Type> taskTypes)
        {
            _queue = queue;
            _scheduler = scheduler;
            _serviceScopeFactory = serviceScopeFactory;
            _taskTypes = taskTypes;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Worker"/> class.
        /// </summary>
        /// <param name="queue">The queue from which tasks will be dequeued.</param>
        /// <param name="scheduler">The task scheduler responsible for managing the execution of tasks.</param>
        /// <param name="serviceScopeFactory">The service scope factory used to create scopes for task execution.</param>
        /// <param name="taskType">The types of tasks that this worker is able to execute.</param>
        public Worker(Queue queue, TaskScheduler scheduler, IServiceScopeFactory serviceScopeFactory, Type taskType)
        {
            _queue = queue;
            _scheduler = scheduler;
            _serviceScopeFactory = serviceScopeFactory;
            _taskTypes = new() { taskType };
        }

        /// <summary>
        /// Worker take first <see cref="ITask"/> from <see cref="Queue"/> and begin it's processing in new <see cref="Thread"/>.
        /// </summary>
        public void Run()
        {
            Task = _queue.GetNextTask(_taskTypes);

            if (Task == null)
            {
                return;
            }
            
            ThreadPool.QueueUserWorkItem(WorkRoutine);
        }

        /// <summary>
        /// Checks if the worker is able to execute the given task type.
        /// </summary>
        /// <param name="taskType"> The type of task to check. </param>
        /// <returns> True if the worker can execute the task type, otherwise false. </returns>
        public bool IsAbleToExecute(Type taskType)
        {
            return _taskTypes.Any(x => x == taskType);
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
