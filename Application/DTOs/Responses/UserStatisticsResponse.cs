namespace DocumentManager.Api.Application.DTOs.Responses;

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
