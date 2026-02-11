using DocumentManager.Api.Application.DTOs.Responses;
using DocumentManager.Api.Application.UseCases.Documents;
using DocumentManager.Api.Domain.Entities;
using DocumentManager.Api.Domain.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace DocumentManager.Api.Tests.UseCases.Documents;

public class GetUserStatisticsUseCaseTests
{
    private readonly Mock<IDocumentRepository> _documentRepositoryMock;
    private readonly Mock<ICommentRepository> _commentRepositoryMock;
    private readonly GetUserStatisticsUseCase _useCase;

    public GetUserStatisticsUseCaseTests()
    {
        _documentRepositoryMock = new Mock<IDocumentRepository>();
        _commentRepositoryMock = new Mock<ICommentRepository>();
        _useCase = new GetUserStatisticsUseCase(_documentRepositoryMock.Object, _commentRepositoryMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldCalculateStatistics()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var documents = new List<Document>
        {
            new Document
            {
                Id = Guid.NewGuid(),
                OwnerId = userId,
                CreatedAt = new DateTime(2024, 1, 1),
                IsArchived = false,
                IsDeleted = false,
                Comments = new List<Comment>
                {
                    new Comment { Id = Guid.NewGuid(), Text = "Comment 1" }
                }
            },
            new Document
            {
                Id = Guid.NewGuid(),
                OwnerId = userId,
                CreatedAt = new DateTime(2024, 2, 1),
                IsArchived = true,
                ArchivedAt = new DateTime(2024, 2, 15),
                IsDeleted = false
            }
        };

        _documentRepositoryMock
            .Setup(x => x.GetByOwnerIdAsync(userId))
            .ReturnsAsync(documents);

        _commentRepositoryMock
            .Setup(x => x.GetByDocumentIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(new List<Comment>());

        // Act
        var result = await _useCase.ExecuteAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result.TotalDocuments.Should().Be(2);
        result.ActiveDocuments.Should().Be(1);
        result.ArchivedDocuments.Should().Be(1);
        result.DocumentsByYear.Should().ContainKey(2024);
    }
}
