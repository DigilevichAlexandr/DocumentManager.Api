using DocumentManager.Ui.Models;
using DocumentManager.Ui.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;
using System.Text.Json;

namespace DocumentManager.Ui.Pages;

public class LoginModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly AuthState _authState;

    public LoginModel(IHttpClientFactory httpClientFactory, AuthState authState)
    {
        _httpClientFactory = httpClientFactory;
        _authState = authState;
    }

    [BindProperty]
    public LoginRequest Model { get; set; } = new();

    public bool IsSubmitting { get; set; }

    public IActionResult OnGet()
    {
        if (_authState.IsAuthenticated)
        {
            return RedirectToPage("/Index");
        }
        return Page();
    }

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
            var response = await httpClient.PostAsJsonAsync("api/auth/login", Model);

            if (!response.IsSuccessStatusCode)
            {
                var errorText = await response.Content.ReadAsStringAsync();
                TempData["ErrorMessage"] = $"Неверный email или пароль. Статус: {(int)response.StatusCode}";
                return Page();
            }

            var json = await response.Content.ReadAsStringAsync();
            var data = JsonSerializer.Deserialize<LoginResponse>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (data == null || string.IsNullOrWhiteSpace(data.Token))
            {
                TempData["ErrorMessage"] = "Не удалось получить токен.";
                return Page();
            }

            _authState.SetToken(data.Token);
            TempData["SuccessMessage"] = "Успешный вход! Добро пожаловать!";
            return RedirectToPage("/Index");
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
