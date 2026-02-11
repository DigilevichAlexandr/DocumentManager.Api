using DocumentManager.Api.Application.Interfaces;
using DocumentManager.Api.Application.UseCases.Auth;
using DocumentManager.Api.Domain.Entities;
using DocumentManager.Api.Domain.Interfaces;
using DocumentManager.Api.Infrastructure.Services;
using FluentAssertions;
using Moq;
using Xunit;

namespace DocumentManager.Api.Tests.UseCases.Auth;

public class RegisterUseCaseTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly IPasswordHasher _passwordHasher;
    private readonly RegisterUseCase _useCase;

    public RegisterUseCaseTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _passwordHasher = new PasswordHasher();
        _useCase = new RegisterUseCase(_userRepositoryMock.Object, _unitOfWorkMock.Object, _passwordHasher);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldCreateUser_WhenValidRequest()
    {
        // Arrange
        var email = "test@example.com";
        var password = "password123";

        _userRepositoryMock
            .Setup(x => x.ExistsByEmailAsync(email))
            .ReturnsAsync(false);

        _userRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<User>()))
            .ReturnsAsync((User u) => u);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _useCase.ExecuteAsync(email, password);

        // Assert
        result.Success.Should().BeTrue();
        result.ErrorMessage.Should().BeNull();
        _userRepositoryMock.Verify(x => x.AddAsync(It.Is<User>(u => 
            u.Email == email && 
            _passwordHasher.VerifyPassword(password, u.PasswordHash))), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnError_WhenUserAlreadyExists()
    {
        // Arrange
        var email = "test@example.com";
        var password = "password123";

        _userRepositoryMock
            .Setup(x => x.ExistsByEmailAsync(email))
            .ReturnsAsync(true);

        // Act
        var result = await _useCase.ExecuteAsync(email, password);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be("User already exists");
        _userRepositoryMock.Verify(x => x.AddAsync(It.IsAny<User>()), Times.Never);
    }
}
