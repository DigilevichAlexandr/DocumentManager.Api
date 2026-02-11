using DocumentManager.Api.Application.DTOs.Responses;
using DocumentManager.Api.Application.Interfaces;
using DocumentManager.Api.Application.UseCases.Auth;
using DocumentManager.Api.Domain.Entities;
using DocumentManager.Api.Domain.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace DocumentManager.Api.Tests.UseCases.Auth;

public class LoginUseCaseTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<ITokenService> _tokenServiceMock;
    private readonly LoginUseCase _useCase;

    public LoginUseCaseTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _tokenServiceMock = new Mock<ITokenService>();
        _useCase = new LoginUseCase(_userRepositoryMock.Object, _tokenServiceMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnToken_WhenValidCredentials()
    {
        // Arrange
        var email = "test@example.com";
        var password = "password123";
        var passwordHash = "password123"; // В реальном приложении это должен быть хеш
        var token = "test-token";

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            PasswordHash = passwordHash
        };

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(email))
            .ReturnsAsync(user);

        _tokenServiceMock
            .Setup(x => x.GenerateToken(user, It.IsAny<int>()))
            .Returns(token);

        // Act
        var result = await _useCase.ExecuteAsync(email, password);

        // Assert
        result.Should().NotBeNull();
        result!.Token.Should().Be(token);
        _tokenServiceMock.Verify(x => x.GenerateToken(user, It.IsAny<int>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnNull_WhenUserNotFound()
    {
        // Arrange
        var email = "test@example.com";
        var password = "password123";

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(email))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _useCase.ExecuteAsync(email, password);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnNull_WhenInvalidPassword()
    {
        // Arrange
        var email = "test@example.com";
        var password = "wrong-password";
        var passwordHash = "correct-hash";

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            PasswordHash = passwordHash
        };

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(email))
            .ReturnsAsync(user);

        // Act
        var result = await _useCase.ExecuteAsync(email, password);

        // Assert
        result.Should().BeNull();
    }
}
