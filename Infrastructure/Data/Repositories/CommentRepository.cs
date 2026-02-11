using DocumentManager.Api.Domain.Entities;
using DocumentManager.Api.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DocumentManager.Api.Infrastructure.Data.Repositories;

public class CommentRepository : ICommentRepository
{
    private readonly AppDbContext _context;

    public CommentRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Comment?> GetByIdAsync(Guid id)
    {
        return await _context.Comments.FindAsync(id);
    }

    public async Task<IEnumerable<Comment>> GetAllAsync()
    {
        return await _context.Comments.ToListAsync();
    }

    public async Task<Comment> AddAsync(Comment entity)
    {
        await _context.Comments.AddAsync(entity);
        return entity;
    }

    public Task UpdateAsync(Comment entity)
    {
        _context.Comments.Update(entity);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Comment entity)
    {
        _context.Comments.Remove(entity);
        return Task.CompletedTask;
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _context.Comments.AnyAsync(c => c.Id == id);
    }

    public async Task<IEnumerable<Comment>> GetByDocumentIdAsync(Guid documentId)
    {
        return await _context.Comments
            .Include(c => c.User)
            .Where(c => c.DocumentId == documentId)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync();
    }
}
