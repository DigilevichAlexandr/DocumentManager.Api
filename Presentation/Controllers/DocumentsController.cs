using DocumentManager.Api.Application.DTOs.Requests;
using DocumentManager.Api.Application.DTOs.Responses;
using DocumentManager.Api.Application.UseCases.Documents;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DocumentManager.Api.Presentation.Controllers;

[ApiController]
[Route("api/documents")]
[Authorize]
public class DocumentsController : ControllerBase
{
    private readonly GetMyDocumentsUseCase _getMyDocumentsUseCase;
    private readonly CreateDocumentUseCase _createDocumentUseCase;
    private readonly UpdateDocumentUseCase _updateDocumentUseCase;
    private readonly DeleteDocumentUseCase _deleteDocumentUseCase;
    private readonly DownloadDocumentFileUseCase _downloadDocumentFileUseCase;
    private readonly GetDocumentHistoryUseCase _getDocumentHistoryUseCase;
    private readonly ShareDocumentUseCase _shareDocumentUseCase;
    private readonly AddCommentUseCase _addCommentUseCase;
    private readonly GetCommentsUseCase _getCommentsUseCase;
    private readonly SetMetadataUseCase _setMetadataUseCase;
    private readonly GetUserStatisticsUseCase _getUserStatisticsUseCase;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public DocumentsController(
        GetMyDocumentsUseCase getMyDocumentsUseCase,
        CreateDocumentUseCase createDocumentUseCase,
        UpdateDocumentUseCase updateDocumentUseCase,
        DeleteDocumentUseCase deleteDocumentUseCase,
        DownloadDocumentFileUseCase downloadDocumentFileUseCase,
        GetDocumentHistoryUseCase getDocumentHistoryUseCase,
        ShareDocumentUseCase shareDocumentUseCase,
        AddCommentUseCase addCommentUseCase,
        GetCommentsUseCase getCommentsUseCase,
        SetMetadataUseCase setMetadataUseCase,
        GetUserStatisticsUseCase getUserStatisticsUseCase,
        IHttpContextAccessor httpContextAccessor)
    {
        _getMyDocumentsUseCase = getMyDocumentsUseCase;
        _createDocumentUseCase = createDocumentUseCase;
        _updateDocumentUseCase = updateDocumentUseCase;
        _deleteDocumentUseCase = deleteDocumentUseCase;
        _downloadDocumentFileUseCase = downloadDocumentFileUseCase;
        _getDocumentHistoryUseCase = getDocumentHistoryUseCase;
        _shareDocumentUseCase = shareDocumentUseCase;
        _addCommentUseCase = addCommentUseCase;
        _getCommentsUseCase = getCommentsUseCase;
        _setMetadataUseCase = setMetadataUseCase;
        _getUserStatisticsUseCase = getUserStatisticsUseCase;
        _httpContextAccessor = httpContextAccessor;
    }

    [HttpGet("my")]
    public async Task<ActionResult<IEnumerable<DocumentResponse>>> GetMyDocuments()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized("Не удалось определить пользователя из токена");
        }

        try
        {
            var documents = await _getMyDocumentsUseCase.ExecuteAsync(userId);
            return Ok(documents);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Ошибка при загрузке документов: {ex.Message}");
        }
    }

    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<DocumentResponse>> CreateDocument(
        [FromForm] string name,
        [FromForm] string? description,
        [FromForm] int expirationDays,
        [FromForm] IFormFile? file)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized("Не удалось определить пользователя из токена");
        }
        
        var request = new CreateDocumentRequest
        {
            Name = name,
            Description = description,
            ExpirationDays = expirationDays
        };

        try
        {
            var document = await _createDocumentUseCase.ExecuteAsync(request, userId, file);
            return Ok(document);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Ошибка при создании документа: {ex.Message}");
        }
    }

    [HttpGet("{id}/download")]
    public async Task<IActionResult> DownloadFile(Guid id)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _downloadDocumentFileUseCase.ExecuteAsync(id, userId);
        
        if (result == null)
            return NotFound();

        var (fileStream, fileName, contentType) = result.Value;
        
        if (fileStream == null)
            return NotFound();
        
        return File(fileStream, contentType ?? "application/octet-stream", fileName ?? "file");
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<DocumentResponse>> UpdateDocument(Guid id, [FromBody] UpdateDocumentRequest request)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized("Не удалось определить пользователя из токена");
        }

        try
        {
            var document = await _updateDocumentUseCase.ExecuteAsync(id, userId, request);
            if (document == null)
                return NotFound();
            
            return Ok(document);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Ошибка при обновлении документа: {ex.Message}");
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteDocument(Guid id)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var deleted = await _deleteDocumentUseCase.ExecuteAsync(id, userId);
        
        if (!deleted)
            return NotFound();
            
        return NoContent();
    }

    [HttpGet("history")]
    public async Task<ActionResult<IEnumerable<DocumentResponse>>> GetHistory()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized("Не удалось определить пользователя из токена");
        }

        try
        {
            var documents = await _getDocumentHistoryUseCase.ExecuteAsync(userId);
            return Ok(documents);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Ошибка при загрузке истории: {ex.Message}");
        }
    }

    [HttpPost("{id}/share")]
    public async Task<ActionResult<ShareLinkResponse>> ShareDocument(Guid id)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized("Не удалось определить пользователя из токена");
        }

        try
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var result = await _shareDocumentUseCase.ExecuteAsync(id, userId, baseUrl);
            if (result == null)
                return NotFound();
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Ошибка при создании ссылки для шаринга: {ex.Message}");
        }
    }

    [HttpPost("{id}/comments")]
    public async Task<ActionResult<CommentResponse>> AddComment(Guid id, [FromBody] AddCommentRequest request)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized("Не удалось определить пользователя из токена");
        }

        try
        {
            var comment = await _addCommentUseCase.ExecuteAsync(id, userId, request);
            if (comment == null)
                return NotFound();
            
            return Ok(comment);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Ошибка при добавлении комментария: {ex.Message}");
        }
    }

    [HttpGet("{id}/comments")]
    public async Task<ActionResult<IEnumerable<CommentResponse>>> GetComments(Guid id)
    {
        try
        {
            var comments = await _getCommentsUseCase.ExecuteAsync(id);
            return Ok(comments);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Ошибка при загрузке комментариев: {ex.Message}");
        }
    }

    [HttpPost("{id}/metadata")]
    public async Task<ActionResult<DocumentMetadataResponse>> SetMetadata(Guid id, [FromBody] SetMetadataRequest request)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized("Не удалось определить пользователя из токена");
        }

        try
        {
            var metadata = await _setMetadataUseCase.ExecuteAsync(id, userId, request);
            if (metadata == null)
                return NotFound();
            
            return Ok(metadata);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Ошибка при установке метаданных: {ex.Message}");
        }
    }

    [HttpGet("statistics")]
    public async Task<ActionResult<UserStatisticsResponse>> GetStatistics([FromQuery] int? year)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized("Не удалось определить пользователя из токена");
        }

        try
        {
            var statistics = await _getUserStatisticsUseCase.ExecuteAsync(userId, year);
            return Ok(statistics);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Ошибка при загрузке статистики: {ex.Message}");
        }
    }
}
