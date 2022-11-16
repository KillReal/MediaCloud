using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaCloud.MediaUploader
{
    public class UploaderTaskStatus
    {
        public Guid Id { get; set; }
        public string TaskType { get; set; }
        public bool IsInProcess { get; set; }
        public int WorkCount { get; set; }
        public int QueuePosition { get; set; }

        public UploaderTaskStatus(Guid id)
        {
            Id = id;
            IsInProcess = Scheduler.IsTaskInProgress(id);
            QueuePosition = Queue.GetTaskPosition(id);
            
            var task = Queue.GetTask(id);

            TaskType = task == null
                ? "Doesn't exist"
                : task.GetType().Name;

            WorkCount = task == null
                ? 0
                : task.GetWorkCount();
        }
    }
}
