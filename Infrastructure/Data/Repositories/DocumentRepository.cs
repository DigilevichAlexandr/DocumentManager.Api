using DocumentManager.Api.Domain.Entities;
using DocumentManager.Api.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DocumentManager.Api.Infrastructure.Data.Repositories;

public class DocumentRepository : IDocumentRepository
{
    private readonly AppDbContext _context;

    public DocumentRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Document?> GetByIdAsync(Guid id)
    {
        return await _context.Documents.FindAsync(id);
    }

    public async Task<IEnumerable<Document>> GetAllAsync()
    {
        return await _context.Documents.ToListAsync();
    }

    public async Task<Document> AddAsync(Document entity)
    {
        await _context.Documents.AddAsync(entity);
        return entity;
    }

    public Task UpdateAsync(Document entity)
    {
        _context.Documents.Update(entity);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Document entity)
    {
        _context.Documents.Remove(entity);
        return Task.CompletedTask;
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _context.Documents.AnyAsync(d => d.Id == id);
    }

    public async Task<IEnumerable<Document>> GetByOwnerIdAsync(Guid ownerId)
    {
        return await _context.Documents
            .Include(d => d.Comments)
                .ThenInclude(c => c.User)
            .Include(d => d.Metadata)
            .Where(d => d.OwnerId == ownerId)
            .ToListAsync();
    }

    public async Task<Document?> GetByIdAndOwnerIdAsync(Guid id, Guid ownerId)
    {
        return await _context.Documents
            .FirstOrDefaultAsync(d => d.Id == id && d.OwnerId == ownerId);
    }

    public async Task<Document?> GetByShareTokenAsync(string shareToken)
    {
        return await _context.Documents
            .Include(d => d.Metadata)
            .FirstOrDefaultAsync(d => d.ShareToken == shareToken && !d.IsDeleted);
    }

    public async Task<IEnumerable<Document>> GetArchivedByOwnerIdAsync(Guid ownerId)
    {
        return await _context.Documents
            .Include(d => d.Comments)
                .ThenInclude(c => c.User)
            .Include(d => d.Metadata)
            .Where(d => d.OwnerId == ownerId && d.IsArchived)
            .ToListAsync();
    }

    public async Task<IEnumerable<Document>> GetExpiredDocumentsAsync()
    {
        var now = DateTime.UtcNow;
        return await _context.Documents
            .Where(d => !d.IsArchived && !d.IsDeleted && d.ExpirationAt <= now)
            .ToListAsync();
    }

    public async Task<IEnumerable<Document>> GetDocumentsForHistoryDeletionAsync()
    {
        var deleteDate = DateTime.UtcNow.AddDays(-30);
        return await _context.Documents
            .Where(d => d.IsArchived && d.ArchivedAt.HasValue && d.ArchivedAt.Value <= deleteDate)
            .ToListAsync();
    }
}
