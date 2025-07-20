using System.ComponentModel.DataAnnotations;

namespace SubtitleTranslator.Models
{
    public class UploadSubtitleViewModel
    {
        [Required(ErrorMessage = "Please select a subtitle file")]
        [Display(Name = "Subtitle File")]
        public IFormFile SubtitleFile { get; set; }

        [Required(ErrorMessage = "Please select source language")]
        [Display(Name = "Source Language")]
        public string SourceLanguage { get; set; }

        [Required(ErrorMessage = "Please select target language")]
        [Display(Name = "Target Language")]
        public string TargetLanguage { get; set; }
    }

    public class TranslateResultViewModel
    {
        public string OriginalFileName { get; set; }
        public string SourceLanguage { get; set; }
        public string TargetLanguage { get; set; }
        public List<SubtitleParagraphViewModel> OriginalSubtitles { get; set; }
        public List<SubtitleParagraphViewModel> TranslatedSubtitles { get; set; }
        public string TranslatedContent { get; set; }
        public bool IsSuccess { get; set; }
        public string ErrorMessage { get; set; }
    }

    public class SubtitleParagraphViewModel
    {
        public int Number { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public string Text { get; set; }
    }

    public class LanguageOption
    {
        public string Code { get; set; }
        public string Name { get; set; }
    }

    public class TranslationProgress
    {
        public int TotalItems { get; set; }
        public int ProcessedItems { get; set; }
        public double ProgressPercentage => TotalItems > 0 ? (double)ProcessedItems / TotalItems * 100 : 0;
        public string CurrentItem { get; set; }
        public bool IsCompleted { get; set; }
        public bool HasError { get; set; }
        public string ErrorMessage { get; set; }
    }
}