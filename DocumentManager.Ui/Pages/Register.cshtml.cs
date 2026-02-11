using DocumentManager.Ui.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DocumentManager.Ui.Pages;

public class RegisterModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;

    public RegisterModel(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    [BindProperty]
    public RegisterRequest Model { get; set; } = new();

    public bool IsSubmitting { get; set; }

    public async Task<IActionResult> OnPostAsync()
    {
        if (string.IsNullOrWhiteSpace(Model.Email) || string.IsNullOrWhiteSpace(Model.Password))
        {
            TempData["ErrorMessage"] = "Пожалуйста, заполните все поля.";
            return Page();
        }

        IsSubmitting = true;

        try
        {
            var httpClient = _httpClientFactory.CreateClient("Api");
            var response = await httpClient.PostAsJsonAsync("api/auth/register", Model);

            if (!response.IsSuccessStatusCode)
            {
                var text = await response.Content.ReadAsStringAsync();
                var errorMessage = string.IsNullOrWhiteSpace(text)
                    ? $"Ошибка регистрации. Статус: {(int)response.StatusCode}"
                    : text;
                TempData["ErrorMessage"] = errorMessage;
                return Page();
            }

            TempData["SuccessMessage"] = "Регистрация прошла успешно! Теперь вы можете войти.";
            Model.Email = string.Empty;
            Model.Password = string.Empty;
        }
        catch (HttpRequestException ex)
        {
            TempData["ErrorMessage"] = $"Ошибка подключения к серверу: {ex.Message}. Убедитесь, что API запущен.";
        }
        catch (TaskCanceledException)
        {
            TempData["ErrorMessage"] = "Превышено время ожидания. Проверьте подключение к серверу.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Ошибка: {ex.Message}";
        }
        finally
        {
            IsSubmitting = false;
        }

        return Page();
    }
}
