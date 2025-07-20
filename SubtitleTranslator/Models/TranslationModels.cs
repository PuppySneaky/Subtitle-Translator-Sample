namespace SubtitleTranslator.Models
{
    public class TranslationRequest
    {
        public string Text { get; set; }
        public string SourceLanguage { get; set; }
        public string TargetLanguage { get; set; }
    }

    public class TranslationResponse
    {
        public string TranslatedText { get; set; }
        public bool IsSuccess { get; set; }
        public string ErrorMessage { get; set; }
    }

    public class TranslationPair
    {
        public string Name { get; set; }
        public string Code { get; set; }

        public TranslationPair(string name, string code)
        {
            Name = name;
            Code = code;
        }
    }

    public class GoogleTranslateResponse
    {
        public Data data { get; set; }
    }

    public class Data
    {
        public List<Translation> translations { get; set; }
    }

    public class Translation
    {
        public string translatedText { get; set; }
        public string detectedSourceLanguage { get; set; }
    }
}