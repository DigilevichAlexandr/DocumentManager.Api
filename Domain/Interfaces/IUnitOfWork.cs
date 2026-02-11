namespace DocumentManager.Api.Domain.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IDocumentRepository Documents { get; }
    IUserRepository Users { get; }
    ICommentRepository Comments { get; }
    IDocumentMetadataRepository DocumentMetadata { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
