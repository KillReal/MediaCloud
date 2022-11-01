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
        public bool IsInProcess { get; set; }
        public int MediaCount { get; set; }
        public int QueuePosition { get; set; }

        public UploaderTaskStatus(Guid id)
        {
            Id = id;
            IsInProcess = Scheduler.IsTaskInProgress(id);
            
            var task = Queue.GetTask(id);
            QueuePosition = Queue.GetTaskPosition(id) + 1;
            MediaCount = task == null
                ? 0
                : task.Content.Count;
        }
    }
}
