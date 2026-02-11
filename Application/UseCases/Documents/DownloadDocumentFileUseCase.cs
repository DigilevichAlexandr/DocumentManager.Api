using DocumentManager.Api.Application.Interfaces;
using DocumentManager.Api.Domain.Interfaces;

namespace DocumentManager.Api.Application.UseCases.Documents;

public class DownloadDocumentFileUseCase
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IFileService _fileService;

    public DownloadDocumentFileUseCase(IDocumentRepository documentRepository, IFileService fileService)
    {
        _documentRepository = documentRepository;
        _fileService = fileService;
    }

    public async Task<(FileStream? FileStream, string? FileName, string? ContentType)?> ExecuteAsync(Guid documentId, Guid ownerId)
    {
        var document = await _documentRepository.GetByIdAndOwnerIdAsync(documentId, ownerId);
        
        if (document == null || document.IsDeleted || string.IsNullOrEmpty(document.FilePath))
            return null;

        var fileStream = await _fileService.GetFileAsync(document.FilePath);
        
        if (fileStream == null)
            return null;

        return (fileStream, document.FileName, document.ContentType);
    }
}
