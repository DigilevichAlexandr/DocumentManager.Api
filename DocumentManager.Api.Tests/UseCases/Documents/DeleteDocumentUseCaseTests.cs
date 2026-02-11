using DocumentManager.Api.Application.Interfaces;
using DocumentManager.Api.Application.UseCases.Documents;
using DocumentManager.Api.Domain.Entities;
using DocumentManager.Api.Domain.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace DocumentManager.Api.Tests.UseCases.Documents;

public class DeleteDocumentUseCaseTests
{
    private readonly Mock<IDocumentRepository> _documentRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IFileService> _fileServiceMock;
    private readonly DeleteDocumentUseCase _useCase;

    public DeleteDocumentUseCaseTests()
    {
        _documentRepositoryMock = new Mock<IDocumentRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _fileServiceMock = new Mock<IFileService>();
        _useCase = new DeleteDocumentUseCase(
            _documentRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _fileServiceMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldDeleteDocument_WhenValid()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var document = new Document
        {
            Id = documentId,
            OwnerId = ownerId,
            FilePath = "/path/to/file"
        };

        _documentRepositoryMock
            .Setup(x => x.GetByIdAndOwnerIdAsync(documentId, ownerId))
            .ReturnsAsync(document);

        _fileServiceMock
            .Setup(x => x.DeleteFileAsync(It.IsAny<string>()))
            .ReturnsAsync(true);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _useCase.ExecuteAsync(documentId, ownerId);

        // Assert
        result.Should().BeTrue();
        document.IsDeleted.Should().BeTrue();
        document.DeletedAt.Should().NotBeNull();
        _fileServiceMock.Verify(x => x.DeleteFileAsync(document.FilePath), Times.Once);
        _documentRepositoryMock.Verify(x => x.UpdateAsync(document), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnFalse_WhenDocumentNotFound()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();

        _documentRepositoryMock
            .Setup(x => x.GetByIdAndOwnerIdAsync(documentId, ownerId))
            .ReturnsAsync((Document?)null);

        // Act
        var result = await _useCase.ExecuteAsync(documentId, ownerId);

        // Assert
        result.Should().BeFalse();
    }
}
