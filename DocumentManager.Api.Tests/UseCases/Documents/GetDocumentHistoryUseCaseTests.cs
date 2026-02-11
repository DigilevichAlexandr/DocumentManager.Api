using DocumentManager.Api.Application.DTOs.Responses;
using DocumentManager.Api.Application.UseCases.Documents;
using DocumentManager.Api.Domain.Entities;
using DocumentManager.Api.Domain.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace DocumentManager.Api.Tests.UseCases.Documents;

public class GetDocumentHistoryUseCaseTests
{
    private readonly Mock<IDocumentRepository> _documentRepositoryMock;
    private readonly GetDocumentHistoryUseCase _useCase;

    public GetDocumentHistoryUseCaseTests()
    {
        _documentRepositoryMock = new Mock<IDocumentRepository>();
        _useCase = new GetDocumentHistoryUseCase(_documentRepositoryMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnOnlyArchivedDocuments()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var documents = new List<Document>
        {
            new Document { Id = Guid.NewGuid(), Name = "Archived Doc 1", OwnerId = ownerId, IsArchived = true },
            new Document { Id = Guid.NewGuid(), Name = "Archived Doc 2", OwnerId = ownerId, IsArchived = true },
            new Document { Id = Guid.NewGuid(), Name = "Active Doc", OwnerId = ownerId, IsArchived = false }
        };

        _documentRepositoryMock
            .Setup(x => x.GetArchivedByOwnerIdAsync(ownerId))
            .ReturnsAsync(documents.Where(d => d.IsArchived).ToList());

        // Act
        var result = await _useCase.ExecuteAsync(ownerId);

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(d => d.IsArchived);
    }
}
