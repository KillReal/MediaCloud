using Task = MediaCloud.TaskScheduler.Tasks.Task;

namespace MediaCloud.TaskScheduler
{
    public class Queue
    {
        private readonly List<Task> _tasks = [];

        public Action<Task> OnTaskComplete;

        public Queue() 
        {
            OnTaskComplete += RemoveTask;
        }

        public bool IsEmpty => _tasks.Count == 0;

        public int TaskCount => _tasks.Count;

        public int WorkCount => _tasks.Sum(x => x.GetWorkCount());

        public void AddTask(Task task) => _tasks.Add(task);

        public void RemoveTask(Task task) => _tasks.Remove(task);

        public Task? GetNextTask(List<Type> types) 
        {
            return _tasks.Where(x => x.IsWaiting && types.Any(y => x.GetType() == y))
                            .FirstOrDefault();
        }

        public List<Type> GetWaitingTaskTypes() => _tasks.Where(x => x.IsWaiting)
                                                            .Select(x => x.GetType())
                                                            .ToList();

        public Task? GetTask(Guid id) => _tasks.FirstOrDefault(x => x.Id == id);

        public int GetTaskPosition(Guid id)
        {
            var task = GetTask(id);

            if (task == null)
            {
                return -1;
            }

            return _tasks.IndexOf(task) + 1;
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

            taskStatus.IsInProgress = !task.IsWaiting;
            taskStatus.IsExist = true;
            taskStatus.WorkCount = task.GetWorkCount();
            taskStatus.ExecutedAt = task.ExecutedAt;

            return taskStatus;
        }

        public List<TaskStatus> GetTaskStatuses() => _tasks.Select(x => GetTaskStatus(x.Id)).ToList();
    }
}
