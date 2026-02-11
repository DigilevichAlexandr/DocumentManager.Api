using DocumentManager.Api.Domain.Entities;

namespace DocumentManager.Api.Domain.Interfaces;

public interface IDocumentRepository : IRepository<Document>
{
    Task<IEnumerable<Document>> GetByOwnerIdAsync(Guid ownerId);
    Task<Document?> GetByIdAndOwnerIdAsync(Guid id, Guid ownerId);
    Task<Document?> GetByShareTokenAsync(string shareToken);
    Task<IEnumerable<Document>> GetArchivedByOwnerIdAsync(Guid ownerId);
    Task<IEnumerable<Document>> GetExpiredDocumentsAsync();
    Task<IEnumerable<Document>> GetDocumentsForHistoryDeletionAsync();
}
