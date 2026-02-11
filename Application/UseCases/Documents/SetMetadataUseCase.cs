using DocumentManager.Api.Application.DTOs.Requests;
using DocumentManager.Api.Application.DTOs.Responses;
using DocumentManager.Api.Domain.Entities;
using DocumentManager.Api.Domain.Interfaces;

namespace DocumentManager.Api.Application.UseCases.Documents;

public class SetMetadataUseCase
{
    private readonly IDocumentMetadataRepository _metadataRepository;
    private readonly IDocumentRepository _documentRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SetMetadataUseCase(
        IDocumentMetadataRepository metadataRepository,
        IDocumentRepository documentRepository,
        IUnitOfWork unitOfWork)
    {
        _metadataRepository = metadataRepository;
        _documentRepository = documentRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<DocumentMetadataResponse?> ExecuteAsync(Guid documentId, Guid ownerId, SetMetadataRequest request)
    {
        var document = await _documentRepository.GetByIdAndOwnerIdAsync(documentId, ownerId);
        if (document == null || document.IsDeleted)
            return null;

        var existingMetadata = await _metadataRepository.GetByDocumentIdAsync(documentId);
        
        if (existingMetadata != null)
        {
            existingMetadata.Latitude = request.Latitude;
            existingMetadata.Longitude = request.Longitude;
            existingMetadata.LocationName = request.LocationName;
            await _metadataRepository.UpdateAsync(existingMetadata);
        }
        else
        {
            var metadata = new DocumentMetadata
            {
                DocumentId = documentId,
                Latitude = request.Latitude,
                Longitude = request.Longitude,
                LocationName = request.LocationName
            };
            await _metadataRepository.AddAsync(metadata);
        }

        await _unitOfWork.SaveChangesAsync();

        var metadataResult = await _metadataRepository.GetByDocumentIdAsync(documentId);
        if (metadataResult == null)
            return null;

        return new DocumentMetadataResponse
        {
            Id = metadataResult.Id,
            Latitude = metadataResult.Latitude,
            Longitude = metadataResult.Longitude,
            LocationName = metadataResult.LocationName,
            CreatedAt = metadataResult.CreatedAt
        };
    }
}
