using DocumentManager.Api.Application.DTOs.Responses;
using DocumentManager.Api.Domain.Interfaces;

namespace DocumentManager.Api.Application.UseCases.Documents;

public class GetCommentsUseCase
{
    private readonly ICommentRepository _commentRepository;

    public GetCommentsUseCase(ICommentRepository commentRepository)
    {
        _commentRepository = commentRepository;
    }

    public async Task<IEnumerable<CommentResponse>> ExecuteAsync(Guid documentId)
    {
        var comments = await _commentRepository.GetByDocumentIdAsync(documentId);
        
        return comments.Select(c => new CommentResponse
        {
            Id = c.Id,
            Text = c.Text,
            CreatedAt = c.CreatedAt,
            UserId = c.UserId,
            UserEmail = c.User.Email
        });
    }
}
