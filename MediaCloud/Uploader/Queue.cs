using MediaCloud.MediaUploader.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Task = MediaCloud.MediaUploader.Tasks.Task;

namespace MediaCloud.MediaUploader
{
    public static class Queue
    {
        private static List<Task> _tasks = new();

        public static bool IsEmpty => _tasks.Count == 0;

        public static int TaskCount => _tasks.Count;

        public static int WorkCount => _tasks.Sum(x => x.GetWorkCount());

        public static void AddTask(Task task) => _tasks.Add(task);

        public static void RemoveTask(Task task) => _tasks.Remove(task);

        public static Task GetTask() => _tasks.First();

        public static Task? GetTask(Guid id) => _tasks.FirstOrDefault(x => x.Id == id);

        public static int GetTaskPosition(Guid id)
        {
            var task = GetTask(id);

            if (task == null)
            {
                return -1;
            }

            return _tasks.IndexOf(task);
        }
    }
}
