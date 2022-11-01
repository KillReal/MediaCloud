using MediaCloud.Uploader.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaCloud.Uploader
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

        public static void AddTask(UploadTask task)
        {
            _tasks.Add(task);
        }

        public static UploadTask GetTask()
        {
            return _tasks.First();
        }
    }
}
