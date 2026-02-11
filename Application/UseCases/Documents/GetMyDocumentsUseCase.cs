using DocumentManager.Api.Application.DTOs.Responses;
using DocumentManager.Api.Domain.Interfaces;

namespace DocumentManager.Api.Application.UseCases.Documents;

public class GetMyDocumentsUseCase
{
    private readonly IDocumentRepository _documentRepository;

    public GetMyDocumentsUseCase(IDocumentRepository documentRepository)
    {
        _documentRepository = documentRepository;
    }

    public async Task<IEnumerable<DocumentResponse>> ExecuteAsync(Guid ownerId)
    {
        var documents = await _documentRepository.GetByOwnerIdAsync(ownerId);
        
        return documents
            .Where(d => !d.IsDeleted && !d.IsArchived)
            .Select(d => new DocumentResponse
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
                ShareToken = d.ShareToken,
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
