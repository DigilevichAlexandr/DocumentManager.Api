using DocumentManager.Api.Application.Interfaces;
using DocumentManager.Api.Domain.Entities;
using DocumentManager.Api.Domain.Interfaces;

namespace DocumentManager.Api.Application.UseCases.Auth;

public class RegisterUseCase
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;

    public RegisterUseCase(
        IUserRepository userRepository, 
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
    }

    public async Task<(bool Success, string? ErrorMessage)> ExecuteAsync(string email, string password)
    {
        if (await _userRepository.ExistsByEmailAsync(email))
            return (false, "User already exists");

        var user = new User
        {
            Email = email,
            PasswordHash = _passwordHasher.HashPassword(password)
        };

        await _userRepository.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        return (true, null);
    }
}
