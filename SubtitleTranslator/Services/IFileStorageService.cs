namespace SubtitleTranslator.Services
{
    public interface IFileStorageService
    {
        Task<string> StoreDataAsync<T>(T data, string prefix = "");
        Task<T> RetrieveDataAsync<T>(string key);
        Task DeleteDataAsync(string key);
        Task<string> StoreFileAsync(IFormFile file);
        Task<byte[]> RetrieveFileAsync(string key);
        void CleanupOldFiles(TimeSpan maxAge);
    }
}