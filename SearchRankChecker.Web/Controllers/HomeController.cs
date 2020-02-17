using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SearchRankChecker.Business.Interfaces;
using SearchRankChecker.Domain.Models;
using SearchRankChecker.Web.ViewModels;

namespace SearchRankChecker.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ICrawlerService _crawlerService;
        private readonly IRankCalculator _rankCalculator;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ICrawlerService crawlerService, IRankCalculator rankCalculator, ILogger<HomeController> logger)
        {
            _crawlerService = crawlerService;
            _rankCalculator = rankCalculator;
            _logger = logger;
        }

        public IActionResult Index(SearchRankViewModel searchRankViewModel)
        {
            return View(nameof(Index), searchRankViewModel.RankString);
        }

        [HttpPost]
        public async Task<IActionResult> Search(Search searchModel)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogError("Input data not provided or invalid!");
                return View(nameof(Index));
            }

            if (!Uri.TryCreate(searchModel.UrlToSearch, UriKind.Absolute, out var urlToSearch))
            {
                var errorMessage = "Provided Url is invalid";
                
                ModelState.AddModelError(nameof(searchModel.UrlToSearch), errorMessage);
                
                _logger.LogError(errorMessage);

                return View(nameof(Index));
            }
            
            var searchResults = await _crawlerService.GetSearchResults(urlToSearch, searchModel.SearchKeywords);

            return _GetSearchRank(searchResults, urlToSearch);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View();
        }

        private IActionResult _GetSearchRank(string searchResults, Uri urlToSearch)
        {
            try
            {
                var searchRankViewModel = new SearchRankViewModel
                {
                    RankString = _rankCalculator.GetUrlRanksFromSearchResults(searchResults, urlToSearch)
                };
            
                return RedirectToAction(nameof(Index), new { searchRankViewModel.RankString });
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex.Message);

                return RedirectToAction("Error");
            }
        }
    }
}
