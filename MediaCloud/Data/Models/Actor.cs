namespace MediaCloud.Data.Models
{
    public class Actor : Record
    {
        public string? Name { get; set; }
        public string? PasswordHash { get; set; }
        public bool IsAdmin { get; set; }
        public bool IsPublic { get; set; }
        public bool IsActivated { get; set; }
        public DateTime LastLoginAt { get; set; }
        public string? InviteCode { get; set; }
    }
}
