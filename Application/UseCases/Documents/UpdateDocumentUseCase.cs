using DocumentManager.Api.Application.DTOs.Requests;
using DocumentManager.Api.Application.DTOs.Responses;
using DocumentManager.Api.Domain.Interfaces;

namespace DocumentManager.Api.Application.UseCases.Documents;

public class UpdateDocumentUseCase
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateDocumentUseCase(IDocumentRepository documentRepository, IUnitOfWork unitOfWork)
    {
        _documentRepository = documentRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<DocumentResponse?> ExecuteAsync(Guid documentId, Guid ownerId, UpdateDocumentRequest request)
    {
        var document = await _documentRepository.GetByIdAndOwnerIdAsync(documentId, ownerId);
        if (document == null || document.IsDeleted)
            return null;

        document.Name = request.Name;
        document.Description = request.Description;
        document.ExpirationAt = DateTime.UtcNow.AddDays(request.ExpirationDays);

        await _documentRepository.UpdateAsync(document);
        await _unitOfWork.SaveChangesAsync();

        return new DocumentResponse
        {
            Id = document.Id,
            OwnerId = document.OwnerId,
            Name = document.Name,
            Description = document.Description,
            CreatedAt = document.CreatedAt,
            ExpirationAt = document.ExpirationAt,
            Status = document.Status,
            FileName = document.FileName,
            FileSize = document.FileSize,
            ContentType = document.ContentType,
            IsArchived = document.IsArchived,
            ArchivedAt = document.ArchivedAt
        };
    }
}
