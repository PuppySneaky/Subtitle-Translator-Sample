using Microsoft.AspNetCore.Mvc;
using SubtitleTranslator.Models;
using SubtitleTranslator.Services;
using System.Diagnostics;

namespace SubtitleTranslator.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ISubtitleService _subtitleService;

        public HomeController(ILogger<HomeController> logger, ISubtitleService subtitleService)
        {
            _logger = logger;
            _subtitleService = subtitleService;
        }

        public IActionResult Index()
        {
            var model = new UploadSubtitleViewModel();
            ViewBag.Languages = _subtitleService.GetSupportedLanguages();
            return View(model);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}