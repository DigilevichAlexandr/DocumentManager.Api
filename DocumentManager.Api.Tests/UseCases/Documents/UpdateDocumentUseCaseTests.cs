using DocumentManager.Api.Application.DTOs.Requests;
using DocumentManager.Api.Application.DTOs.Responses;
using DocumentManager.Api.Application.UseCases.Documents;
using DocumentManager.Api.Domain.Entities;
using DocumentManager.Api.Domain.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace DocumentManager.Api.Tests.UseCases.Documents;

public class UpdateDocumentUseCaseTests
{
    private readonly Mock<IDocumentRepository> _documentRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly UpdateDocumentUseCase _useCase;

    public UpdateDocumentUseCaseTests()
    {
        _documentRepositoryMock = new Mock<IDocumentRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _useCase = new UpdateDocumentUseCase(_documentRepositoryMock.Object, _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldUpdateDocument_WhenValidRequest()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var request = new UpdateDocumentRequest
        {
            Name = "Updated Name",
            Description = "Updated Description",
            ExpirationDays = 60
        };

        var document = new Document
        {
            Id = documentId,
            Name = "Original Name",
            Description = "Original Description",
            OwnerId = ownerId,
            ExpirationAt = DateTime.UtcNow.AddDays(30),
            IsDeleted = false
        };

        _documentRepositoryMock
            .Setup(x => x.GetByIdAndOwnerIdAsync(documentId, ownerId))
            .ReturnsAsync(document);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _useCase.ExecuteAsync(documentId, ownerId, request);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be(request.Name);
        result.Description.Should().Be(request.Description);
        _documentRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Document>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnNull_WhenDocumentNotFound()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var request = new UpdateDocumentRequest
        {
            Name = "Updated Name",
            ExpirationDays = 60
        };

        _documentRepositoryMock
            .Setup(x => x.GetByIdAndOwnerIdAsync(documentId, ownerId))
            .ReturnsAsync((Document?)null);

        // Act
        var result = await _useCase.ExecuteAsync(documentId, ownerId, request);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnNull_WhenDocumentDeleted()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var request = new UpdateDocumentRequest
        {
            Name = "Updated Name",
            ExpirationDays = 60
        };

        var document = new Document
        {
            Id = documentId,
            OwnerId = ownerId,
            IsDeleted = true
        };

        _documentRepositoryMock
            .Setup(x => x.GetByIdAndOwnerIdAsync(documentId, ownerId))
            .ReturnsAsync(document);

        // Act
        var result = await _useCase.ExecuteAsync(documentId, ownerId, request);

        // Assert
        result.Should().BeNull();
    }
}
