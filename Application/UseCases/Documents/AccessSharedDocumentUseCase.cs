using DocumentManager.Api.Application.DTOs.Responses;
using DocumentManager.Api.Application.Interfaces;
using DocumentManager.Api.Domain.Entities;
using DocumentManager.Api.Domain.Interfaces;

namespace DocumentManager.Api.Application.UseCases.Documents;

public class AccessSharedDocumentUseCase
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IDocumentMetadataRepository _metadataRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFileService _fileService;

    public AccessSharedDocumentUseCase(
        IDocumentRepository documentRepository,
        IDocumentMetadataRepository metadataRepository,
        IUnitOfWork unitOfWork,
        IFileService fileService)
    {
        _documentRepository = documentRepository;
        _metadataRepository = metadataRepository;
        _unitOfWork = unitOfWork;
        _fileService = fileService;
    }

    public async Task<DocumentResponse?> ExecuteAsync(string shareToken, Guid newOwnerId)
    {
        var originalDocument = await _documentRepository.GetByShareTokenAsync(shareToken);
        if (originalDocument == null || originalDocument.IsDeleted)
            return null;

        // Создаем копию документа
        var copiedDocument = new Document
        {
            Name = originalDocument.Name,
            Description = originalDocument.Description,
            OwnerId = newOwnerId,
            CreatedAt = DateTime.UtcNow,
            ExpirationAt = originalDocument.ExpirationAt,
            Status = originalDocument.Status,
            FileName = originalDocument.FileName,
            FileSize = originalDocument.FileSize,
            ContentType = originalDocument.ContentType
        };

        // Копируем файл, если он есть
        if (!string.IsNullOrEmpty(originalDocument.FilePath) && !string.IsNullOrEmpty(originalDocument.FileName))
        {
            var sourcePath = originalDocument.FilePath;
            copiedDocument.FilePath = await _fileService.CopyFileAsync(sourcePath, originalDocument.FileName, copiedDocument.Id, newOwnerId);
        }

        // Копируем метаданные, если они есть
        if (originalDocument.Metadata != null)
        {
            var copiedMetadata = new DocumentMetadata
            {
                DocumentId = copiedDocument.Id,
                Latitude = originalDocument.Metadata.Latitude,
                Longitude = originalDocument.Metadata.Longitude,
                LocationName = originalDocument.Metadata.LocationName
            };
            await _metadataRepository.AddAsync(copiedMetadata);
        }

        await _documentRepository.AddAsync(copiedDocument);

        // Удаляем оригинальный документ
        originalDocument.IsDeleted = true;
        originalDocument.DeletedAt = DateTime.UtcNow;
        originalDocument.ShareToken = null;
        await _documentRepository.UpdateAsync(originalDocument);

        await _unitOfWork.SaveChangesAsync();

        return new DocumentResponse
        {
            Id = copiedDocument.Id,
            OwnerId = copiedDocument.OwnerId,
            Name = copiedDocument.Name,
            Description = copiedDocument.Description,
            CreatedAt = copiedDocument.CreatedAt,
            ExpirationAt = copiedDocument.ExpirationAt,
            Status = copiedDocument.Status,
            FileName = copiedDocument.FileName,
            FileSize = copiedDocument.FileSize,
            ContentType = copiedDocument.ContentType
        };
    }
}
