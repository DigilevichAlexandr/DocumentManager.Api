using DocumentManager.Api.Application.DTOs.Requests;
using DocumentManager.Api.Application.DTOs.Responses;
using DocumentManager.Api.Application.Interfaces;
using DocumentManager.Api.Application.UseCases.Documents;
using DocumentManager.Api.Domain.Entities;
using DocumentManager.Api.Domain.Interfaces;
using DocumentManager.Api.Presentation.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;
using Xunit;

namespace DocumentManager.Api.Tests.Controllers;

public class DocumentsControllerTests
{
    private readonly Mock<IDocumentRepository> _documentRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IFileService> _fileServiceMock;
    private readonly Mock<ICommentRepository> _commentRepositoryMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IDocumentMetadataRepository> _metadataRepositoryMock;
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
    private readonly DocumentsController _controller;

    public DocumentsControllerTests()
    {
        _documentRepositoryMock = new Mock<IDocumentRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _fileServiceMock = new Mock<IFileService>();
        _commentRepositoryMock = new Mock<ICommentRepository>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _metadataRepositoryMock = new Mock<IDocumentMetadataRepository>();
        _httpContextAccessorMock = new Mock<IHttpContextAccessor>();

        var getMyDocumentsUseCase = new GetMyDocumentsUseCase(_documentRepositoryMock.Object);
        var createDocumentUseCase = new CreateDocumentUseCase(_documentRepositoryMock.Object, _unitOfWorkMock.Object, _fileServiceMock.Object);
        var updateDocumentUseCase = new UpdateDocumentUseCase(_documentRepositoryMock.Object, _unitOfWorkMock.Object);
        var deleteDocumentUseCase = new DeleteDocumentUseCase(_documentRepositoryMock.Object, _unitOfWorkMock.Object, _fileServiceMock.Object);
        var downloadDocumentFileUseCase = new DownloadDocumentFileUseCase(_documentRepositoryMock.Object, _fileServiceMock.Object);
        var getDocumentHistoryUseCase = new GetDocumentHistoryUseCase(_documentRepositoryMock.Object);
        var shareDocumentUseCase = new ShareDocumentUseCase(_documentRepositoryMock.Object, _unitOfWorkMock.Object);
        var addCommentUseCase = new AddCommentUseCase(_commentRepositoryMock.Object, _documentRepositoryMock.Object, _userRepositoryMock.Object, _unitOfWorkMock.Object);
        var getCommentsUseCase = new GetCommentsUseCase(_commentRepositoryMock.Object);
        var setMetadataUseCase = new SetMetadataUseCase(_metadataRepositoryMock.Object, _documentRepositoryMock.Object, _unitOfWorkMock.Object);
        var getUserStatisticsUseCase = new GetUserStatisticsUseCase(_documentRepositoryMock.Object, _commentRepositoryMock.Object);

        _controller = new DocumentsController(
            getMyDocumentsUseCase,
            createDocumentUseCase,
            updateDocumentUseCase,
            deleteDocumentUseCase,
            downloadDocumentFileUseCase,
            getDocumentHistoryUseCase,
            shareDocumentUseCase,
            addCommentUseCase,
            getCommentsUseCase,
            setMetadataUseCase,
            getUserStatisticsUseCase,
            _httpContextAccessorMock.Object);

        // Setup user claims
        var userId = Guid.NewGuid();
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) };
        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };
    }

    [Fact]
    public async Task GetMyDocuments_ShouldReturnOk_WhenValid()
    {
        // Arrange
        var userId = Guid.Parse(_controller.User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var documents = new List<Document>
        {
            new Document { Id = Guid.NewGuid(), Name = "Doc 1", OwnerId = userId, IsDeleted = false, IsArchived = false },
            new Document { Id = Guid.NewGuid(), Name = "Doc 2", OwnerId = userId, IsDeleted = false, IsArchived = false }
        };

        _documentRepositoryMock
            .Setup(x => x.GetByOwnerIdAsync(userId))
            .ReturnsAsync(documents);

        // Act
        var result = await _controller.GetMyDocuments();

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedDocuments = okResult.Value.Should().BeAssignableTo<IEnumerable<DocumentResponse>>().Subject;
        returnedDocuments.Should().NotBeNull();
        returnedDocuments!.Should().HaveCount(2);
    }

    [Fact]
    public async Task CreateDocument_ShouldReturnOk_WhenValid()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var document = new Document
        {
            Id = Guid.NewGuid(),
            Name = "Test Document",
            Description = "Test Description",
            OwnerId = userId,
            ExpirationAt = DateTime.UtcNow.AddDays(30)
        };

        _documentRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Document>()))
            .ReturnsAsync(document);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _controller.CreateDocument("Test Document", "Test Description", 30, null);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedDocument = okResult.Value.Should().BeOfType<DocumentResponse>().Subject;
        returnedDocument.Should().NotBeNull();
        returnedDocument!.Name.Should().Be("Test Document");
    }

    [Fact]
    public async Task DeleteDocument_ShouldReturnNoContent_WhenSuccess()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var userId = Guid.Parse(_controller.User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var document = new Document
        {
            Id = documentId,
            OwnerId = userId,
            FilePath = "/path/to/file"
        };

        _documentRepositoryMock
            .Setup(x => x.GetByIdAndOwnerIdAsync(documentId, userId))
            .ReturnsAsync(document);

        _fileServiceMock
            .Setup(x => x.DeleteFileAsync(It.IsAny<string>()))
            .ReturnsAsync(true);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _controller.DeleteDocument(documentId);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task DeleteDocument_ShouldReturnNotFound_WhenDocumentNotFound()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var userId = Guid.Parse(_controller.User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        _documentRepositoryMock
            .Setup(x => x.GetByIdAndOwnerIdAsync(documentId, userId))
            .ReturnsAsync((Document?)null);

        // Act
        var result = await _controller.DeleteDocument(documentId);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }
}
