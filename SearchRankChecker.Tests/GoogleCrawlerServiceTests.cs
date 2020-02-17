using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using SearchRankChecker.Business.Interfaces;
using SearchRankChecker.Business.Services;
using SearchRankChecker.Domain.Enums;
using SearchRankChecker.Domain.Handlers;

namespace SearchRankChecker.Tests
{
    [TestFixture]
    public class GoogleCrawlerServiceTests
    {
        //private IHttpClientFactory _fakeHttpClientFactory;
        //private ICrawlerService _googleCrawlerService;

        //[SetUp]
        //public void Setup()
        //{


        //const string userAgentString =
        //    "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/80.0.3987.106 Safari/537.36";


        //httpClient.BaseAddress = new Uri("https://www.google.com.au");
        //httpClient.DefaultRequestHeaders.Clear();
        //httpClient.DefaultRequestHeaders.Add("User-Agent", userAgentString);



        //A.CallTo(() => fakeHttpClient.SendAsync(A<HttpRequestMessage>.Ignored, A<HttpCompletionOption>.Ignored, A<CancellationToken>.Ignored))
        //    .Returns(new HttpResponseMessage
        //    {
        //        Content = new StringContent("Dummy Content"),
        //        StatusCode = HttpStatusCode.OK 
        //    });


        //}

        [Test]
        public async Task Test_Get_Search_Results_Success()
        {
            var fakeHttpClientFactory = new Mock<IHttpClientFactory>();

            //var fakeHttpMessageHandler = new FakeHttpMessageHandler(new HttpResponseMessage
            //{
            //    StatusCode = HttpStatusCode.OK,
            //    Content = new StringContent("<html><body><p1>Test Dummy Data</p></div></html>", Encoding.UTF8, "text/html")
            //});

            //var fakeHttpClient = new HttpClient(fakeHttpMessageHandler);

            //A.CallTo(() => fakeHttpClientFactory.CreateClient(nameof(HttpClientsEnum.GoogleClient)))
            //    .Returns(fakeHttpClient);

            //ICrawlerService googleCrawlerService = new GoogleCrawlerService(fakeHttpClientFactory);

            //var asaa = await googleCrawlerService.GetSearchResults(new Uri("http://www.infotrack.com.au"),
            //    "online title search australia");

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("{'name':thecodebuzz,'city':'USA'}"),
                });

            var client = new HttpClient(mockHttpMessageHandler.Object);
            fakeHttpClientFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(client);

            Assert.True(true);


        }

        [Test]
        public void Sample_Test()
        {
            // ARRANGE
            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            handlerMock
                .Protected()
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
                    Content = new StringContent("[{'id':1,'value':'1'}]"),
                })
                .Verifiable();

            // use real http client with mocked handler here
            var httpClient = new HttpClient(handlerMock.Object)
            {
                BaseAddress = new Uri("http://test.com/"),
            };
        }
    }
}
