using DocumentManager.Api.Application.Interfaces;
using DocumentManager.Api.Domain.Interfaces;

namespace DocumentManager.Api.Application.UseCases.Documents;

public class DeleteDocumentUseCase
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFileService _fileService;

    public DeleteDocumentUseCase(
        IDocumentRepository documentRepository, 
        IUnitOfWork unitOfWork,
        IFileService fileService)
    {
        _documentRepository = documentRepository;
        _unitOfWork = unitOfWork;
        _fileService = fileService;
    }

    public async Task<bool> ExecuteAsync(Guid documentId, Guid ownerId)
    {
        var document = await _documentRepository.GetByIdAndOwnerIdAsync(documentId, ownerId);
        
        if (document == null)
            return false;

        if (!string.IsNullOrEmpty(document.FilePath))
        {
            await _fileService.DeleteFileAsync(document.FilePath);
        }

        document.IsDeleted = true;
        document.DeletedAt = DateTime.UtcNow;
        
        await _documentRepository.UpdateAsync(document);
        await _unitOfWork.SaveChangesAsync();
        
        return true;
    }
}
