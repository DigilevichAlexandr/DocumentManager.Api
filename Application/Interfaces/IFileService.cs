namespace DocumentManager.Api.Application.Interfaces;

public interface IFileService
{
    Task<string> SaveFileAsync(Stream fileStream, string fileName, Guid documentId, Guid ownerId);
    Task<FileStream?> GetFileAsync(string filePath);
    Task<bool> DeleteFileAsync(string filePath);
    Task<string> CopyFileAsync(string sourcePath, string fileName, Guid newDocumentId, Guid newOwnerId);
    string GetContentType(string fileName);
}
