using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaCloud.TaskScheduler
{
    public class TaskStatus
    {
        public Guid Id { get; set; }
        public bool IsInProgress { get; set; }
        public bool IsExist { get; set; }
        public int WorkCount { get; set; }
        public int QueuePosition { get; set; }
    }
}
