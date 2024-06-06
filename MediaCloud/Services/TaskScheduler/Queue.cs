using MediaCloud.TaskScheduler.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Task = MediaCloud.TaskScheduler.Tasks.Task;

namespace MediaCloud.TaskScheduler
{
    public class Queue
    {
        private readonly List<Task> _tasks = new();

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

        public Task? GetNextTask(string type) 
        {
            return _tasks.Where(x => x.IsWaiting && type.Split(" ")
                                .Any(y => x.GetType().Name == y))
                            .FirstOrDefault();
        }

        public List<string> GetWaitingTaskTypes() => _tasks.Where(x => x.IsWaiting)
                                                            .Select(x => x.GetType().Name)
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
    }
}
