using DocumentManager.Api.Domain.Entities;
using DocumentManager.Api.Infrastructure.Data;
using DocumentManager.Api.Infrastructure.Data.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DocumentManager.Api.Tests.Repositories;

public class DocumentRepositoryTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly DocumentRepository _repository;

    public DocumentRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _repository = new DocumentRepository(_context);
    }

    [Fact]
    public async Task GetByOwnerIdAsync_ShouldReturnOnlyOwnerDocuments()
    {
        // Arrange
        var ownerId1 = Guid.NewGuid();
        var ownerId2 = Guid.NewGuid();

        var doc1 = new Document { Id = Guid.NewGuid(), OwnerId = ownerId1, Name = "Doc 1" };
        var doc2 = new Document { Id = Guid.NewGuid(), OwnerId = ownerId1, Name = "Doc 2" };
        var doc3 = new Document { Id = Guid.NewGuid(), OwnerId = ownerId2, Name = "Doc 3" };

        _context.Documents.AddRange(doc1, doc2, doc3);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByOwnerIdAsync(ownerId1);

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(d => d.OwnerId == ownerId1);
    }

    [Fact]
    public async Task GetByIdAndOwnerIdAsync_ShouldReturnDocument_WhenExists()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var document = new Document { Id = documentId, OwnerId = ownerId, Name = "Test Doc" };

        _context.Documents.Add(document);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAndOwnerIdAsync(documentId, ownerId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(documentId);
        result.OwnerId.Should().Be(ownerId);
    }

    [Fact]
    public async Task GetByIdAndOwnerIdAsync_ShouldReturnNull_WhenNotExists()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();

        // Act
        var result = await _repository.GetByIdAndOwnerIdAsync(documentId, ownerId);

        // Assert
        result.Should().BeNull();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
