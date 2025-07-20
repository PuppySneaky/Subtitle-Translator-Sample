using SubtitleTranslator.Models;

namespace SubtitleTranslator.Services
{
    public interface ITranslationService
    {
        Task<TranslationResponse> TranslateTextAsync(string text, string sourceLanguage, string targetLanguage);
        Task<List<TranslationResponse>> TranslateTextsAsync(List<string> texts, string sourceLanguage, string targetLanguage);
        bool IsServiceAvailable();
        string GetServiceName();
        int GetMaxCharacters();
    }
}