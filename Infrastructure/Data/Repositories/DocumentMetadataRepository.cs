using DocumentManager.Api.Domain.Entities;
using DocumentManager.Api.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DocumentManager.Api.Infrastructure.Data.Repositories;

public class DocumentMetadataRepository : IDocumentMetadataRepository
{
    private readonly AppDbContext _context;

    public DocumentMetadataRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<DocumentMetadata?> GetByIdAsync(Guid id)
    {
        return await _context.DocumentMetadata.FindAsync(id);
    }

    public async Task<IEnumerable<DocumentMetadata>> GetAllAsync()
    {
        return await _context.DocumentMetadata.ToListAsync();
    }

    public async Task<DocumentMetadata> AddAsync(DocumentMetadata entity)
    {
        await _context.DocumentMetadata.AddAsync(entity);
        return entity;
    }

    public Task UpdateAsync(DocumentMetadata entity)
    {
        _context.DocumentMetadata.Update(entity);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(DocumentMetadata entity)
    {
        _context.DocumentMetadata.Remove(entity);
        return Task.CompletedTask;
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _context.DocumentMetadata.AnyAsync(m => m.Id == id);
    }

    public async Task<DocumentMetadata?> GetByDocumentIdAsync(Guid documentId)
    {
        return await _context.DocumentMetadata
            .FirstOrDefaultAsync(m => m.DocumentId == documentId);
    }
}
