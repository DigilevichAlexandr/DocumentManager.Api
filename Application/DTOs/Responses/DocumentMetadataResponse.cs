namespace DocumentManager.Api.Application.DTOs.Responses;

public class DocumentMetadataResponse
{
    public Guid Id { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? LocationName { get; set; }
    public DateTime CreatedAt { get; set; }
}
