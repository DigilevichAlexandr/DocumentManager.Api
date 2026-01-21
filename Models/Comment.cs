namespace DocumentManager.Api.Models
{
    public class Comment
    {
        public int Id { get; set; }
        public string Text { get; set; } = null!;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public int DocumentId { get; set; }
        public Document Document { get; set; } = null!;
        public int UserId { get; set; }
        public User User { get; set; } = null!;
    }

}
