namespace MediaCloud.TaskScheduler
{
    public class TaskStatus
    {
        public Guid Id { get; set; }
        public int QueuePosition { get; set; }
        public bool IsInProgress { get; set; }
        public int WorkCount { get; set; }
        public bool IsCompleted { get; set; }
        public string CompletionMessage {get; set; } = string.Empty;
        public DateTime ExecutedAt { get; set; }
        public DateTime CompletedAt {get; set; } 
        public List<Guid> AffectedEntities {get; set;} = [];
    }
}
