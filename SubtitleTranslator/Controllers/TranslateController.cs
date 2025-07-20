using Microsoft.AspNetCore.Mvc;
using SubtitleTranslator.Models;
using SubtitleTranslator.Services;
using System.Text;

namespace SubtitleTranslator.Controllers
{
    public class TranslateController : Controller
    {
        private readonly ISubtitleService _subtitleService;
        private readonly ITranslationService _translationService;
        private readonly IFileStorageService _fileStorageService;
        private readonly ILogger<TranslateController> _logger;

        public TranslateController(
            ISubtitleService subtitleService,
            ITranslationService translationService,
            IFileStorageService fileStorageService,
            ILogger<TranslateController> logger)
        {
            _subtitleService = subtitleService;
            _translationService = translationService;
            _fileStorageService = fileStorageService;
            _logger = logger;
        }

        public IActionResult Index()
        {
            var model = new UploadSubtitleViewModel();
            ViewBag.Languages = _subtitleService.GetSupportedLanguages();
            return View(model);
        }

        [HttpPost]
        [RequestSizeLimit(50 * 1024 * 1024)] // 50MB limit
        public async Task<IActionResult> Upload(UploadSubtitleViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Languages = _subtitleService.GetSupportedLanguages();
                return View("Index", model);
            }

            if (!_subtitleService.IsValidSubtitleFile(model.SubtitleFile))
            {
                ModelState.AddModelError("SubtitleFile", "Please upload a valid subtitle file (.srt, .vtt, .ass, .ssa, .sub)");
                ViewBag.Languages = _subtitleService.GetSupportedLanguages();
                return View("Index", model);
            }

            // Check file size (20MB limit for user uploads)
            if (model.SubtitleFile.Length > 20 * 1024 * 1024)
            {
                ModelState.AddModelError("SubtitleFile", "File size must be less than 20MB");
                ViewBag.Languages = _subtitleService.GetSupportedLanguages();
                return View("Index", model);
            }

            try
            {
                _logger.LogInformation("Processing subtitle file upload: {FileName} ({Size} bytes)",
                    model.SubtitleFile.FileName, model.SubtitleFile.Length);

                // Parse the subtitle file
                var subtitle = await _subtitleService.ParseSubtitleFileAsync(model.SubtitleFile);

                if (subtitle == null || subtitle.Paragraphs.Count == 0)
                {
                    ModelState.AddModelError("SubtitleFile", "Could not parse the subtitle file or it contains no subtitles.");
                    ViewBag.Languages = _subtitleService.GetSupportedLanguages();
                    return View("Index", model);
                }

                _logger.LogInformation("Successfully parsed {Count} subtitle entries", subtitle.Paragraphs.Count);

                // Store data in temporary files instead of session/cookies
                var subtitleKey = await _fileStorageService.StoreDataAsync(subtitle, "subtitle_");

                // Store only small metadata in session (no large data)
                HttpContext.Session.SetString("SubtitleKey", subtitleKey);
                HttpContext.Session.SetString("SourceLanguage", model.SourceLanguage ?? "auto");
                HttpContext.Session.SetString("TargetLanguage", model.TargetLanguage);
                HttpContext.Session.SetString("OriginalFileName", model.SubtitleFile.FileName);

                return RedirectToAction("Translate");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing subtitle upload: {FileName}", model.SubtitleFile.FileName);
                ModelState.AddModelError("", "An error occurred while processing your file. Please try again.");
                ViewBag.Languages = _subtitleService.GetSupportedLanguages();
                return View("Index", model);
            }
        }

        public async Task<IActionResult> Translate()
        {
            var subtitleKey = HttpContext.Session.GetString("SubtitleKey");
            var sourceLanguage = HttpContext.Session.GetString("SourceLanguage");
            var targetLanguage = HttpContext.Session.GetString("TargetLanguage");
            var originalFileName = HttpContext.Session.GetString("OriginalFileName");

            if (string.IsNullOrEmpty(subtitleKey))
            {
                _logger.LogWarning("No subtitle key found in session, redirecting to upload");
                return RedirectToAction("Index");
            }

            try
            {
                // Retrieve subtitle data from temporary file
                var subtitle = await _fileStorageService.RetrieveDataAsync<Core.Common.Subtitle>(subtitleKey);

                if (subtitle == null)
                {
                    _logger.LogError("Could not retrieve subtitle data for key: {Key}", subtitleKey);
                    return RedirectToAction("Index");
                }

                _logger.LogInformation("Starting translation from {Source} to {Target} for {Count} subtitles",
                    sourceLanguage, targetLanguage, subtitle.Paragraphs.Count);

                var resultModel = new TranslateResultViewModel
                {
                    OriginalFileName = originalFileName,
                    SourceLanguage = sourceLanguage,
                    TargetLanguage = targetLanguage,
                    OriginalSubtitles = subtitle.Paragraphs.Select(p => new SubtitleParagraphViewModel
                    {
                        Number = p.Number,
                        StartTime = p.StartTime.ToSrtTime(),
                        EndTime = p.EndTime.ToSrtTime(),
                        Text = p.Text
                    }).ToList(),
                    TranslatedSubtitles = new List<SubtitleParagraphViewModel>(),
                    IsSuccess = false
                };

                // Translate each subtitle with proper error handling
                var translatedSubtitle = new Core.Common.Subtitle();
                int totalSubtitles = subtitle.Paragraphs.Count;
                int processedCount = 0;
                int successCount = 0;
                int failedCount = 0;

                foreach (var paragraph in subtitle.Paragraphs)
                {
                    try
                    {
                        var translationResponse = await _translationService.TranslateTextAsync(
                            paragraph.Text, sourceLanguage, targetLanguage);

                        processedCount++;

                        if (translationResponse.IsSuccess)
                        {
                            successCount++;
                            var translatedParagraph = new Core.SubtitleFormats.Paragraph
                            {
                                Number = paragraph.Number,
                                StartTime = paragraph.StartTime,
                                EndTime = paragraph.EndTime,
                                Text = translationResponse.TranslatedText
                            };

                            translatedSubtitle.Paragraphs.Add(translatedParagraph);

                            resultModel.TranslatedSubtitles.Add(new SubtitleParagraphViewModel
                            {
                                Number = paragraph.Number,
                                StartTime = paragraph.StartTime.ToSrtTime(),
                                EndTime = paragraph.EndTime.ToSrtTime(),
                                Text = translationResponse.TranslatedText
                            });
                        }
                        else
                        {
                            failedCount++;
                            _logger.LogWarning("Translation failed for subtitle {Number}: {Error}",
                                paragraph.Number, translationResponse.ErrorMessage);

                            // Keep original text with failure indicator
                            var failedParagraph = new Core.SubtitleFormats.Paragraph
                            {
                                Number = paragraph.Number,
                                StartTime = paragraph.StartTime,
                                EndTime = paragraph.EndTime,
                                Text = paragraph.Text
                            };

                            translatedSubtitle.Paragraphs.Add(failedParagraph);

                            resultModel.TranslatedSubtitles.Add(new SubtitleParagraphViewModel
                            {
                                Number = paragraph.Number,
                                StartTime = paragraph.StartTime.ToSrtTime(),
                                EndTime = paragraph.EndTime.ToSrtTime(),
                                Text = paragraph.Text + " [Translation Failed]"
                            });
                        }

                        // Progress logging every 10 items
                        if (processedCount % 10 == 0 || processedCount == totalSubtitles)
                        {
                            _logger.LogInformation("Progress: {Processed}/{Total} subtitles processed ({Success} successful, {Failed} failed)",
                                processedCount, totalSubtitles, successCount, failedCount);
                        }

                        // Rate limiting delay (increased for large files)
                        await Task.Delay(300);
                    }
                    catch (Exception ex)
                    {
                        failedCount++;
                        _logger.LogError(ex, "Error translating subtitle {Number}: {Text}",
                            paragraph.Number, paragraph.Text);

                        // Add original text with error indicator
                        var errorParagraph = new Core.SubtitleFormats.Paragraph
                        {
                            Number = paragraph.Number,
                            StartTime = paragraph.StartTime,
                            EndTime = paragraph.EndTime,
                            Text = paragraph.Text
                        };

                        translatedSubtitle.Paragraphs.Add(errorParagraph);

                        resultModel.TranslatedSubtitles.Add(new SubtitleParagraphViewModel
                        {
                            Number = paragraph.Number,
                            StartTime = paragraph.StartTime.ToSrtTime(),
                            EndTime = paragraph.EndTime.ToSrtTime(),
                            Text = paragraph.Text + " [Translation Error]"
                        });
                    }
                }

                // Generate final SRT content
                resultModel.TranslatedContent = _subtitleService.GenerateSrtContent(translatedSubtitle);
                resultModel.IsSuccess = true;

                _logger.LogInformation("Translation completed: {Success}/{Total} subtitles successfully translated",
                    successCount, totalSubtitles);

                // Clean up temporary files
                await _fileStorageService.DeleteDataAsync(subtitleKey);

                // Clear session data
                HttpContext.Session.Remove("SubtitleKey");
                HttpContext.Session.Remove("SourceLanguage");
                HttpContext.Session.Remove("TargetLanguage");
                HttpContext.Session.Remove("OriginalFileName");

                return View("Result", resultModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Critical error during translation process");

                // Clean up on error
                if (!string.IsNullOrEmpty(subtitleKey))
                {
                    await _fileStorageService.DeleteDataAsync(subtitleKey);
                }

                // Clear session on error
                HttpContext.Session.Clear();

                var errorModel = new TranslateResultViewModel
                {
                    IsSuccess = false,
                    ErrorMessage = "A critical error occurred during translation. Please try uploading your file again."
                };

                return View("Result", errorModel);
            }
        }

        [HttpPost]
        public async Task<IActionResult> DownloadTranslated(string content, string fileName, string targetLanguage)
        {
            if (string.IsNullOrEmpty(content))
            {
                return BadRequest("No content to download");
            }

            try
            {
                var originalFileName = Path.GetFileNameWithoutExtension(fileName);
                var extension = Path.GetExtension(fileName);
                var newFileName = $"{originalFileName}_{targetLanguage}{extension}";

                var bytes = Encoding.UTF8.GetBytes(content);

                return File(bytes, "application/octet-stream", newFileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating download file");
                return BadRequest("Error generating download file");
            }
        }

        [HttpGet]
        public IActionResult GetLanguages()
        {
            var languages = _subtitleService.GetSupportedLanguages();
            return Json(languages);
        }
    }
}