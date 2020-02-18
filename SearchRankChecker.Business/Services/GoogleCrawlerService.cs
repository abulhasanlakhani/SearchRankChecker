using System;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SearchRankChecker.Business.Interfaces;
using SearchRankChecker.Domain.Enums;

namespace SearchRankChecker.Business.Services
{
    public class GoogleCrawlerService : ICrawlerService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<GoogleCrawlerService> _logger;
        public IConfiguration Configuration { get; }

        public GoogleCrawlerService(IHttpClientFactory httpClientFactory, ILogger<GoogleCrawlerService> logger, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            Configuration = configuration;
        }

        /// <summary>
        /// Uses HttpClient to crawl Google Search and bring the results as string
        /// </summary>
        /// <param name="searchTerms"></param>
        /// <exception cref="WebException"></exception>
        /// <exception cref="HttpRequestException"></exception>
        /// <exception cref="SocketException"></exception>
        /// <returns></returns>
        public async Task<string> GetSearchResults(string searchTerms)
        {
            if (string.IsNullOrEmpty(searchTerms))
                throw new ArgumentException("Search terms must be provided!");

            var query = BuildCrawlQuery(searchTerms);
            var httpClient = _httpClientFactory.CreateClient(nameof(HttpClientsEnum.GoogleClient));

            var request = new HttpRequestMessage(HttpMethod.Get, query);

            try
            {
                using var response = await httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadAsStringAsync();
            }
            catch (Exception e) when (e is WebException || e is HttpRequestException || e is SocketException)
            {
                _logger.LogError(e, "Exception occured while processing the http request!");
                throw;
            }
        }

        /// <summary>
        /// Build the query to be sent to google search
        /// </summary>
        /// <param name="searchTerms"></param>
        /// <returns>Search query with encoded search terms</returns>
        private string BuildCrawlQuery(string searchTerms)
        {
            var encodedSearchTerm = HttpUtility.UrlEncode(searchTerms);

            var searchQuery = $"search?q={encodedSearchTerm}";
            var maxSearchResultsConfig = Configuration["HttpClients:GoogleClient:MaxSearchResults"];

            if (!string.IsNullOrEmpty(maxSearchResultsConfig))
            {
                searchQuery += $"&num={maxSearchResultsConfig}";
            }
            
            return searchQuery;
        }
    }
}
