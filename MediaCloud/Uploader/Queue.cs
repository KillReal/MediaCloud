using MediaCloud.MediaUploader.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaCloud.MediaUploader
{
    public static class Queue
    {
        private static List<UploadTask> _tasks = new();

        public static bool IsEmpty
        {
            get => _tasks.Count == 0;
        }

        public static int TaskCount
        {
            get => _tasks.Count;
        }

        public static int MediaCount
        {
            get
            {
                var count = 0;

                foreach (var task in _tasks)
                {
                    count += task.Content.Count;
                }

                return count;
            }
        }

        public static void AddTask(UploadTask task) => _tasks.Add(task);

        public static void RemoveTask(UploadTask task) => _tasks.Remove(task);

        public static UploadTask GetTask() => _tasks.First();

        public static UploadTask? GetTask(Guid id) => _tasks.FirstOrDefault(x => x.Id == id);
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
