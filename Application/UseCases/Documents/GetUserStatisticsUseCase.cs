using DocumentManager.Api.Application.DTOs.Responses;
using DocumentManager.Api.Domain.Interfaces;

namespace DocumentManager.Api.Application.UseCases.Documents;

public class GetUserStatisticsUseCase
{
    private readonly IDocumentRepository _documentRepository;
    private readonly ICommentRepository _commentRepository;

    public GetUserStatisticsUseCase(
        IDocumentRepository documentRepository,
        ICommentRepository commentRepository)
    {
        _documentRepository = documentRepository;
        _commentRepository = commentRepository;
    }

    public async Task<UserStatisticsResponse> ExecuteAsync(Guid userId, int? year = null)
    {
        var allDocuments = (await _documentRepository.GetByOwnerIdAsync(userId))
            .Where(d => !d.IsDeleted)
            .ToList();

        var filteredDocuments = year.HasValue
            ? allDocuments.Where(d => d.CreatedAt.Year == year.Value).ToList()
            : allDocuments;

        var activeDocuments = filteredDocuments.Count(d => !d.IsArchived);
        var archivedDocuments = filteredDocuments.Count(d => d.IsArchived);

        var documentsByYear = allDocuments
            .GroupBy(d => d.CreatedAt.Year)
            .ToDictionary(g => g.Key, g => g.Count());

        var documentsByMonth = filteredDocuments
            .GroupBy(d => d.CreatedAt.ToString("yyyy-MM"))
            .ToDictionary(g => g.Key, g => g.Count());

        var userComments = 0;
        foreach (var doc in filteredDocuments)
        {
            var comments = await _commentRepository.GetByDocumentIdAsync(doc.Id);
            userComments += comments.Count();
        }

        var averageLifetime = filteredDocuments.Any()
            ? filteredDocuments
                .Where(d => d.IsArchived && d.ArchivedAt.HasValue)
                .Select(d => (d.ArchivedAt!.Value - d.CreatedAt).TotalDays)
                .DefaultIfEmpty(0)
                .Average()
            : 0;

        return new UserStatisticsResponse
        {
            TotalDocuments = filteredDocuments.Count,
            ActiveDocuments = activeDocuments,
            ArchivedDocuments = archivedDocuments,
            DocumentsByYear = documentsByYear,
            DocumentsByMonth = documentsByMonth,
            TotalComments = userComments,
            AverageDocumentLifetimeDays = Math.Round(averageLifetime, 2)
        };
    }
}
