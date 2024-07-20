namespace MediaCloud.TaskScheduler
{
    public class TaskStatus
    {
        public Guid Id { get; set; }
        public int QueuePosition { get; set; }
        public bool IsInProgress { get; set; }
        public int WorkCount { get; set; }
        public bool IsExist { get; set; }
        public DateTime ExecutedAt { get; set; }
    }
}
