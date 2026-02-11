using DocumentManager.Api.Domain.Enums;

namespace DocumentManager.Api.Domain.Entities;

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
    public bool IsDeleted { get; set; } = false;
    
    // Файл
    public string? FilePath { get; set; }
    public string? FileName { get; set; }
    public long? FileSize { get; set; }
    public string? ContentType { get; set; }
    
    // Шаринг
    public string? ShareToken { get; set; }
    public DateTime? SharedAt { get; set; }
    
    // История
    public bool IsArchived { get; set; } = false;
    public DateTime? ArchivedAt { get; set; }
    public DateTime? HistoryDeleteAt { get; set; } // Дата удаления из истории (через 30 дней после архивации)
    
    // Навигационные свойства
    public List<Comment> Comments { get; set; } = new();
    public DocumentMetadata? Metadata { get; set; }
}
