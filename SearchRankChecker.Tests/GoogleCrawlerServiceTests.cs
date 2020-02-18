using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using SearchRankChecker.Business.Interfaces;
using SearchRankChecker.Business.Services;

namespace SearchRankChecker.Tests
{
    [TestFixture]
    public class GoogleCrawlerServiceTests
    {
        private Mock<ILogger<GoogleCrawlerService>> _logger;
        private Mock<IConfiguration> _mockConfig;

        [SetUp]
        public void Setup()
        {
            _logger = new Mock<ILogger<GoogleCrawlerService>>();
            _mockConfig = new Mock<IConfiguration>();
        }

        [Test]
        public async Task Test_Get_Search_Results_Success()
        {
            // ARRANGE
            var expectedSearchResultString =
                "<div class=\"r\"><a href=\"http://www.infotrack.com.au\">Test Dummy Data</a></div>";

            var httpResponseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(expectedSearchResultString)
            };

            var handlerMockObject = GetHandlerMock(httpResponseMessage);

            var mockIHttpClientFactoryObject = GetMockIHttpClientFactory(handlerMockObject);

            // ACT

            ICrawlerService crawlerService = new GoogleCrawlerService(mockIHttpClientFactoryObject, _logger.Object, _mockConfig.Object);
            
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
                "<div class=\"r\"><a href=\"http://www.infotrack.com.au\">Test Dummy Data</a></div>";

            var httpResponseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest,
                Content = new StringContent(expectedSearchResultString)
            };

            var handlerMockObject = GetHandlerMock(httpResponseMessage);

            var mockIHttpClientFactoryObject = GetMockIHttpClientFactory(handlerMockObject);

            // ACT

            ICrawlerService crawlerService = new GoogleCrawlerService(mockIHttpClientFactoryObject, _logger.Object, _mockConfig.Object);
            
            // ASSERT

            Assert.ThrowsAsync<HttpRequestException>(async () =>
                await crawlerService.GetSearchResults("online title search"), "Bad Request");
        }

        [Test]
        public void Test_Get_Search_Results_Without_Providing_Search_Terms()
        {
            // ARRANGE
            var expectedSearchResultString =
                "<div class=\"r\"><a href=\"http://www.infotrack.com.au\">Test Dummy Data</a></div>";

            var httpResponseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest,
                Content = new StringContent(expectedSearchResultString)
            };

            var handlerMockObject = GetHandlerMock(httpResponseMessage);

            var mockIHttpClientFactoryObject = GetMockIHttpClientFactory(handlerMockObject);

            // ACT

            ICrawlerService crawlerService = new GoogleCrawlerService(mockIHttpClientFactoryObject, _logger.Object, _mockConfig.Object);
            
            // ASSERT

            Assert.ThrowsAsync<ArgumentException>(async () =>
                await crawlerService.GetSearchResults(""), "Search terms must be provided!");
        }

        [Test]
        public async Task Test_Get_Search_Results_With_Max_Search_Search_Results_Config()
        {
            // ARRANGE
            var expectedSearchResultString =
                "<div class=\"r\"><a href=\"http://www.infotrack.com.au\">Test Dummy Data</a></div>";

            _mockConfig.SetupGet(c => c["HttpClients:GoogleClient:MaxSearchResults"])
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

            ICrawlerService crawlerService = new GoogleCrawlerService(mockIHttpClientFactoryObject, _logger.Object, _mockConfig.Object);
            var searchResults = await crawlerService.GetSearchResults("online title search");
            
            // ASSERT
            _mockConfig.Verify(mock => mock["HttpClients:GoogleClient:MaxSearchResults"], Times.Once);

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
