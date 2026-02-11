namespace DocumentManager.Api.Application.DTOs.Requests;

public class RegisterRequest
{
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
}
