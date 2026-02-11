using DocumentManager.Api.Application.DTOs.Responses;
using DocumentManager.Api.Application.Interfaces;
using DocumentManager.Api.Application.UseCases.Documents;
using DocumentManager.Api.Domain.Entities;
using DocumentManager.Api.Domain.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace DocumentManager.Api.Tests.UseCases.Documents;

public class AccessSharedDocumentUseCaseTests
{
    private readonly Mock<IDocumentRepository> _documentRepositoryMock;
    private readonly Mock<IDocumentMetadataRepository> _metadataRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IFileService> _fileServiceMock;
    private readonly AccessSharedDocumentUseCase _useCase;

    public AccessSharedDocumentUseCaseTests()
    {
        _documentRepositoryMock = new Mock<IDocumentRepository>();
        _metadataRepositoryMock = new Mock<IDocumentMetadataRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _fileServiceMock = new Mock<IFileService>();
        _useCase = new AccessSharedDocumentUseCase(
            _documentRepositoryMock.Object,
            _metadataRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _fileServiceMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldCopyDocumentAndDeleteOriginal()
    {
        // Arrange
        var shareToken = "test-token";
        var originalOwnerId = Guid.NewGuid();
        var newOwnerId = Guid.NewGuid();

        var originalDocument = new Document
        {
            Id = Guid.NewGuid(),
            OwnerId = originalOwnerId,
            Name = "Shared Document",
            Description = "Test",
            FileName = "test.pdf",
            FilePath = "/path/to/file",
            ShareToken = shareToken,
            IsDeleted = false,
            Metadata = new DocumentMetadata
            {
                Latitude = 55.7558,
                Longitude = 37.6173
            }
        };

        _documentRepositoryMock
            .Setup(x => x.GetByShareTokenAsync(shareToken))
            .ReturnsAsync(originalDocument);

        _fileServiceMock
            .Setup(x => x.CopyFileAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>()))
            .ReturnsAsync("/new/path/to/file");

        _documentRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Document>()))
            .ReturnsAsync((Document doc) => doc);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _useCase.ExecuteAsync(shareToken, newOwnerId);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be(originalDocument.Name);
        originalDocument.IsDeleted.Should().BeTrue();
        originalDocument.ShareToken.Should().BeNull();
        _documentRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Document>()), Times.Once);
        _documentRepositoryMock.Verify(x => x.UpdateAsync(originalDocument), Times.Once);
    }
}
