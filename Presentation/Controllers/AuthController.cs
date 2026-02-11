using DocumentManager.Api.Application.DTOs.Requests;
using DocumentManager.Api.Application.DTOs.Responses;
using DocumentManager.Api.Application.UseCases.Auth;
using Microsoft.AspNetCore.Mvc;

namespace DocumentManager.Api.Presentation.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly RegisterUseCase _registerUseCase;
    private readonly LoginUseCase _loginUseCase;

    public AuthController(RegisterUseCase registerUseCase, LoginUseCase loginUseCase)
    {
        _registerUseCase = registerUseCase;
        _loginUseCase = loginUseCase;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var (success, errorMessage) = await _registerUseCase.ExecuteAsync(request.Email, request.Password);
        
        if (!success)
            return BadRequest(errorMessage);

        return Ok();
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        var response = await _loginUseCase.ExecuteAsync(request.Email, request.Password);
        
        if (response == null)
            return Unauthorized();

        return Ok(response);
    }
}
