using DocumentManager.Api.Application.DTOs.Requests;
using DocumentManager.Api.Application.DTOs.Responses;
using DocumentManager.Api.Application.Interfaces;
using DocumentManager.Api.Domain.Entities;
using DocumentManager.Api.Domain.Interfaces;

namespace DocumentManager.Api.Application.UseCases.Documents;

public class CreateDocumentUseCase
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFileService _fileService;

    public CreateDocumentUseCase(
        IDocumentRepository documentRepository, 
        IUnitOfWork unitOfWork,
        IFileService fileService)
    {
        _documentRepository = documentRepository;
        _unitOfWork = unitOfWork;
        _fileService = fileService;
    }

    public async Task<DocumentResponse> ExecuteAsync(CreateDocumentRequest request, Guid ownerId, IFormFile? file = null)
    {
        var document = new Document
        {
            Name = request.Name,
            Description = request.Description,
            OwnerId = ownerId,
            ExpirationAt = DateTime.UtcNow.AddDays(request.ExpirationDays)
        };

        if (file != null && file.Length > 0)
        {
            document.FileName = file.FileName;
            document.FileSize = file.Length;
            document.ContentType = _fileService.GetContentType(file.FileName);
            
            using var fileStream = file.OpenReadStream();
            document.FilePath = await _fileService.SaveFileAsync(fileStream, file.FileName, document.Id, ownerId);
        }

        var createdDocument = await _documentRepository.AddAsync(document);
        await _unitOfWork.SaveChangesAsync();

        return new DocumentResponse
        {
            Id = createdDocument.Id,
            OwnerId = createdDocument.OwnerId,
            Name = createdDocument.Name,
            Description = createdDocument.Description,
            CreatedAt = createdDocument.CreatedAt,
            ExpirationAt = createdDocument.ExpirationAt,
            Status = createdDocument.Status,
            FileName = createdDocument.FileName,
            FileSize = createdDocument.FileSize,
            ContentType = createdDocument.ContentType
        };
    }
}
