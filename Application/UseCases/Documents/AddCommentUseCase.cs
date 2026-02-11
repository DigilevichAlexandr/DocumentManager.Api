using DocumentManager.Api.Application.DTOs.Requests;
using DocumentManager.Api.Application.DTOs.Responses;
using DocumentManager.Api.Domain.Entities;
using DocumentManager.Api.Domain.Interfaces;

namespace DocumentManager.Api.Application.UseCases.Documents;

public class AddCommentUseCase
{
    private readonly ICommentRepository _commentRepository;
    private readonly IDocumentRepository _documentRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AddCommentUseCase(
        ICommentRepository commentRepository,
        IDocumentRepository documentRepository,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork)
    {
        _commentRepository = commentRepository;
        _documentRepository = documentRepository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<CommentResponse?> ExecuteAsync(Guid documentId, Guid userId, AddCommentRequest request)
    {
        var document = await _documentRepository.GetByIdAsync(documentId);
        if (document == null || document.IsDeleted)
            return null;

        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            return null;

        var comment = new Comment
        {
            DocumentId = documentId,
            UserId = userId,
            Text = request.Text
        };

        await _commentRepository.AddAsync(comment);
        await _unitOfWork.SaveChangesAsync();

        return new CommentResponse
        {
            Id = comment.Id,
            Text = comment.Text,
            CreatedAt = comment.CreatedAt,
            UserId = comment.UserId,
            UserEmail = user.Email
        };
    }
}
