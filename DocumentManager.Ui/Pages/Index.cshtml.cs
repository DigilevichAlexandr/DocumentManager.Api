using DocumentManager.Ui.Models;
using DocumentManager.Ui.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Net.Http.Headers;
using System.Text.Json;

namespace DocumentManager.Ui.Pages;

public class IndexModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly AuthState _authState;
    private readonly IConfiguration _configuration;

    public IndexModel(IHttpClientFactory httpClientFactory, AuthState authState, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _authState = authState;
        _configuration = configuration;
    }

    public List<DocumentResponse> Documents { get; set; } = new();
    public bool IsLoading { get; set; }
    public string? Error { get; set; }
    public string? SuccessMessage { get; set; }
    public string? AuthToken => _authState.Token;
    public string ApiBaseUrl => _configuration["ApiBaseUrl"] ?? "https://localhost:7153";

    [BindProperty]
    public CreateDocumentForm CreateModel { get; set; } = new() { ExpirationDays = 30 };

    public async Task OnGetAsync()
    {
        ViewData["IsAuthenticated"] = _authState.IsAuthenticated;
        SuccessMessage = TempData["SuccessMessage"]?.ToString();
        if (_authState.IsAuthenticated)
        {
            await LoadDocumentsAsync();
        }
    }

    public async Task<IActionResult> OnPostAsync(IFormFile? file)
    {
        ViewData["IsAuthenticated"] = _authState.IsAuthenticated;

        if (!_authState.IsAuthenticated)
        {
            Error = "Необходимо выполнить вход.";
            ModelState.AddModelError("", Error);
            return Page();
        }

        // Проверяем валидность вручную, так как для multipart/form-data ModelState может не работать корректно
        if (string.IsNullOrWhiteSpace(CreateModel.Name))
        {
            ModelState.AddModelError(nameof(CreateModel.Name), "Название документа обязательно.");
        }
        
        if (CreateModel.ExpirationDays < 1 || CreateModel.ExpirationDays > 365)
        {
            ModelState.AddModelError(nameof(CreateModel.ExpirationDays), "Количество дней должно быть от 1 до 365.");
        }

        if (!ModelState.IsValid)
        {
            Error = "Пожалуйста, исправьте ошибки в форме.";
            return Page();
        }

        IsLoading = true;
        Error = null;

        try
        {
            var httpClient = _httpClientFactory.CreateClient("Api");
            using var content = new MultipartFormDataContent();

            content.Add(new StringContent(CreateModel.Name), "name");
            if (!string.IsNullOrWhiteSpace(CreateModel.Description))
            {
                content.Add(new StringContent(CreateModel.Description), "description");
            }
            content.Add(new StringContent(CreateModel.ExpirationDays.ToString()), "expirationDays");

            if (file != null && file.Length > 0)
            {
                var stream = file.OpenReadStream();
                var fileContent = new StreamContent(stream);
                fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType ?? "application/octet-stream");
                content.Add(fileContent, "file", file.FileName);
            }

            using var request = new HttpRequestMessage(HttpMethod.Post, "api/documents")
            {
                Content = content
            };
            
            if (string.IsNullOrWhiteSpace(_authState.Token))
            {
                Error = "Токен авторизации отсутствует. Пожалуйста, выполните вход снова.";
                return Page();
            }
            
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _authState.Token);

            var response = await httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Error = $"Ошибка создания документа: {(int)response.StatusCode}. {errorContent}";
                return Page();
            }

            TempData["SuccessMessage"] = "Документ успешно создан!";
            CreateModel = new CreateDocumentForm { ExpirationDays = 30 };
            await LoadDocumentsAsync();
            return RedirectToPage("/Index");
        }
        catch (HttpRequestException ex)
        {
            Error = $"Ошибка подключения к серверу: {ex.Message}. Убедитесь, что API запущен.";
        }
        catch (Exception ex)
        {
            Error = $"Ошибка: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }

        return Page();
    }

    private async Task LoadDocumentsAsync()
    {
        IsLoading = true;
        Error = null;

        try
        {
            var httpClient = _httpClientFactory.CreateClient("Api");
            using var request = new HttpRequestMessage(HttpMethod.Get, "api/documents/my");
            
            if (string.IsNullOrWhiteSpace(_authState.Token))
            {
                Error = "Токен авторизации отсутствует. Пожалуйста, выполните вход снова.";
                return;
            }
            
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _authState.Token);

            var response = await httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    Error = "Сессия истекла. Пожалуйста, выполните вход снова.";
                    _authState.Logout();
                }
                else
                {
                    Error = $"Ошибка загрузки документов: {(int)response.StatusCode}. {errorContent}";
                }
                return;
            }

            var json = await response.Content.ReadAsStringAsync();
            Documents = JsonSerializer.Deserialize<List<DocumentResponse>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new List<DocumentResponse>();
        }
        catch (HttpRequestException ex)
        {
            Error = $"Ошибка подключения к серверу: {ex.Message}. Убедитесь, что API запущен.";
        }
        catch (Exception ex)
        {
            Error = $"Ошибка: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }
}

public class CreateDocumentForm
{
    [Required(ErrorMessage = "Название обязательно")]
    public string Name { get; set; } = string.Empty;
    
    public string? Description { get; set; }
    
    [Range(1, 365, ErrorMessage = "Количество дней должно быть от 1 до 365")]
    public int ExpirationDays { get; set; }
}
