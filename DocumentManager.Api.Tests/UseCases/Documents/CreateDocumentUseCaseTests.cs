using DocumentManager.Api.Application.DTOs.Requests;
using DocumentManager.Api.Application.DTOs.Responses;
using DocumentManager.Api.Application.Interfaces;
using DocumentManager.Api.Application.UseCases.Documents;
using DocumentManager.Api.Domain.Entities;
using DocumentManager.Api.Domain.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace DocumentManager.Api.Tests.UseCases.Documents;

public class CreateDocumentUseCaseTests
{
    private readonly Mock<IDocumentRepository> _documentRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IFileService> _fileServiceMock;
    private readonly CreateDocumentUseCase _useCase;

    public CreateDocumentUseCaseTests()
    {
        _documentRepositoryMock = new Mock<IDocumentRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _fileServiceMock = new Mock<IFileService>();
        _useCase = new CreateDocumentUseCase(
            _documentRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _fileServiceMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldCreateDocument_WhenValidRequest()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var request = new CreateDocumentRequest
        {
            Name = "Test Document",
            Description = "Test Description",
            ExpirationDays = 30
        };

        var document = new Document
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            OwnerId = ownerId,
            ExpirationAt = DateTime.UtcNow.AddDays(request.ExpirationDays)
        };

        _documentRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Document>()))
            .ReturnsAsync(document);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _useCase.ExecuteAsync(request, ownerId);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be(request.Name);
        result.Description.Should().Be(request.Description);
        result.OwnerId.Should().Be(ownerId);
        _documentRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Document>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldSaveFile_WhenFileProvided()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var request = new CreateDocumentRequest
        {
            Name = "Test Document",
            Description = "Test Description",
            ExpirationDays = 30
        };

        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.FileName).Returns("test.pdf");
        fileMock.Setup(f => f.Length).Returns(1024L);
        fileMock.Setup(f => f.ContentType).Returns("application/pdf");
        fileMock.Setup(f => f.OpenReadStream()).Returns(() => new MemoryStream());

        var document = new Document
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            OwnerId = ownerId
        };

        _documentRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Document>()))
            .ReturnsAsync(document);

        _fileServiceMock
            .Setup(x => x.GetContentType(It.IsAny<string>()))
            .Returns("application/pdf");

        _fileServiceMock
            .Setup(x => x.SaveFileAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>()))
            .ReturnsAsync("/path/to/file");

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _useCase.ExecuteAsync(request, ownerId, fileMock.Object);

        // Assert
        result.Should().NotBeNull();
        _fileServiceMock.Verify(x => x.SaveFileAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Once);
    }
}
