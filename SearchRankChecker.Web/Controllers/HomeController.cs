using System;
using System.Globalization;
using System.Threading;
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

        public IActionResult Index(SearchViewModel searchViewModel)
        {
            // Get current culture
            // Doing this here in an attempt to force google search to return AU specific results - Doesn't work
            var currentCulture = Thread.CurrentThread.CurrentCulture;
            searchViewModel.SearchRegion = currentCulture.Name;

            return View(searchViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Search(Search searchModel)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogError("Input data not provided or invalid!");
                return View(nameof(Index), new SearchViewModel());
            }

            if (!Uri.TryCreate(searchModel.UrlToSearch, UriKind.Absolute, out var urlToSearch))
            {
                var errorMessage = "Provided Url is invalid";

                ModelState.AddModelError(nameof(searchModel.UrlToSearch), errorMessage);

                _logger.LogError(errorMessage);

                return View(nameof(Index));
            }

            try
            {
                var searchResults = await _crawlerService.GetSearchResults(searchModel.SearchKeywords);

                return _GetSearchRank(searchResults, urlToSearch);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Http Request Failed. Check Request Parameters");
                return RedirectToAction("Error");
            }
        }

        [HttpPost]
        public IActionResult SetLanguage(SearchViewModel searchViewModel)
        {
            // Set Culture
            CultureInfo newCulture = new CultureInfo(searchViewModel.SearchRegion);

            CultureInfo.DefaultThreadCurrentCulture = newCulture;
            CultureInfo.DefaultThreadCurrentUICulture = newCulture;

            return RedirectToAction(nameof(Index), new SearchViewModel
            {
                SearchRegion = Thread.CurrentThread.CurrentCulture.Name
            });
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
                var searchRankViewModel = new SearchViewModel
                {
                    RankString = _rankCalculator.GetUrlRanksFromSearchResults(searchResults, urlToSearch)
                };

                return RedirectToAction(nameof(Index), searchRankViewModel);
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex.Message);

                return RedirectToAction("Error");
            }
        }
    }
}
