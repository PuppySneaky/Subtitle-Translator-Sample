using System.Text.Json;

namespace SubtitleTranslator.Services
{
    public class FileStorageService : IFileStorageService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<FileStorageService> _logger;
        private readonly string _tempPath;

        public FileStorageService(IWebHostEnvironment environment, ILogger<FileStorageService> logger)
        {
            _environment = environment;
            _logger = logger;
            _tempPath = Path.Combine(_environment.WebRootPath, "temp");

            if (!Directory.Exists(_tempPath))
                Directory.CreateDirectory(_tempPath);
        }

        public async Task<string> StoreDataAsync<T>(T data, string prefix = "")
        {
            try
            {
                var key = $"{prefix}{Guid.NewGuid()}.json";
                var filePath = Path.Combine(_tempPath, key);

                var json = JsonSerializer.Serialize(data, new JsonSerializerOptions
                {
                    WriteIndented = false,
                    DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
                });

                await File.WriteAllTextAsync(filePath, json, System.Text.Encoding.UTF8);
                _logger.LogDebug("Stored data to file: {Key}", key);
                return key;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error storing data to file");
                throw;
            }
        }

        public async Task<T> RetrieveDataAsync<T>(string key)
        {
            try
            {
                if (string.IsNullOrEmpty(key))
                    return default(T);

                var filePath = Path.Combine(_tempPath, key);

                if (!File.Exists(filePath))
                {
                    _logger.LogWarning("File not found: {Key}", key);
                    return default(T);
                }

                var json = await File.ReadAllTextAsync(filePath, System.Text.Encoding.UTF8);
                var result = JsonSerializer.Deserialize<T>(json);
                _logger.LogDebug("Retrieved data from file: {Key}", key);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving data from file: {Key}", key);
                return default(T);
            }
        }

        public async Task DeleteDataAsync(string key)
        {
            try
            {
                if (string.IsNullOrEmpty(key))
                    return;

                var filePath = Path.Combine(_tempPath, key);

                if (File.Exists(filePath))
                {
                    await Task.Run(() => File.Delete(filePath));
                    _logger.LogDebug("Deleted file: {Key}", key);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting file: {Key}", key);
            }
        }

        public async Task<string> StoreFileAsync(IFormFile file)
        {
            try
            {
                var key = $"upload_{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                var filePath = Path.Combine(_tempPath, key);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                _logger.LogDebug("Stored uploaded file: {Key}", key);
                return key;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error storing uploaded file");
                throw;
            }
        }

        public async Task<byte[]> RetrieveFileAsync(string key)
        {
            try
            {
                if (string.IsNullOrEmpty(key))
                    return null;

                var filePath = Path.Combine(_tempPath, key);

                if (!File.Exists(filePath))
                    return null;

                return await File.ReadAllBytesAsync(filePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving file: {Key}", key);
                return null;
            }
        }

        public void CleanupOldFiles(TimeSpan maxAge)
        {
            try
            {
                var cutoffTime = DateTime.Now - maxAge;
                var files = Directory.GetFiles(_tempPath);

                int deletedCount = 0;
                foreach (var file in files)
                {
                    var fileInfo = new FileInfo(file);
                    if (fileInfo.CreationTime < cutoffTime)
                    {
                        File.Delete(file);
                        deletedCount++;
                    }
                }

                if (deletedCount > 0)
                {
                    _logger.LogInformation("Cleaned up {Count} old temporary files", deletedCount);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cleaning up old files");
            }
        }
    }
}