using DocumentManager.Api.Application.DTOs.Requests;
using DocumentManager.Api.Application.DTOs.Responses;
using DocumentManager.Api.Application.UseCases.Documents;
using DocumentManager.Api.Domain.Entities;
using DocumentManager.Api.Domain.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace DocumentManager.Api.Tests.UseCases.Documents;

public class AddCommentUseCaseTests
{
    private readonly Mock<ICommentRepository> _commentRepositoryMock;
    private readonly Mock<IDocumentRepository> _documentRepositoryMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly AddCommentUseCase _useCase;

    public AddCommentUseCaseTests()
    {
        _commentRepositoryMock = new Mock<ICommentRepository>();
        _documentRepositoryMock = new Mock<IDocumentRepository>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _useCase = new AddCommentUseCase(
            _commentRepositoryMock.Object,
            _documentRepositoryMock.Object,
            _userRepositoryMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldAddComment_WhenValid()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var request = new AddCommentRequest { Text = "Test comment" };

        var document = new Document
        {
            Id = documentId,
            IsDeleted = false
        };

        var user = new User
        {
            Id = userId,
            Email = "test@example.com"
        };

        var comment = new Comment
        {
            Id = Guid.NewGuid(),
            DocumentId = documentId,
            UserId = userId,
            Text = request.Text,
            User = user
        };

        _documentRepositoryMock
            .Setup(x => x.GetByIdAsync(documentId))
            .ReturnsAsync(document);

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(user);

        _commentRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Comment>()))
            .ReturnsAsync(comment);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _useCase.ExecuteAsync(documentId, userId, request);

        // Assert
        result.Should().NotBeNull();
        result!.Text.Should().Be(request.Text);
        result.UserId.Should().Be(userId);
        _commentRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Comment>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnNull_WhenDocumentNotFound()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var request = new AddCommentRequest { Text = "Test comment" };

        _documentRepositoryMock
            .Setup(x => x.GetByIdAsync(documentId))
            .ReturnsAsync((Document?)null);

        // Act
        var result = await _useCase.ExecuteAsync(documentId, userId, request);

        // Assert
        result.Should().BeNull();
    }
}
