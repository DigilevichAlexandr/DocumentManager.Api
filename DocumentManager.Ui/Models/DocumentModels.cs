namespace DocumentManager.Ui.Models;

public class DocumentResponse
{
    public Guid Id { get; set; }
    public Guid OwnerId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ExpirationAt { get; set; }
    public int Status { get; set; }
    public string? FileName { get; set; }
    public long? FileSize { get; set; }
    public string? ContentType { get; set; }
    public bool IsArchived { get; set; }
    public DateTime? ArchivedAt { get; set; }
    public string? ShareToken { get; set; }
    public DocumentMetadataResponse? Metadata { get; set; }
    public List<CommentResponse> Comments { get; set; } = new();
}

public class CommentResponse
{
    public Guid Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public Guid UserId { get; set; }
    public string UserEmail { get; set; } = string.Empty;
}

public class DocumentMetadataResponse
{
    public Guid Id { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? LocationName { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ShareLinkResponse
{
    public string ShareToken { get; set; } = string.Empty;
    public string ShareUrl { get; set; } = string.Empty;
}

public class UserStatisticsResponse
{
    public int TotalDocuments { get; set; }
    public int ActiveDocuments { get; set; }
    public int ArchivedDocuments { get; set; }
    public Dictionary<int, int> DocumentsByYear { get; set; } = new();
    public Dictionary<string, int> DocumentsByMonth { get; set; } = new();
    public int TotalComments { get; set; }
    public double AverageDocumentLifetimeDays { get; set; }
}

