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
        private HomeController _homeController;


        [SetUp]
        public void Setup()
        {
            _mockCrawlService = new Mock<ICrawlerService>();
            _mockRankCalculator = new Mock<IRankCalculator>();
            _mockLogger = new Mock<ILogger<HomeController>>();

            _homeController = new HomeController(_mockCrawlService.Object, _mockRankCalculator.Object, _mockLogger.Object);
        }

        [Test]
        public void Search_Get_Action_Returns_View_Result_With_Empty_Rank_String()
        {
            var result = _homeController.Index(new SearchViewModel { RankString = "" });

            Assert.That(result, Is.TypeOf<ViewResult>());
        }

        [Test]
        public async Task Search_Post_Action_Returns_View_Result_With_Rank_String()
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
