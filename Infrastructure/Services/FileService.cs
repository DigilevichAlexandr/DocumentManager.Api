using DocumentManager.Api.Application.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace DocumentManager.Api.Infrastructure.Services;

public class FileService : IFileService
{
    private readonly string _uploadsPath;
    private readonly IWebHostEnvironment _environment;

    public FileService(IWebHostEnvironment environment, IConfiguration configuration)
    {
        _environment = environment;
        _uploadsPath = configuration["FileStorage:Path"] ?? Path.Combine(_environment.ContentRootPath, "Uploads");
        
        if (!Directory.Exists(_uploadsPath))
        {
            Directory.CreateDirectory(_uploadsPath);
        }
    }

    public async Task<string> SaveFileAsync(Stream fileStream, string fileName, Guid documentId, Guid ownerId)
    {
        var ownerDirectory = Path.Combine(_uploadsPath, ownerId.ToString());
        if (!Directory.Exists(ownerDirectory))
        {
            Directory.CreateDirectory(ownerDirectory);
        }

        var fileExtension = Path.GetExtension(fileName);
        var uniqueFileName = $"{documentId}{fileExtension}";
        var filePath = Path.Combine(ownerDirectory, uniqueFileName);

        using (var fileStreamWriter = new FileStream(filePath, FileMode.Create))
        {
            await fileStream.CopyToAsync(fileStreamWriter);
        }

        return filePath;
    }

    public Task<FileStream?> GetFileAsync(string filePath)
    {
        if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
        {
            return Task.FromResult<FileStream?>(null);
        }

        var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        return Task.FromResult<FileStream?>(fileStream);
    }

    public Task<bool> DeleteFileAsync(string filePath)
    {
        if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
        {
            return Task.FromResult(false);
        }

        try
        {
            File.Delete(filePath);
            return Task.FromResult(true);
        }
        catch
        {
            return Task.FromResult(false);
        }
    }

    public async Task<string> CopyFileAsync(string sourcePath, string fileName, Guid newDocumentId, Guid newOwnerId)
    {
        if (string.IsNullOrEmpty(sourcePath) || !File.Exists(sourcePath))
        {
            throw new FileNotFoundException("Исходный файл не найден", sourcePath);
        }

        var ownerDirectory = Path.Combine(_uploadsPath, newOwnerId.ToString());
        if (!Directory.Exists(ownerDirectory))
        {
            Directory.CreateDirectory(ownerDirectory);
        }

        var fileExtension = Path.GetExtension(fileName);
        var uniqueFileName = $"{newDocumentId}{fileExtension}";
        var destinationPath = Path.Combine(ownerDirectory, uniqueFileName);

        await Task.Run(() => File.Copy(sourcePath, destinationPath, overwrite: true));

        return destinationPath;
    }

    public string GetContentType(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return extension switch
        {
            ".pdf" => "application/pdf",
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".xls" => "application/vnd.ms-excel",
            ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            ".txt" => "text/plain",
            ".jpg" => "image/jpeg",
            ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            _ => "application/octet-stream"
        };
    }
}
