using DocumentManager.Api.Application.DTOs.Responses;
using DocumentManager.Api.Application.Interfaces;
using DocumentManager.Api.Domain.Interfaces;

namespace DocumentManager.Api.Application.UseCases.Auth;

public class LoginUseCase
{
    private readonly IUserRepository _userRepository;
    private readonly ITokenService _tokenService;

    public LoginUseCase(IUserRepository userRepository, ITokenService tokenService)
    {
        _userRepository = userRepository;
        _tokenService = tokenService;
    }

    public async Task<LoginResponse?> ExecuteAsync(string email, string password)
    {
        var user = await _userRepository.GetByEmailAsync(email);
        
        if (user == null || user.PasswordHash != password)
            return null;

        var token = _tokenService.GenerateToken(user);
        
        return new LoginResponse { Token = token };
    }
}
