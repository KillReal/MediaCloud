using MediaCloud.MediaUploader.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Task = MediaCloud.MediaUploader.Tasks.Task;

namespace MediaCloud.MediaUploader
{
    public class Queue
    {
        private readonly List<Task> _tasks = new();

        public Action<Guid> OnTaskComplete;

        public Queue() 
        {
            OnTaskComplete += RemoveTask;
        }

        public bool IsEmpty => _tasks.Count == 0;

        public int TaskCount => _tasks.Count;

        public int WorkCount => _tasks.Sum(x => x.GetWorkCount());

        public void AddTask(Task task) => _tasks.Add(task);

        public void RemoveTask(Task task) => _tasks.Remove(task);
        public void RemoveTask(Guid id) => RemoveTask(_tasks.First(x => x.Id == id));

        public Task GetNextTask() => _tasks.First();

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
