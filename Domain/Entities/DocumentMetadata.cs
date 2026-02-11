namespace DocumentManager.Api.Domain.Entities;

public class DocumentMetadata
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid DocumentId { get; set; }
    public Document Document { get; set; } = null!;
    
    // Координаты местоположения
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    
    // Дополнительные метаданные
    public string? LocationName { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
