using Task = MediaCloud.TaskScheduler.Tasks.Task;

namespace MediaCloud.TaskScheduler
{
    public class Queue
    {
        private int _completedTaskLifetimeMin;
        private readonly List<Task> _tasks = [];

        private void RemoveOldCompletedTasks()
        {
            var completedTasks = _tasks.Where(x => x.IsCompleted 
                && DateTime.Now - x.CompletedAt > new TimeSpan(0, _completedTaskLifetimeMin, 0));

            foreach (var task in completedTasks)
            {
                RemoveTask(task);
            }
        }

        private void RemoveTask(Task task) => _tasks.Remove(task);

        public Action<Task, string?> OnTaskComplete;

        public Queue() 
        {
            OnTaskComplete += CompleteTask;

            //TODO: move to EnvironmentSettings
            _completedTaskLifetimeMin = 30;
        }

        public bool IsEmpty => _tasks.Count == 0;

        public int TaskCount => _tasks.Count;

        public int WorkCount => _tasks.Sum(x => x.GetWorkCount());

        public void AddTask(Task task)
        { 
            _tasks.Add(task);

            RemoveOldCompletedTasks();
        }

        public void CompleteTask(Task task, string? message) 
        {
            task.IsCompleted = true;
            task.CompletedAt = DateTime.Now;

            if (message != null)
            {
                task.CompletionMessage = message;
            }
        }

        public Task? GetNextTask() => _tasks.Where(x => x.IsCompleted == false && x.IsExecuted == false).FirstOrDefault();

        public Task? GetTask(Guid id) => _tasks.FirstOrDefault(x => x.Id == id);

        public int GetTaskPosition(Guid id)
        {
            var task = GetTask(id);

            if (task == null)
            {
                return -1;
            }

            return _tasks.Where(x => x.IsCompleted == false).ToList().IndexOf(task) + 1;
        }

        public TaskStatus GetTaskStatus(Guid taskId) 
        {
             var taskStatus = new TaskStatus
            {
                Id = taskId,
                QueuePosition = GetTaskPosition(taskId)
            };
            var task = GetTask(taskId);

            if (task == null)
            {
                return new();
            }


            //TODO: Replace with Task itself.
            taskStatus.IsInProgress = task.IsExecuted;
            taskStatus.IsCompleted = task.IsCompleted;
            taskStatus.CompletionMessage = task.CompletionMessage;
            taskStatus.WorkCount = task.GetWorkCount();
            taskStatus.ExecutedAt = task.ExecutedAt;
            taskStatus.CompletedAt = task.CompletedAt;

            return taskStatus;
        }

        public List<TaskStatus> GetTaskStatuses()
        {
            var taskStatuses = new List<TaskStatus>();

            foreach (var task in _tasks)
            {
                taskStatuses.Add(GetTaskStatus(task.Id));
            }

            return taskStatuses;
        }
    }
}
