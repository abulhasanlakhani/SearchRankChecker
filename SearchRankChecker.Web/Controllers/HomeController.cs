using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SearchRankChecker.Business.Interfaces;
using SearchRankChecker.Domain.Models;
using SearchRankChecker.Web.ViewModels;

namespace SearchRankChecker.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ICrawlerService _crawlerService;
        private readonly IRankCalculator _rankCalculator;

        public HomeController(ICrawlerService crawlerService, IRankCalculator rankCalculator)
        {
            _crawlerService = crawlerService;
            _rankCalculator = rankCalculator;
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
                return View(nameof(Index));
            }

            if (!Uri.TryCreate(searchModel.UrlToSearch, UriKind.Absolute, out var urlToSearch))
            {
                ModelState.AddModelError(nameof(searchModel.UrlToSearch), "Provided Url is invalid");
                return View(nameof(Index));
            }
            
            var searchResults = await _crawlerService.GetSearchResults(urlToSearch, searchModel.SearchKeywords);
            
            var searchRankViewModel = new SearchRankViewModel
            {
                RankString = _rankCalculator.GetUrlRanksFromSearchResults(searchResults, urlToSearch)
            };
            
            return RedirectToAction(nameof(Index), new { searchRankViewModel.RankString });
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
