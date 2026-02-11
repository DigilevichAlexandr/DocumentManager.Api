using DocumentManager.Ui.Models;
using DocumentManager.Ui.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace DocumentManager.Ui.Pages;

public class AuthModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly AuthState _authState;

    public AuthModel(IHttpClientFactory httpClientFactory, AuthState authState)
    {
        _httpClientFactory = httpClientFactory;
        _authState = authState;
    }

    [BindProperty]
    public LoginRequest LoginModel { get; set; } = new();

    [BindProperty]
    public RegisterRequest RegisterModel { get; set; } = new();

    public bool IsSubmittingLogin { get; set; }
    public bool IsSubmittingRegister { get; set; }
    public string ActiveTab { get; set; } = "login";

    public IActionResult OnGet(string? tab = null)
    {
        if (_authState.IsAuthenticated)
        {
            return RedirectToPage("/Index");
        }

        ActiveTab = tab == "register" ? "register" : "login";
        return Page();
    }

    public async Task<IActionResult> OnPostLoginAsync()
    {
        ActiveTab = "login";

        if (string.IsNullOrWhiteSpace(LoginModel.Email) || string.IsNullOrWhiteSpace(LoginModel.Password))
        {
            TempData["LoginErrorMessage"] = "Пожалуйста, заполните все поля.";
            return Page();
        }

        IsSubmittingLogin = true;

        try
        {
            var httpClient = _httpClientFactory.CreateClient("Api");
            var response = await httpClient.PostAsJsonAsync("api/auth/login", LoginModel);

            if (!response.IsSuccessStatusCode)
            {
                var errorText = await response.Content.ReadAsStringAsync();
                TempData["LoginErrorMessage"] = $"Неверный email или пароль. Статус: {(int)response.StatusCode}";
                return Page();
            }

            var json = await response.Content.ReadAsStringAsync();
            var data = JsonSerializer.Deserialize<LoginResponse>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (data == null || string.IsNullOrWhiteSpace(data.Token))
            {
                TempData["LoginErrorMessage"] = "Не удалось получить токен.";
                return Page();
            }

            _authState.SetToken(data.Token);
            TempData["LoginSuccessMessage"] = "Успешный вход! Добро пожаловать!";
            return RedirectToPage("/Index");
        }
        catch (HttpRequestException ex)
        {
            TempData["LoginErrorMessage"] = $"Ошибка подключения к серверу: {ex.Message}. Убедитесь, что API запущен.";
        }
        catch (TaskCanceledException)
        {
            TempData["LoginErrorMessage"] = "Превышено время ожидания. Проверьте подключение к серверу.";
        }
        catch (Exception ex)
        {
            TempData["LoginErrorMessage"] = $"Ошибка: {ex.Message}";
        }
        finally
        {
            IsSubmittingLogin = false;
        }

        return Page();
    }

    public async Task<IActionResult> OnPostRegisterAsync()
    {
        ActiveTab = "register";

        if (string.IsNullOrWhiteSpace(RegisterModel.Email) || string.IsNullOrWhiteSpace(RegisterModel.Password))
        {
            TempData["RegisterErrorMessage"] = "Пожалуйста, заполните все поля.";
            return Page();
        }

        IsSubmittingRegister = true;

        try
        {
            var httpClient = _httpClientFactory.CreateClient("Api");
            var response = await httpClient.PostAsJsonAsync("api/auth/register", RegisterModel);

            if (!response.IsSuccessStatusCode)
            {
                var text = await response.Content.ReadAsStringAsync();
                var errorMessage = string.IsNullOrWhiteSpace(text)
                    ? $"Ошибка регистрации. Статус: {(int)response.StatusCode}"
                    : text;
                TempData["RegisterErrorMessage"] = errorMessage;
                return Page();
            }

            TempData["RegisterSuccessMessage"] = "Регистрация прошла успешно! Теперь вы можете войти.";
            RegisterModel.Email = string.Empty;
            RegisterModel.Password = string.Empty;
            ActiveTab = "login"; // Переключаемся на вкладку входа после успешной регистрации
        }
        catch (HttpRequestException ex)
        {
            TempData["RegisterErrorMessage"] = $"Ошибка подключения к серверу: {ex.Message}. Убедитесь, что API запущен.";
        }
        catch (TaskCanceledException)
        {
            TempData["RegisterErrorMessage"] = "Превышено время ожидания. Проверьте подключение к серверу.";
        }
        catch (Exception ex)
        {
            TempData["RegisterErrorMessage"] = $"Ошибка: {ex.Message}";
        }
        finally
        {
            IsSubmittingRegister = false;
        }

        return Page();
    }
}
