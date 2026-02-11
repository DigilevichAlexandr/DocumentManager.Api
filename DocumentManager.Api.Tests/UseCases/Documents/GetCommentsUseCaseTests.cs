using DocumentManager.Api.Application.DTOs.Responses;
using DocumentManager.Api.Application.UseCases.Documents;
using DocumentManager.Api.Domain.Entities;
using DocumentManager.Api.Domain.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace DocumentManager.Api.Tests.UseCases.Documents;

public class GetCommentsUseCaseTests
{
    private readonly Mock<ICommentRepository> _commentRepositoryMock;
    private readonly GetCommentsUseCase _useCase;

    public GetCommentsUseCaseTests()
    {
        _commentRepositoryMock = new Mock<ICommentRepository>();
        _useCase = new GetCommentsUseCase(_commentRepositoryMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnComments()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var comments = new List<Comment>
        {
            new Comment
            {
                Id = Guid.NewGuid(),
                DocumentId = documentId,
                UserId = userId,
                Text = "Comment 1",
                User = new User { Id = userId, Email = "user1@example.com" }
            },
            new Comment
            {
                Id = Guid.NewGuid(),
                DocumentId = documentId,
                UserId = userId,
                Text = "Comment 2",
                User = new User { Id = userId, Email = "user1@example.com" }
            }
        };

        _commentRepositoryMock
            .Setup(x => x.GetByDocumentIdAsync(documentId))
            .ReturnsAsync(comments);

        // Act
        var result = await _useCase.ExecuteAsync(documentId);

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(c => c.Text.StartsWith("Comment"));
    }
}
