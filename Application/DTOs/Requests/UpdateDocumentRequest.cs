namespace DocumentManager.Api.Application.DTOs.Requests;

public class UpdateDocumentRequest
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public int ExpirationDays { get; set; }
}
