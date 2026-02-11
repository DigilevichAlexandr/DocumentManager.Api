using DocumentManager.Api.Application.DTOs.Requests;
using DocumentManager.Api.Application.DTOs.Responses;
using DocumentManager.Api.Application.UseCases.Documents;
using DocumentManager.Api.Domain.Entities;
using DocumentManager.Api.Domain.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace DocumentManager.Api.Tests.UseCases.Documents;

public class SetMetadataUseCaseTests
{
    private readonly Mock<IDocumentMetadataRepository> _metadataRepositoryMock;
    private readonly Mock<IDocumentRepository> _documentRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly SetMetadataUseCase _useCase;

    public SetMetadataUseCaseTests()
    {
        _metadataRepositoryMock = new Mock<IDocumentMetadataRepository>();
        _documentRepositoryMock = new Mock<IDocumentRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _useCase = new SetMetadataUseCase(
            _metadataRepositoryMock.Object,
            _documentRepositoryMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldCreateMetadata_WhenNotExists()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var request = new SetMetadataRequest
        {
            Latitude = 55.7558,
            Longitude = 37.6173,
            LocationName = "Moscow"
        };

        var document = new Document
        {
            Id = documentId,
            OwnerId = ownerId,
            IsDeleted = false
        };

        var metadata = new DocumentMetadata
        {
            Id = Guid.NewGuid(),
            DocumentId = documentId,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            LocationName = request.LocationName
        };

        _documentRepositoryMock
            .Setup(x => x.GetByIdAndOwnerIdAsync(documentId, ownerId))
            .ReturnsAsync(document);

        _metadataRepositoryMock
            .SetupSequence(x => x.GetByDocumentIdAsync(documentId))
            .ReturnsAsync((DocumentMetadata?)null)
            .ReturnsAsync(metadata);

        _metadataRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<DocumentMetadata>()))
            .ReturnsAsync((DocumentMetadata m) => { m.Id = metadata.Id; return m; });

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _useCase.ExecuteAsync(documentId, ownerId, request);

        // Assert
        result.Should().NotBeNull();
        result!.Latitude.Should().Be(request.Latitude);
        result.Longitude.Should().Be(request.Longitude);
        result.LocationName.Should().Be(request.LocationName);
        _metadataRepositoryMock.Verify(x => x.AddAsync(It.IsAny<DocumentMetadata>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldUpdateMetadata_WhenExists()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var request = new SetMetadataRequest
        {
            Latitude = 55.7558,
            Longitude = 37.6173,
            LocationName = "Updated Location"
        };

        var document = new Document
        {
            Id = documentId,
            OwnerId = ownerId,
            IsDeleted = false
        };

        var existingMetadata = new DocumentMetadata
        {
            Id = Guid.NewGuid(),
            DocumentId = documentId,
            Latitude = 0,
            Longitude = 0
        };

        _documentRepositoryMock
            .Setup(x => x.GetByIdAndOwnerIdAsync(documentId, ownerId))
            .ReturnsAsync(document);

        _metadataRepositoryMock
            .Setup(x => x.GetByDocumentIdAsync(documentId))
            .ReturnsAsync(existingMetadata);

        _metadataRepositoryMock
            .Setup(x => x.GetByDocumentIdAsync(documentId))
            .ReturnsAsync(existingMetadata);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _useCase.ExecuteAsync(documentId, ownerId, request);

        // Assert
        result.Should().NotBeNull();
        existingMetadata.Latitude.Should().Be(request.Latitude);
        existingMetadata.Longitude.Should().Be(request.Longitude);
        _metadataRepositoryMock.Verify(x => x.UpdateAsync(existingMetadata), Times.Once);
    }
}
