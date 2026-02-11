using Microsoft.AspNetCore.Http;

namespace DocumentManager.Ui.Services;

public class AuthState
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private const string TokenKey = "AuthToken";

    public AuthState(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? Token
    {
        get => _httpContextAccessor.HttpContext?.Session.GetString(TokenKey);
        set
        {
            if (_httpContextAccessor.HttpContext != null)
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    _httpContextAccessor.HttpContext.Session.Remove(TokenKey);
                }
                else
                {
                    _httpContextAccessor.HttpContext.Session.SetString(TokenKey, value);
                }
            }
        }
    }

    public bool IsAuthenticated => !string.IsNullOrWhiteSpace(Token);

    public void SetToken(string? token)
    {
        Token = token;
    }

    public void Logout()
    {
        Token = null;
    }
}

