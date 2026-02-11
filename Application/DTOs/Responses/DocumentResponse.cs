using DocumentManager.Api.Domain.Enums;

namespace DocumentManager.Api.Application.DTOs.Responses;

public class DocumentResponse
{
    public Guid Id { get; set; }
    public Guid OwnerId { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ExpirationAt { get; set; }
    public DocumentStatus Status { get; set; }
    public string? FileName { get; set; }
    public long? FileSize { get; set; }
    public string? ContentType { get; set; }
    public bool IsArchived { get; set; }
    public DateTime? ArchivedAt { get; set; }
    public string? ShareToken { get; set; }
    public DocumentMetadataResponse? Metadata { get; set; }
    public List<CommentResponse> Comments { get; set; } = new();
}
