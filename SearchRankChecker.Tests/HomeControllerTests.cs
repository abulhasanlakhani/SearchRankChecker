using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SearchRankChecker.Business.Interfaces;
using SearchRankChecker.Domain.Models;
using SearchRankChecker.Web.Controllers;
using SearchRankChecker.Web.ViewModels;

namespace SearchRankChecker.Tests
{
    [TestFixture]
    public class HomeControllerTests
    {
        private Mock<ICrawlerService> _mockCrawlService;
        private Mock<IRankCalculator> _mockRankCalculator;
        private Mock<ILogger<HomeController>> _mockLogger;
        private Mock<IConfiguration> _mockConfig;
        private HomeController _homeController;
        

        [SetUp]
        public void Setup()
        {
            _mockCrawlService = new Mock<ICrawlerService>();
            _mockRankCalculator = new Mock<IRankCalculator>();
            _mockLogger = new Mock<ILogger<HomeController>>();
            _mockConfig = new Mock<IConfiguration>();

            _homeController = new HomeController(_mockCrawlService.Object, _mockRankCalculator.Object, _mockLogger.Object, _mockConfig.Object);
        }

        [Test]
        public void Search_Get_Action_Returns_View_Result_With_Empty_Rank_String()
        {
            var result = _homeController.Index(new SearchViewModel { RankString = "" });

            Assert.That(result, Is.TypeOf<ViewResult>());
        }

        [Test]
        public void Privacy_Get_Action_Returns_View_Privacy_View_Result()
        {
            var result = _homeController.Privacy();

            Assert.That(result, Is.TypeOf<ViewResult>());
            Assert.That(((ViewResult)result).ViewName, Is.EqualTo("Privacy"));
        }

        [Test]
        public void Error_Get_Action_Returns_View_Error_View_Result()
        {
            var result = _homeController.Error();

            Assert.That(result, Is.TypeOf<ViewResult>());
            Assert.That(((ViewResult)result).ViewName, Is.EqualTo("Error"));
        }

        [Test]
        public void Search_Get_Action_Returns_View_Result_With_Search_Engine_Name()
        {
            _mockConfig.Setup(_ => _[It.IsAny<string>()]).Returns("GoogleClient");
            _mockConfig.Setup(_ => _["HttpClientSettings:GoogleClient:SearchEngineName"]).Returns("Google Search");

            var result = _homeController.Index(new SearchViewModel { RankString = "" });

            Assert.That(result, Is.TypeOf<ViewResult>());
            Assert.That(((ViewResult)result).Model, Is.TypeOf<SearchViewModel>());
            var searchViewModel = (SearchViewModel) ((ViewResult) result).Model;

            Assert.That(searchViewModel.SearchEngineName, Is.EqualTo("Google Search"));
        }

        [Test]
        public async Task Search_Post_Action_Returns_Redirect_To_Action_Result_With_Rank_String()
        {
            var result = await _homeController.Search(new Search
            {
                SearchKeywords = "online title search",
                UrlToSearch = "http://www.infotrack.com.au"
            });

            var redirectToActionResult = (RedirectToActionResult) result;
            
            Assert.That(result, Is.TypeOf<RedirectToActionResult>());
            Assert.That(redirectToActionResult.ActionName, Is.EqualTo("Index"));
        }

        [Test]
        public void Set_Language_Post_Action_Returns_Redirect_To_Action_Result()
        {
            var result = _homeController.SetLanguage(new SearchViewModel
            {
                SearchRegion = "en-AU"
            });

            var redirectToActionResult = (RedirectToActionResult) result;
            
            Assert.That(result, Is.TypeOf<RedirectToActionResult>());
            Assert.That(redirectToActionResult.ActionName, Is.EqualTo("Index"));
            Assert.That(Thread.CurrentThread.CurrentCulture.Name, Is.EqualTo("en-AU"));
        }

        [Test]
        public async Task GetUrlRanksFromSearchResults_Method_In_Search_Post_Action_Throws_Exception()
        {
            _mockRankCalculator.Setup(_ => _.GetUrlRanksFromSearchResults(It.IsAny<string>(), It.IsAny<Uri>()))
                .Throws<ArgumentException>();

            var result = await _homeController.Search(new Search
            {
                SearchKeywords = "online title search",
                UrlToSearch = "http://www.infotrack.com.au"
            });

            var redirectToActionResult = (RedirectToActionResult) result;
            
            Assert.That(result, Is.TypeOf<RedirectToActionResult>());
            Assert.That(redirectToActionResult.ActionName, Is.EqualTo("Error"));
        }

        [Test]
        public async Task GetSearchResults_Method_In_Search_Post_Action_Throws_Exception()
        {
            _mockCrawlService.Setup(_ => _.GetSearchResults(It.IsAny<string>()))
                .Throws<HttpRequestException>();

            var result = await _homeController.Search(new Search
            {
                SearchKeywords = "online title search",
                UrlToSearch = "http://www.infotrack.com.au"
            });

            var redirectToActionResult = (RedirectToActionResult) result;
            
            Assert.That(result, Is.TypeOf<RedirectToActionResult>());
            Assert.That(redirectToActionResult.ActionName, Is.EqualTo("Error"));
        }

        [Test]
        public async Task Search_Post_Action_Returns_Error_View_When_Model_State_Invalid()
        {
            _homeController.ModelState.AddModelError("Dummy Error", "Dummy Error Message");

            var result = await _homeController.Search(new Search
            {
                SearchKeywords = "online title search",
                UrlToSearch = "http://www.infotrack.com.au"
            });

            var viewResult = (ViewResult) result;
            
            Assert.That(result, Is.TypeOf<ViewResult>());
            Assert.That(viewResult.ViewName, Is.EqualTo("Index"));
            _mockLogger.VerifyAll();
        }

        [Test]
        public async Task Search_Post_Action_Returns_Error_View_When_Uri_Is_Invalid()
        {
            var result = await _homeController.Search(new Search
            {
                SearchKeywords = "online title search",
                UrlToSearch = "http:wwwinfotrack.com.au"
            });

            var viewResult = (ViewResult) result;
            
            Assert.That(result, Is.TypeOf<ViewResult>());
            Assert.That(viewResult.ViewName, Is.EqualTo("Index"));
            _mockLogger.VerifyAll();
        }
    }
}
