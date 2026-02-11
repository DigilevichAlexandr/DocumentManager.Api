using DocumentManager.Api.Application.DTOs.Responses;
using DocumentManager.Api.Domain.Interfaces;
using System.Security.Cryptography;
using System.Text;

namespace DocumentManager.Api.Application.UseCases.Documents;

public class ShareDocumentUseCase
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ShareDocumentUseCase(IDocumentRepository documentRepository, IUnitOfWork unitOfWork)
    {
        _documentRepository = documentRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ShareLinkResponse?> ExecuteAsync(Guid documentId, Guid ownerId, string baseUrl)
    {
        var document = await _documentRepository.GetByIdAndOwnerIdAsync(documentId, ownerId);
        if (document == null || document.IsDeleted || document.IsArchived)
            return null;

        // Генерируем уникальный токен для шаринга
        var token = GenerateShareToken();
        document.ShareToken = token;
        document.SharedAt = DateTime.UtcNow;

        await _documentRepository.UpdateAsync(document);
        await _unitOfWork.SaveChangesAsync();

        var shareUrl = $"{baseUrl}/api/documents/share/{token}";

        return new ShareLinkResponse
        {
            ShareToken = token,
            ShareUrl = shareUrl
        };
    }

    private string GenerateShareToken()
    {
        var bytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes).Replace("+", "-").Replace("/", "_").Replace("=", "");
    }
}
