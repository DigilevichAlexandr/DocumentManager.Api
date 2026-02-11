using DocumentManager.Api.Application.DTOs.Responses;
using DocumentManager.Api.Application.UseCases.Documents;
using DocumentManager.Api.Domain.Entities;
using DocumentManager.Api.Domain.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace DocumentManager.Api.Tests.UseCases.Documents;

public class ShareDocumentUseCaseTests
{
    private readonly Mock<IDocumentRepository> _documentRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly ShareDocumentUseCase _useCase;

    public ShareDocumentUseCaseTests()
    {
        _documentRepositoryMock = new Mock<IDocumentRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _useCase = new ShareDocumentUseCase(_documentRepositoryMock.Object, _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldGenerateShareToken_WhenValidDocument()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var baseUrl = "https://localhost:7153";

        var document = new Document
        {
            Id = documentId,
            OwnerId = ownerId,
            Name = "Test Document",
            IsDeleted = false,
            IsArchived = false
        };

        _documentRepositoryMock
            .Setup(x => x.GetByIdAndOwnerIdAsync(documentId, ownerId))
            .ReturnsAsync(document);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _useCase.ExecuteAsync(documentId, ownerId, baseUrl);

        // Assert
        result.Should().NotBeNull();
        result!.ShareToken.Should().NotBeNullOrEmpty();
        result.ShareUrl.Should().Contain(result.ShareToken);
        document.ShareToken.Should().NotBeNull();
        document.SharedAt.Should().NotBeNull();
        _documentRepositoryMock.Verify(x => x.UpdateAsync(document), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnNull_WhenDocumentNotFound()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var baseUrl = "https://localhost:7153";

        _documentRepositoryMock
            .Setup(x => x.GetByIdAndOwnerIdAsync(documentId, ownerId))
            .ReturnsAsync((Document?)null);

        // Act
        var result = await _useCase.ExecuteAsync(documentId, ownerId, baseUrl);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnNull_WhenDocumentArchived()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var baseUrl = "https://localhost:7153";

        var document = new Document
        {
            Id = documentId,
            OwnerId = ownerId,
            IsArchived = true
        };

        _documentRepositoryMock
            .Setup(x => x.GetByIdAndOwnerIdAsync(documentId, ownerId))
            .ReturnsAsync(document);

        // Act
        var result = await _useCase.ExecuteAsync(documentId, ownerId, baseUrl);

        // Assert
        result.Should().BeNull();
    }
}
