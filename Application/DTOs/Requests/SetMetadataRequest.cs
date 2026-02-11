namespace DocumentManager.Api.Application.DTOs.Requests;

public class SetMetadataRequest
{
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? LocationName { get; set; }
}
