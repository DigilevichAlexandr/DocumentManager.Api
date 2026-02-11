using DocumentManager.Api.Application.DTOs.Responses;
using DocumentManager.Api.Application.UseCases.Documents;
using DocumentManager.Api.Domain.Entities;
using DocumentManager.Api.Domain.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace DocumentManager.Api.Tests.UseCases.Documents;

public class GetMyDocumentsUseCaseTests
{
    private readonly Mock<IDocumentRepository> _documentRepositoryMock;
    private readonly GetMyDocumentsUseCase _useCase;

    public GetMyDocumentsUseCaseTests()
    {
        _documentRepositoryMock = new Mock<IDocumentRepository>();
        _useCase = new GetMyDocumentsUseCase(_documentRepositoryMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnOnlyActiveDocuments()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var documents = new List<Document>
        {
            new Document { Id = Guid.NewGuid(), Name = "Active Doc", OwnerId = ownerId, IsDeleted = false, IsArchived = false },
            new Document { Id = Guid.NewGuid(), Name = "Deleted Doc", OwnerId = ownerId, IsDeleted = true, IsArchived = false },
            new Document { Id = Guid.NewGuid(), Name = "Archived Doc", OwnerId = ownerId, IsDeleted = false, IsArchived = true }
        };

        _documentRepositoryMock
            .Setup(x => x.GetByOwnerIdAsync(ownerId))
            .ReturnsAsync(documents);

        // Act
        var result = await _useCase.ExecuteAsync(ownerId);

        // Assert
        result.Should().HaveCount(1);
        result.First().Name.Should().Be("Active Doc");
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnEmptyList_WhenNoDocuments()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        _documentRepositoryMock
            .Setup(x => x.GetByOwnerIdAsync(ownerId))
            .ReturnsAsync(new List<Document>());

        // Act
        var result = await _useCase.ExecuteAsync(ownerId);

        // Assert
        result.Should().BeEmpty();
    }
}
