using DocumentManager.Ui.Models;
using DocumentManager.Ui.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;
using System.Text.Json;

namespace DocumentManager.Ui.Pages;

public class StatisticsModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly AuthState _authState;

    public StatisticsModel(IHttpClientFactory httpClientFactory, AuthState authState)
    {
        _httpClientFactory = httpClientFactory;
        _authState = authState;
    }

    public UserStatisticsResponse? Statistics { get; set; }
    public bool IsLoading { get; set; }
    public string? Error { get; set; }
    public int? SelectedYear { get; set; }

    public async Task OnGetAsync(int? year = null)
    {
        ViewData["IsAuthenticated"] = _authState.IsAuthenticated;
        SelectedYear = year;
        
        if (!_authState.IsAuthenticated)
        {
            Error = "Необходимо выполнить вход.";
            return;
        }

        await LoadStatisticsAsync(year);
    }

    private async Task LoadStatisticsAsync(int? year = null)
    {
        IsLoading = true;
        Error = null;

        try
        {
            var httpClient = _httpClientFactory.CreateClient("Api");
            var url = year.HasValue ? $"api/documents/statistics?year={year.Value}" : "api/documents/statistics";
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            
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
                    Error = $"Ошибка загрузки статистики: {(int)response.StatusCode}. {errorContent}";
                }
                return;
            }

            var json = await response.Content.ReadAsStringAsync();
            Statistics = JsonSerializer.Deserialize<UserStatisticsResponse>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
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
