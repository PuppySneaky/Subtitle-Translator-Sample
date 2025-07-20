using SubtitleTranslator.Models;
using System.Text;
using System.Text.Json;
using System.Web;

namespace SubtitleTranslator.Services
{
    public class GoogleTranslationService : ITranslationService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<GoogleTranslationService> _logger;

        public GoogleTranslationService(HttpClient httpClient, ILogger<GoogleTranslationService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;

            // Configure HttpClient for Google Translate
            _httpClient.DefaultRequestHeaders.Add("User-Agent",
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36");
        }

        public async Task<TranslationResponse> TranslateTextAsync(string text, string sourceLanguage, string targetLanguage)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return new TranslationResponse
                {
                    TranslatedText = string.Empty,
                    IsSuccess = true
                };
            }

            try
            {
                // Clean the text for translation
                var cleanText = CleanTextForTranslation(text);

                // Use Google Translate free API endpoint
                var encodedText = HttpUtility.UrlEncode(cleanText);
                var url = $"https://translate.googleapis.com/translate_a/single?client=gtx&sl={sourceLanguage}&tl={targetLanguage}&dt=t&q={encodedText}";

                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var translatedText = ParseGoogleTranslateResponse(jsonResponse, text);

                    return new TranslationResponse
                    {
                        TranslatedText = translatedText,
                        IsSuccess = true
                    };
                }
                else
                {
                    _logger.LogError("Google Translate API returned error: {StatusCode}", response.StatusCode);
                    return new TranslationResponse
                    {
                        TranslatedText = text,
                        IsSuccess = false,
                        ErrorMessage = $"Translation service returned error: {response.StatusCode}"
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error translating text: {Text}", text);
                return new TranslationResponse
                {
                    TranslatedText = text,
                    IsSuccess = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<List<TranslationResponse>> TranslateTextsAsync(List<string> texts, string sourceLanguage, string targetLanguage)
        {
            var results = new List<TranslationResponse>();

            foreach (var text in texts)
            {
                var result = await TranslateTextAsync(text, sourceLanguage, targetLanguage);
                results.Add(result);

                // Add a small delay to avoid rate limiting
                await Task.Delay(100);
            }

            return results;
        }

        public bool IsServiceAvailable()
        {
            try
            {
                // Simple connectivity check
                return true; // In a real implementation, you might want to check connectivity
            }
            catch
            {
                return false;
            }
        }

        public string GetServiceName()
        {
            return "Google Translate";
        }

        public int GetMaxCharacters()
        {
            return 1500; // Google Translate free API limit per request
        }

        private string CleanTextForTranslation(string text)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            // Remove HTML tags but preserve subtitle formatting
            var cleanText = text;

            // Preserve line breaks
            cleanText = cleanText.Replace("\r\n", "\\n").Replace("\n", "\\n");

            // Remove or replace problematic characters for URL encoding
            cleanText = cleanText.Replace("&", "and");

            return cleanText;
        }

        private string ParseGoogleTranslateResponse(string jsonResponse, string originalText)
        {
            try
            {
                // Google Translate free API returns a complex JSON array
                // Format: [[["translated text","original text",null,null,0]],null,"source_lang"]

                using var document = JsonDocument.Parse(jsonResponse);
                var root = document.RootElement;

                if (root.ValueKind == JsonValueKind.Array && root.GetArrayLength() > 0)
                {
                    var firstElement = root[0];
                    if (firstElement.ValueKind == JsonValueKind.Array && firstElement.GetArrayLength() > 0)
                    {
                        var translationArray = firstElement[0];
                        if (translationArray.ValueKind == JsonValueKind.Array && translationArray.GetArrayLength() > 0)
                        {
                            var translatedText = translationArray[0].GetString();

                            // Restore line breaks
                            if (!string.IsNullOrEmpty(translatedText))
                            {
                                translatedText = translatedText.Replace("\\n", "\n");
                                return translatedText;
                            }
                        }
                    }
                }

                // If parsing fails, return original text
                return originalText;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing Google Translate response");
                return originalText;
            }
        }
    }
}