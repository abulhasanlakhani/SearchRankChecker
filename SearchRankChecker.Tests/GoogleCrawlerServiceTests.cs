using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
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
        [Test]
        public async Task Test_Get_Search_Results_Success()
        {
            // ARRANGE
            var expectedSearchResultString =
                "<div class=\"r\"><a href=\"http://www.infotrack.com.au\">Test Dummy Data</a></div>";

            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            handlerMock.Protected()
                // Setup the PROTECTED method to mock
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                // prepare the expected response of the mocked http call
                .ReturnsAsync(new HttpResponseMessage()
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(expectedSearchResultString),
                })
                .Verifiable();

            // use real http client with mocked handler here
            var httpClient = new HttpClient(handlerMock.Object)
            {
                BaseAddress = new Uri("http://test.com/"),
            };

            var mockHttpClientFactory = new Mock<IHttpClientFactory>();

            mockHttpClientFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(httpClient);

            // ACT

            ICrawlerService crawlerService = new GoogleCrawlerService(mockHttpClientFactory.Object);
            
            var actualSearchResults = await crawlerService.GetSearchResults("online title search");

            // ASSERT

            Assert.That(actualSearchResults, Is.Not.Empty);
            Assert.That(actualSearchResults, Is.EqualTo(expectedSearchResultString));
        }
    }
}
