using DocumentManager.Ui.Models;
using DocumentManager.Ui.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;
using System.Text.Json;

namespace DocumentManager.Ui.Pages;

public class HistoryModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly AuthState _authState;

    public HistoryModel(IHttpClientFactory httpClientFactory, AuthState authState)
    {
        _httpClientFactory = httpClientFactory;
        _authState = authState;
    }

    public List<DocumentResponse> Documents { get; set; } = new();
    public bool IsLoading { get; set; }
    public string? Error { get; set; }

    public async Task OnGetAsync()
    {
        ViewData["IsAuthenticated"] = _authState.IsAuthenticated;
        if (!_authState.IsAuthenticated)
        {
            Error = "Необходимо выполнить вход.";
            return;
        }

        await LoadHistoryAsync();
    }

    private async Task LoadHistoryAsync()
    {
        IsLoading = true;
        Error = null;

        try
        {
            var httpClient = _httpClientFactory.CreateClient("Api");
            using var request = new HttpRequestMessage(HttpMethod.Get, "api/documents/history");
            
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
                    Error = $"Ошибка загрузки истории: {(int)response.StatusCode}. {errorContent}";
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
