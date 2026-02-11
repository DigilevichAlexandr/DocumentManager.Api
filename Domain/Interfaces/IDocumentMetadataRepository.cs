using DocumentManager.Api.Domain.Entities;

namespace DocumentManager.Api.Domain.Interfaces;

public interface IDocumentMetadataRepository : IRepository<DocumentMetadata>
{
    Task<DocumentMetadata?> GetByDocumentIdAsync(Guid documentId);
}
