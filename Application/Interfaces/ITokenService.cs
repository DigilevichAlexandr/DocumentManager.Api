using DocumentManager.Api.Domain.Entities;

namespace DocumentManager.Api.Application.Interfaces;

public interface ITokenService
{
    string GenerateToken(User user, int expiresInHours = 2);
}
