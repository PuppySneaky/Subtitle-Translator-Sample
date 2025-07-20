using SubtitleTranslator.Core.Common;
using SubtitleTranslator.Models;
using System.Text;

namespace SubtitleTranslator.Services
{
    public class SubtitleService : ISubtitleService
    {
        private readonly ILogger<SubtitleService> _logger;

        public SubtitleService(ILogger<SubtitleService> logger)
        {
            _logger = logger;
        }

        public async Task<Subtitle> ParseSubtitleFileAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return null;

            try
            {
                using var stream = file.OpenReadStream();
                using var reader = new StreamReader(stream, Encoding.UTF8);
                var content = await reader.ReadToEndAsync();

                var lines = content.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None).ToList();
                var fileExtension = Path.GetExtension(file.FileName);

                return Subtitle.Parse(lines, fileExtension);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing subtitle file: {FileName}", file.FileName);
                return null;
            }
        }

        public async Task<Subtitle> ParseSubtitleTextAsync(string content, string fileExtension)
        {
            if (string.IsNullOrEmpty(content))
                return null;

            try
            {
                return await Task.FromResult(Subtitle.ParseFromText(content, fileExtension));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing subtitle text");
                return null;
            }
        }

        public string GenerateSrtContent(Subtitle subtitle)
        {
            if (subtitle == null || subtitle.Paragraphs == null)
                return string.Empty;

            subtitle.Renumber();
            return subtitle.ToSrt();
        }

        public List<LanguageOption> GetSupportedLanguages()
        {
            return new List<LanguageOption>
            {
                new LanguageOption { Code = "af", Name = "Afrikaans" },
                new LanguageOption { Code = "sq", Name = "Albanian" },
                new LanguageOption { Code = "am", Name = "Amharic" },
                new LanguageOption { Code = "ar", Name = "Arabic" },
                new LanguageOption { Code = "hy", Name = "Armenian" },
                new LanguageOption { Code = "az", Name = "Azerbaijani" },
                new LanguageOption { Code = "eu", Name = "Basque" },
                new LanguageOption { Code = "be", Name = "Belarusian" },
                new LanguageOption { Code = "bn", Name = "Bengali" },
                new LanguageOption { Code = "bs", Name = "Bosnian" },
                new LanguageOption { Code = "bg", Name = "Bulgarian" },
                new LanguageOption { Code = "ca", Name = "Catalan" },
                new LanguageOption { Code = "ceb", Name = "Cebuano" },
                new LanguageOption { Code = "ny", Name = "Chichewa" },
                new LanguageOption { Code = "zh-cn", Name = "Chinese (Simplified)" },
                new LanguageOption { Code = "zh-tw", Name = "Chinese (Traditional)" },
                new LanguageOption { Code = "co", Name = "Corsican" },
                new LanguageOption { Code = "hr", Name = "Croatian" },
                new LanguageOption { Code = "cs", Name = "Czech" },
                new LanguageOption { Code = "da", Name = "Danish" },
                new LanguageOption { Code = "nl", Name = "Dutch" },
                new LanguageOption { Code = "en", Name = "English" },
                new LanguageOption { Code = "eo", Name = "Esperanto" },
                new LanguageOption { Code = "et", Name = "Estonian" },
                new LanguageOption { Code = "tl", Name = "Filipino" },
                new LanguageOption { Code = "fi", Name = "Finnish" },
                new LanguageOption { Code = "fr", Name = "French" },
                new LanguageOption { Code = "fy", Name = "Frisian" },
                new LanguageOption { Code = "gl", Name = "Galician" },
                new LanguageOption { Code = "ka", Name = "Georgian" },
                new LanguageOption { Code = "de", Name = "German" },
                new LanguageOption { Code = "el", Name = "Greek" },
                new LanguageOption { Code = "gu", Name = "Gujarati" },
                new LanguageOption { Code = "ht", Name = "Haitian Creole" },
                new LanguageOption { Code = "ha", Name = "Hausa" },
                new LanguageOption { Code = "haw", Name = "Hawaiian" },
                new LanguageOption { Code = "he", Name = "Hebrew" },
                new LanguageOption { Code = "hi", Name = "Hindi" },
                new LanguageOption { Code = "hmn", Name = "Hmong" },
                new LanguageOption { Code = "hu", Name = "Hungarian" },
                new LanguageOption { Code = "is", Name = "Icelandic" },
                new LanguageOption { Code = "ig", Name = "Igbo" },
                new LanguageOption { Code = "id", Name = "Indonesian" },
                new LanguageOption { Code = "ga", Name = "Irish" },
                new LanguageOption { Code = "it", Name = "Italian" },
                new LanguageOption { Code = "ja", Name = "Japanese" },
                new LanguageOption { Code = "jw", Name = "Javanese" },
                new LanguageOption { Code = "kn", Name = "Kannada" },
                new LanguageOption { Code = "kk", Name = "Kazakh" },
                new LanguageOption { Code = "km", Name = "Khmer" },
                new LanguageOption { Code = "ko", Name = "Korean" },
                new LanguageOption { Code = "ku", Name = "Kurdish" },
                new LanguageOption { Code = "ky", Name = "Kyrgyz" },
                new LanguageOption { Code = "lo", Name = "Lao" },
                new LanguageOption { Code = "la", Name = "Latin" },
                new LanguageOption { Code = "lv", Name = "Latvian" },
                new LanguageOption { Code = "lt", Name = "Lithuanian" },
                new LanguageOption { Code = "lb", Name = "Luxembourgish" },
                new LanguageOption { Code = "mk", Name = "Macedonian" },
                new LanguageOption { Code = "mg", Name = "Malagasy" },
                new LanguageOption { Code = "ms", Name = "Malay" },
                new LanguageOption { Code = "ml", Name = "Malayalam" },
                new LanguageOption { Code = "mt", Name = "Maltese" },
                new LanguageOption { Code = "mi", Name = "Maori" },
                new LanguageOption { Code = "mr", Name = "Marathi" },
                new LanguageOption { Code = "mn", Name = "Mongolian" },
                new LanguageOption { Code = "my", Name = "Myanmar" },
                new LanguageOption { Code = "ne", Name = "Nepali" },
                new LanguageOption { Code = "no", Name = "Norwegian" },
                new LanguageOption { Code = "or", Name = "Odia" },
                new LanguageOption { Code = "ps", Name = "Pashto" },
                new LanguageOption { Code = "fa", Name = "Persian" },
                new LanguageOption { Code = "pl", Name = "Polish" },
                new LanguageOption { Code = "pt", Name = "Portuguese" },
                new LanguageOption { Code = "pa", Name = "Punjabi" },
                new LanguageOption { Code = "ro", Name = "Romanian" },
                new LanguageOption { Code = "ru", Name = "Russian" },
                new LanguageOption { Code = "sm", Name = "Samoan" },
                new LanguageOption { Code = "gd", Name = "Scots Gaelic" },
                new LanguageOption { Code = "sr", Name = "Serbian" },
                new LanguageOption { Code = "st", Name = "Sesotho" },
                new LanguageOption { Code = "sn", Name = "Shona" },
                new LanguageOption { Code = "sd", Name = "Sindhi" },
                new LanguageOption { Code = "si", Name = "Sinhala" },
                new LanguageOption { Code = "sk", Name = "Slovak" },
                new LanguageOption { Code = "sl", Name = "Slovenian" },
                new LanguageOption { Code = "so", Name = "Somali" },
                new LanguageOption { Code = "es", Name = "Spanish" },
                new LanguageOption { Code = "su", Name = "Sundanese" },
                new LanguageOption { Code = "sw", Name = "Swahili" },
                new LanguageOption { Code = "sv", Name = "Swedish" },
                new LanguageOption { Code = "tg", Name = "Tajik" },
                new LanguageOption { Code = "ta", Name = "Tamil" },
                new LanguageOption { Code = "te", Name = "Telugu" },
                new LanguageOption { Code = "th", Name = "Thai" },
                new LanguageOption { Code = "tr", Name = "Turkish" },
                new LanguageOption { Code = "uk", Name = "Ukrainian" },
                new LanguageOption { Code = "ur", Name = "Urdu" },
                new LanguageOption { Code = "ug", Name = "Uyghur" },
                new LanguageOption { Code = "uz", Name = "Uzbek" },
                new LanguageOption { Code = "vi", Name = "Vietnamese" },
                new LanguageOption { Code = "cy", Name = "Welsh" },
                new LanguageOption { Code = "xh", Name = "Xhosa" },
                new LanguageOption { Code = "yi", Name = "Yiddish" },
                new LanguageOption { Code = "yo", Name = "Yoruba" },
                new LanguageOption { Code = "zu", Name = "Zulu" }
            };
        }

        public async Task<bool> SaveSubtitleFileAsync(string content, string fileName, string directory)
        {
            try
            {
                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                var filePath = Path.Combine(directory, fileName);
                await File.WriteAllTextAsync(filePath, content, Encoding.UTF8);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving subtitle file: {FileName}", fileName);
                return false;
            }
        }

        public string DetectLanguage(Subtitle subtitle)
        {
            // Simple language detection based on common words
            // In a real implementation, you might use a more sophisticated language detection library
            if (subtitle?.Paragraphs == null || subtitle.Paragraphs.Count == 0)
                return "en";

            var allText = string.Join(" ", subtitle.Paragraphs.Select(p => p.Text)).ToLowerInvariant();

            // Simple heuristic detection
            if (allText.Contains("the ") || allText.Contains(" and ") || allText.Contains(" is "))
                return "en";
            if (allText.Contains("der ") || allText.Contains("die ") || allText.Contains("das "))
                return "de";
            if (allText.Contains("le ") || allText.Contains("la ") || allText.Contains("les "))
                return "fr";
            if (allText.Contains("el ") || allText.Contains("la ") || allText.Contains("los "))
                return "es";

            return "en"; // Default to English
        }

        public bool IsValidSubtitleFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return false;

            var allowedExtensions = new[] { ".srt", ".vtt", ".ass", ".ssa", ".sub" };
            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();

            return allowedExtensions.Contains(fileExtension);
        }
    }
}