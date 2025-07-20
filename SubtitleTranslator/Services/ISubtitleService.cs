using SubtitleTranslator.Core.Common;
using SubtitleTranslator.Models;

namespace SubtitleTranslator.Services
{
    public interface ISubtitleService
    {
        Task<Subtitle> ParseSubtitleFileAsync(IFormFile file);
        Task<Subtitle> ParseSubtitleTextAsync(string content, string fileExtension);
        string GenerateSrtContent(Subtitle subtitle);
        List<LanguageOption> GetSupportedLanguages();
        Task<bool> SaveSubtitleFileAsync(string content, string fileName, string directory);
        string DetectLanguage(Subtitle subtitle);
        bool IsValidSubtitleFile(IFormFile file);
    }
}