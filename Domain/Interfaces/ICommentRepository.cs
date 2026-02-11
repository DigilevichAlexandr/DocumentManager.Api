using DocumentManager.Api.Domain.Entities;

namespace DocumentManager.Api.Domain.Interfaces;

public interface ICommentRepository : IRepository<Comment>
{
    Task<IEnumerable<Comment>> GetByDocumentIdAsync(Guid documentId);
}
