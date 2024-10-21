namespace MediaCloud.Data.Models
{
    public class User : Record
    {
        public string? Name { get; set; }
        public string? PasswordHash { get; set; }
        public bool IsAdmin { get; set; }
        public bool IsPublic { get; set; }
        public bool IsActivated { get; set; }
        public DateTime LastLoginAt { get; set; }
        public DateTime? NextLoginAttemptAt {get; set; }
        public int FailLoginAttemptCount {get; set;}
        public string? InviteCode { get; set; }
        public string? PersonalSettings { get; set; }
    }
}
