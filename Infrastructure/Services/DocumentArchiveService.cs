using DocumentManager.Api.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DocumentManager.Api.Infrastructure.Services;

public class DocumentArchiveService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DocumentArchiveService> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromHours(1);

    public DocumentArchiveService(IServiceProvider serviceProvider, ILogger<DocumentArchiveService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ArchiveExpiredDocumentsAsync();
                await DeleteOldArchivedDocumentsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при выполнении фоновой задачи архивации документов");
            }

            await Task.Delay(_checkInterval, stoppingToken);
        }
    }

    private async Task ArchiveExpiredDocumentsAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        try
        {
            var expiredDocuments = (await unitOfWork.Documents.GetExpiredDocumentsAsync()).ToList();
            
            if (!expiredDocuments.Any())
                return;

            _logger.LogInformation($"Найдено {expiredDocuments.Count} истекших документов для архивации");

            foreach (var document in expiredDocuments)
            {
                document.IsArchived = true;
                document.ArchivedAt = DateTime.UtcNow;
                document.HistoryDeleteAt = DateTime.UtcNow.AddDays(30);
                document.Status = Domain.Enums.DocumentStatus.Expired;
                await unitOfWork.Documents.UpdateAsync(document);
            }

            await unitOfWork.SaveChangesAsync();
            _logger.LogInformation($"Заархивировано {expiredDocuments.Count} документов");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при архивации истекших документов");
        }
    }

    private async Task DeleteOldArchivedDocumentsAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var fileService = scope.ServiceProvider.GetRequiredService<Application.Interfaces.IFileService>();

        try
        {
            var documentsToDelete = (await unitOfWork.Documents.GetDocumentsForHistoryDeletionAsync()).ToList();
            
            if (!documentsToDelete.Any())
                return;

            _logger.LogInformation($"Найдено {documentsToDelete.Count} документов для удаления из истории");

            foreach (var document in documentsToDelete)
            {
                // Удаляем файл, если он есть
                if (!string.IsNullOrEmpty(document.FilePath))
                {
                    await fileService.DeleteFileAsync(document.FilePath);
                }

                // Помечаем документ как удаленный
                document.IsDeleted = true;
                document.DeletedAt = DateTime.UtcNow;
                await unitOfWork.Documents.UpdateAsync(document);
            }

            await unitOfWork.SaveChangesAsync();
            _logger.LogInformation($"Удалено {documentsToDelete.Count} документов из истории");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при удалении старых документов из истории");
        }
    }
}
