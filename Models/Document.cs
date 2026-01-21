namespace DocumentManager.Api.Models
{
    public enum DocumentStatus
    {
        Active,
        Expired,
        Deleted
    }

    public class Document
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid OwnerId { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime ExpirationAt { get; set; }

        public DocumentStatus Status { get; set; } = DocumentStatus.Active;
        public DateTime? DeletedAt { get; set; }
    }
}
