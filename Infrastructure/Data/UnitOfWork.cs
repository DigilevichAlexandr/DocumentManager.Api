using DocumentManager.Api.Domain.Interfaces;
using DocumentManager.Api.Infrastructure.Data.Repositories;

namespace DocumentManager.Api.Infrastructure.Data;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;
    private IDocumentRepository? _documents;
    private IUserRepository? _users;
    private ICommentRepository? _comments;
    private IDocumentMetadataRepository? _documentMetadata;

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
    }

    public IDocumentRepository Documents
    {
        get
        {
            _documents ??= new DocumentRepository(_context);
            return _documents;
        }
    }

    public IUserRepository Users
    {
        get
        {
            _users ??= new UserRepository(_context);
            return _users;
        }
    }

    public ICommentRepository Comments
    {
        get
        {
            _comments ??= new CommentRepository(_context);
            return _comments;
        }
    }

    public IDocumentMetadataRepository DocumentMetadata
    {
        get
        {
            _documentMetadata ??= new DocumentMetadataRepository(_context);
            return _documentMetadata;
        }
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
