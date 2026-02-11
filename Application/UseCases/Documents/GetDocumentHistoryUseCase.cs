using DocumentManager.Api.Application.DTOs.Responses;
using DocumentManager.Api.Domain.Interfaces;

namespace DocumentManager.Api.Application.UseCases.Documents;

public class GetDocumentHistoryUseCase
{
    private readonly IDocumentRepository _documentRepository;

    public GetDocumentHistoryUseCase(IDocumentRepository documentRepository)
    {
        _documentRepository = documentRepository;
    }

    public async Task<IEnumerable<DocumentResponse>> ExecuteAsync(Guid ownerId)
    {
        var archivedDocuments = await _documentRepository.GetArchivedByOwnerIdAsync(ownerId);
        
        return archivedDocuments.Select(d => new DocumentResponse
        {
            Id = d.Id,
            OwnerId = d.OwnerId,
            Name = d.Name,
            Description = d.Description,
            CreatedAt = d.CreatedAt,
            ExpirationAt = d.ExpirationAt,
            Status = d.Status,
            FileName = d.FileName,
            FileSize = d.FileSize,
            ContentType = d.ContentType,
            IsArchived = d.IsArchived,
            ArchivedAt = d.ArchivedAt,
            Metadata = d.Metadata != null ? new DocumentMetadataResponse
            {
                Id = d.Metadata.Id,
                Latitude = d.Metadata.Latitude,
                Longitude = d.Metadata.Longitude,
                LocationName = d.Metadata.LocationName,
                CreatedAt = d.Metadata.CreatedAt
            } : null,
            Comments = d.Comments.Select(c => new CommentResponse
            {
                Id = c.Id,
                Text = c.Text,
                CreatedAt = c.CreatedAt,
                UserId = c.UserId,
                UserEmail = c.User.Email
            }).ToList()
        });
    }
}
