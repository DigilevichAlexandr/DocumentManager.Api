using DocumentManager.Api.Application.UseCases.Documents;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DocumentManager.Api.Presentation.Controllers;

[ApiController]
[Route("api/documents/share")]
public class ShareController : ControllerBase
{
    private readonly AccessSharedDocumentUseCase _accessSharedDocumentUseCase;

    public ShareController(AccessSharedDocumentUseCase accessSharedDocumentUseCase)
    {
        _accessSharedDocumentUseCase = accessSharedDocumentUseCase;
    }

    [HttpGet("{token}")]
    public async Task<IActionResult> AccessSharedDocument(string token)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        Guid userId;
        
        if (string.IsNullOrWhiteSpace(userIdClaim) || !Guid.TryParse(userIdClaim, out userId))
        {
            userId = Guid.NewGuid();
        }

        try
        {
            var document = await _accessSharedDocumentUseCase.ExecuteAsync(token, userId);
            if (document == null)
                return NotFound("Документ не найден или уже был удален");
            
            return Ok(new { message = "Документ успешно скопирован в ваши документы", document });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Ошибка при доступе к документу: {ex.Message}");
        }
    }
}
