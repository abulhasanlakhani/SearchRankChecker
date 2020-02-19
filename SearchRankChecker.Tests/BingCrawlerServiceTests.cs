using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using SearchRankChecker.Business.Interfaces;
using SearchRankChecker.Business.Services;
using SearchRankChecker.Domain.Models;

namespace SearchRankChecker.Tests
{
    [TestFixture]
    public class BingCrawlerServiceTests
    {
        private Mock<ILogger<CrawlerService>> _logger;
        private Mock<IConfiguration> _mockConfig;
        private Mock<IOptionsSnapshot<AppSettings>> _mockOptions;
        private AppSettings _appSettings;

        [SetUp]
        public void Setup()
        {
            _logger = new Mock<ILogger<CrawlerService>>();
            _mockConfig = new Mock<IConfiguration>();
            _appSettings = new AppSettings {SelectedHttpClient = "BingClient"};
            _mockOptions = new Mock<IOptionsSnapshot<AppSettings>>();

            _mockOptions.Setup(_ => _.Value).Returns(_appSettings);
        }

        [Test]
        public async Task Test_Get_Search_Results_Success()
        {
            // ARRANGE
            var expectedSearchResultString =
                "<li class=\"b_algo\"><h2><a href=\"http://www.infotrack.com.au\"></a></h2>";

            var httpResponseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(expectedSearchResultString)
            };

            var handlerMockObject = GetHandlerMock(httpResponseMessage);

            var mockIHttpClientFactoryObject = GetMockIHttpClientFactory(handlerMockObject);

            // ACT

            ICrawlerService crawlerService = new CrawlerService(mockIHttpClientFactoryObject, _logger.Object, _mockConfig.Object, _mockOptions.Object);
            
            var actualSearchResults = await crawlerService.GetSearchResults("online title search");

            // ASSERT

            Assert.That(actualSearchResults, Is.Not.Empty);
            Assert.That(actualSearchResults, Is.EqualTo(expectedSearchResultString));
        }

        [Test]
        public void Test_Get_Search_Results_Not_Success_Status_Code()
        {
            // ARRANGE
            var expectedSearchResultString =
                "<li class=\"b_algo\"><h2><a href=\"http://www.infotrack.com.au\"></a></h2>";

            var httpResponseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest,
                Content = new StringContent(expectedSearchResultString)
            };

            var handlerMockObject = GetHandlerMock(httpResponseMessage);

            var mockIHttpClientFactoryObject = GetMockIHttpClientFactory(handlerMockObject);

            // ACT

            ICrawlerService crawlerService = new CrawlerService(mockIHttpClientFactoryObject, _logger.Object, _mockConfig.Object, _mockOptions.Object);
            
            // ASSERT

            Assert.ThrowsAsync<HttpRequestException>(async () =>
                await crawlerService.GetSearchResults("online title search"), "Bad Request");
        }

        [Test]
        public void Test_Get_Search_Results_Without_Providing_Search_Terms()
        {
            // ARRANGE
            var expectedSearchResultString =
                "<li class=\"b_algo\"><h2><a href=\"http://www.infotrack.com.au\"></a></h2>";

            var httpResponseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest,
                Content = new StringContent(expectedSearchResultString)
            };

            var handlerMockObject = GetHandlerMock(httpResponseMessage);

            var mockIHttpClientFactoryObject = GetMockIHttpClientFactory(handlerMockObject);

            // ACT

            ICrawlerService crawlerService = new CrawlerService(mockIHttpClientFactoryObject, _logger.Object, _mockConfig.Object, _mockOptions.Object);
            
            // ASSERT

            Assert.ThrowsAsync<ArgumentException>(async () =>
                await crawlerService.GetSearchResults(""), "Search terms must be provided!");
        }

        [Test]
        public void Test_Get_Search_Results_Without_Selected_Client_Setting()
        {
            // ARRANGE
            _appSettings = new AppSettings {SelectedHttpClient = ""};

            _mockOptions.Setup(_ => _.Value).Returns(_appSettings);
            
            var expectedSearchResultString =
                "<li class=\"b_algo\"><h2><a href=\"http://www.infotrack.com.au\"></a></h2>";

            var httpResponseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(expectedSearchResultString)
            };

            var handlerMockObject = GetHandlerMock(httpResponseMessage);

            var mockIHttpClientFactoryObject = GetMockIHttpClientFactory(handlerMockObject);

            // ACT

            ICrawlerService crawlerService = new CrawlerService(mockIHttpClientFactoryObject, _logger.Object, _mockConfig.Object, _mockOptions.Object);
            
            // ASSERT

            Assert.ThrowsAsync<ArgumentException>(async () =>
                await crawlerService.GetSearchResults("online title search"), "Default HttpClient should be set in the config");
        }

        [Test]
        public async Task Test_Get_Search_Results_With_Max_Search_Search_Results_Config()
        {
            // ARRANGE
            var expectedSearchResultString =
                "<li class=\"b_algo\"><h2><a href=\"http://www.infotrack.com.au\"></a></h2>";

            _mockConfig.SetupGet(c => c[$"HttpClientSettings:{_appSettings.SelectedHttpClient}:MaxSearchResults"])
                .Returns("100")
                .Verifiable();

            var httpResponseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(expectedSearchResultString)
            };

            var handlerMockObject = GetHandlerMock(httpResponseMessage);

            var mockIHttpClientFactoryObject = GetMockIHttpClientFactory(handlerMockObject);

            // ACT

            ICrawlerService crawlerService = new CrawlerService(mockIHttpClientFactoryObject, _logger.Object, _mockConfig.Object, _mockOptions.Object);
            var searchResults = await crawlerService.GetSearchResults("online title search");
            
            // ASSERT
            _mockConfig.Verify(mock => mock[$"HttpClientSettings:{_appSettings.SelectedHttpClient}:MaxSearchResults"], Times.Once);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="httpResponseMessage"></param>
        /// <returns></returns>
        private HttpMessageHandler GetHandlerMock(HttpResponseMessage httpResponseMessage)
        {
            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            handlerMock.Protected()
                // Setup the PROTECTED method to mock
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                // prepare the expected response of the mocked http call
                .ReturnsAsync(httpResponseMessage)
                .Verifiable();

            return handlerMock.Object;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="handlerMockObject"></param>
        /// <returns></returns>
        private IHttpClientFactory GetMockIHttpClientFactory(HttpMessageHandler handlerMockObject)
        {
            // use real http client with mocked handler here
            var httpClient = new HttpClient(handlerMockObject)
            {
                BaseAddress = new Uri("http://test.com/"),
            };

            var mockHttpClientFactory = new Mock<IHttpClientFactory>();

            mockHttpClientFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(httpClient);

            return mockHttpClientFactory.Object;
        }
    }
}
