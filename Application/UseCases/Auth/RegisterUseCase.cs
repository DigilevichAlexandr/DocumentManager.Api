using DocumentManager.Api.Domain.Entities;
using DocumentManager.Api.Domain.Interfaces;

namespace DocumentManager.Api.Application.UseCases.Auth;

public class RegisterUseCase
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RegisterUseCase(IUserRepository userRepository, IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<(bool Success, string? ErrorMessage)> ExecuteAsync(string email, string password)
    {
        if (await _userRepository.ExistsByEmailAsync(email))
            return (false, "User already exists");

        var user = new User
        {
            Email = email,
            PasswordHash = password 
        };

        await _userRepository.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        return (true, null);
    }
}
